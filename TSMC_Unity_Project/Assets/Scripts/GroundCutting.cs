using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

public class GroundCutting : MonoBehaviour
{
    public List<Line> vertical_grid_lines_ { get; set; }
    public List<Line> horizontal_grid_lines_ { get; set; }
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
        this.vertical_grid_lines_.Insert(0, new Line(-999999999f, -999999999f, -999999999f, 999999999f));
        this.vertical_grid_lines_.Add(new Line(999999999f, -999999999f, 999999999f, 999999999f));
        this.horizontal_grid_lines_.Insert(0, new Line(-999999999f, 999999999f, 999999999f, 999999999f));
        this.horizontal_grid_lines_.Add(new Line(-999999999f, -999999999f, 999999999f, -999999999f));
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
            this.grids_containing_points_[index_x + index_y * x_grid_num].Add(this.unclassified_ground_points_[i]);
        }
    }
    public int FindIndexOfPointInGrid(float[] point, List<Line> grid_lines)
    {
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
    public void PassGroundCuttingMemberDataToDataBase(List<ClassifiedPointCloudData>classified_point_cloud_data,WorkItemData work_item_data_grid)
    {
        classified_point_cloud_data.AddRange(this.grid_classified_point_cloud_data_);
        work_item_data_grid = this.work_item_data_grid_;
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

}
