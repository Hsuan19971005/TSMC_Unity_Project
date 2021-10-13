using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GroundCutting 
{
    public List<Line> vertical_grid_lines_ { get; set; }//紀錄垂直網格線，高到低排列
    public List<Line> horizontal_grid_lines_ { get; set; }//紀錄水平網格線，左到右排列
    public List<float[]> unclassified_ground_points_ { get; set; }//地面的所有尚未分割的點
    public List<float[]>[] grids_containing_points_ { get; set; }//儲存分割後的點
    public List<ClassifiedPointCloudData> grid_classified_point_cloud_data_ { get; set; }//儲存依照分割點雲網格創造出的ClassifiedPointCloudData
    public WorkItemData work_item_data_grid_ { get; set; }
    public List<string> all_id_ { get; set; }
    public GroundCutting()
    {
        this.vertical_grid_lines_ = new List<Line>();
        this.horizontal_grid_lines_ = new List<Line>();
        this.unclassified_ground_points_ = new List<float[]>();
        this.grid_classified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.work_item_data_grid_ = new WorkItemData(623);
    }
    public GroundCutting(List<float[]> ground_points)
    {
        this.vertical_grid_lines_ = new List<Line>();
        this.horizontal_grid_lines_ = new List<Line>();
        this.unclassified_ground_points_ = new List<float[]>();
        this.grid_classified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.work_item_data_grid_ = new WorkItemData(623);
        SetGroundPoints(ground_points);
    }

    public void SetAllGridLines(List<Line> unclassifid_lines)
    {
        for (int i = 0; i < unclassifid_lines.Count; i++)
        {
            float x_span = Math.Abs(unclassifid_lines[i].point1_x_ - unclassifid_lines[i].point2_x_);
            float y_span = Math.Abs(unclassifid_lines[i].point1_y_ - unclassifid_lines[i].point2_y_);
            if (x_span <= y_span) SetVerticalGridLines(unclassifid_lines[i], this.vertical_grid_lines_);
            else SetHorizontalGridLines(unclassifid_lines[i], this.horizontal_grid_lines_);
        }
        //this.vertical_grid_lines_.Insert(0, new Line(-999999999f, -999999999f, -999999999f, 999999999f));
        //this.vertical_grid_lines_.Add(new Line(999999999f, -999999999f, 999999999f, 999999999f));
        //this.horizontal_grid_lines_.Insert(0, new Line(-999999999f, 999999999f, 999999999f, 999999999f));
        //this.horizontal_grid_lines_.Add(new Line(-999999999f, -999999999f, 999999999f, -999999999f));
    }
    public void CollectGroundPointsByOneIfcFloorData(FloorData one_floor_data)
    {
        //收集Slab的所有點
        foreach (var current_id_name in one_floor_data.WorkItemDataIfcSlab_.id_name_)
        {
            int index_of_current_id_name = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(current_id_name);
            if (index_of_current_id_name == -1) continue;
            for (int i = 0; i < DataBase.classified_point_cloud_data_[index_of_current_id_name].points_.Count; i++)
            {
                float[] copied_point = DataBase.classified_point_cloud_data_[index_of_current_id_name].points_[i];
                float[] new_point = new float[] { copied_point[0], copied_point[1], copied_point[2] };
                this.unclassified_ground_points_.Add(new_point);
            }
        }
        //收集BuildingElementProxy的所有點
        foreach (var current_id_name in one_floor_data.WorkItemDataIfcBuildingElementProxy_.id_name_)
        {
            int index_of_current_id_name = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(current_id_name);
            //Console.WriteLine("index_of_current_id_name:" + index_of_current_id_name + "     in SetGroundCuttingAllGroundPoints");
            if (index_of_current_id_name == -1) continue;
            for (int i = 0; i < DataBase.classified_point_cloud_data_[index_of_current_id_name].points_.Count; i++)
            {
                float[] copied_point = DataBase.classified_point_cloud_data_[index_of_current_id_name].points_[i];
                float[] new_point = new float[] { copied_point[0], copied_point[1], copied_point[2] };
                this.unclassified_ground_points_.Add(new_point);
            }
        }
    }
    public void SetGroundPoints(List<float[]> ground_points)
    {
        this.unclassified_ground_points_ = ground_points;
    }
    public void ClassifyGroundPointsIntoGridsContainingPoints()
    {
        int x_grid_num = this.vertical_grid_lines_.Count - 1;
        int y_grid_num = this.horizontal_grid_lines_.Count - 1;
        //設定gird_的大小及創建空間
        this.grids_containing_points_ = new List<float[]>[x_grid_num * y_grid_num];
        for (int i = 0; i < x_grid_num * y_grid_num; i++) this.grids_containing_points_[i] = new List<float[]>();
        //將每一個點放入指定的Grid
        for (int i = 0; i < this.unclassified_ground_points_.Count; i++)
        {
            int index_x = FindIndexOfPointInGrid(this.unclassified_ground_points_[i], this.vertical_grid_lines_);
            int index_y = FindIndexOfPointInGrid(this.unclassified_ground_points_[i], this.horizontal_grid_lines_);
            //點不在網格內
            if (index_x == -1 || index_y == -1) continue;
            //點在網格內
            this.grids_containing_points_[index_x + index_y * x_grid_num].Add(this.unclassified_ground_points_[i]);
        }
    }
    public int FindIndexOfPointInGrid(float[] point, List<Line> grid_lines)
    {
        //點位於最小網格線左邊 或 點位於最大網格線右邊 回傳-1
        if (Cross(grid_lines[0], point) > 0 || Cross(grid_lines[grid_lines.Count - 1], point) < 0) return -1;
        //二分搜尋法
        int low, high, mid;
        mid = 0;
        low = 1;
        high = grid_lines.Count - 1;
        while (low <= high)
        {
            mid = (low + high) / 2;
            if (Cross(grid_lines[mid], point) > 0) high = mid - 1;//在左
            else if (Cross(grid_lines[mid], point) < 0) low = mid + 1;//在右
            else return mid;
        }
        return low - 1;
    }
    public void SetGridClassifiedPointCloudData()
    {
        for (int i = 0; i < this.grids_containing_points_.Length; i++)
        {
            ClassifiedPointCloudData classified_point_cloud_data = new ClassifiedPointCloudData();
            classified_point_cloud_data.points_ = this.grids_containing_points_[i];
            classified_point_cloud_data.id_name_ = "Grid" + i;
            float[] point1 = new float[3], point2 = new float[3], point3 = new float[3], point4 = new float[3];
            if(IntersectionOfTwoLines(this.vertical_grid_lines_[i % (this.vertical_grid_lines_.Count-1)], this.horizontal_grid_lines_[i / (this.vertical_grid_lines_.Count-1) ], point1)==false)Debug.Log("point1平行");
            if (IntersectionOfTwoLines(this.vertical_grid_lines_[i % (this.vertical_grid_lines_.Count-1)+1], this.horizontal_grid_lines_[i / (this.vertical_grid_lines_.Count-1)], point2) == false) Debug.Log("point2平行");
            if (IntersectionOfTwoLines(this.vertical_grid_lines_[i % (this.vertical_grid_lines_.Count-1)+1], this.horizontal_grid_lines_[i / (this.vertical_grid_lines_.Count-1)+1], point3) == false) Debug.Log("point3平行");
            if (IntersectionOfTwoLines(this.vertical_grid_lines_[i % (this.vertical_grid_lines_.Count-1)], this.horizontal_grid_lines_[i / (this.vertical_grid_lines_.Count-1 )+1], point4) == false) Debug.Log("point4平行");
            classified_point_cloud_data.four_corner_points.Add(point1);
            classified_point_cloud_data.four_corner_points.Add(point2);
            classified_point_cloud_data.four_corner_points.Add(point3);
            classified_point_cloud_data.four_corner_points.Add(point4);
            this.grid_classified_point_cloud_data_.Add(classified_point_cloud_data);
        }
    }
    public void SetWorkItemDataGrid()
    {
        for(int i = 0; i < this.grids_containing_points_.Length; i++)
        {
            this.work_item_data_grid_.id_name_.Add("Grid" + i);
        }
    }
    public void SetHorizontalGridLines(Line line, List<Line> line_group)
    {
        line.SetInXOrder();
        if (line_group.Count == 0)
        {
            line_group.Add(line);
            return;
        }
        int insert_position = 0;
        //從高到低排列
        while (insert_position < line_group.Count)
        {
            if (line.mid_point_y_ <= line_group[insert_position].mid_point_y_) insert_position++;
            else
            {
                line_group.Insert(insert_position, line);
                return;
            }
        }
        line_group.Add(line);
    }
    public void SetVerticalGridLines(Line line, List<Line> line_group)
    {
        line.SetInYOrder();
        if (line_group.Count == 0)
        {
            line_group.Add(line);
            return;
        }
        int insert_position = 0;
        //從左到右排列
        while (insert_position < line_group.Count)
        {
            if (line.mid_point_x_ > line_group[insert_position].mid_point_x_) insert_position++;
            else
            {
                line_group.Insert(insert_position, line);
                return;
            }
        }
        line_group.Add(line);
    }
    public float Cross(Line line, float[] point3)
    {
        return (line.point2_x_ - line.point1_x_) * (point3[1] - line.point1_y_) - (line.point2_y_ - line.point1_y_) * (point3[0] - line.point1_x_);
    }
    public float Cross(float point1_x,float point1_y,float point2_x,float point2_y)
    {
        return point1_x * point2_y - point2_x * point1_y;
    }
    public bool IntersectionOfTwoLines(Line line1,Line line2, float[] return_one_point)
    {
        float[] vector_1 = new float[2] { line1.point2_x_ - line1.point1_x_, line1.point2_y_ - line1.point1_y_ };
        float[] vector_2 = new float[2] { line2.point2_x_ - line2.point1_x_, line2.point2_y_ - line2.point1_y_ };
        float[] vector_3 = new float[2] { line2.point1_x_ - line1.point1_x_, line2.point1_y_ - line1.point1_y_ };
        if (Cross(vector_1[0], vector_1[1], vector_2[0], vector_2[1]) == 0) return false;//兩線平行，交點不存在。兩線重疊，交點無限多
        return_one_point[0] = line1.point1_x_ + vector_1[0] * Cross(vector_3[0], vector_3[1], vector_2[0], vector_2[1]) / Cross(vector_1[0],vector_1[1],vector_2[0],vector_2[1]);
        return_one_point[1] = line1.point1_y_ + vector_1[1] * Cross(vector_3[0], vector_3[1], vector_2[0], vector_2[1]) / Cross(vector_1[0], vector_1[1], vector_2[0], vector_2[1]);
        return true;
    }



    //Get 系列
    public List<ClassifiedPointCloudData> GetGridClassifiedPointCloudData()
    {
        return this.grid_classified_point_cloud_data_;
    }
    public WorkItemData GetWorkItemDataGrid()
    {
        return this.work_item_data_grid_;
    }
}
