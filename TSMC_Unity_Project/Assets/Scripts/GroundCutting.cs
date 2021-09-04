using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

public class GroundCutting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public List<Line> vertical_grid_lines_ { get; set; }
    public List<Line> horizontal_grid_lines_ { get; set; }
    public List<float[]> ground_points_ { get; set; }//地面的所有尚未分割的點
    public List<float[]>[] grids_ { get; set; }//儲存分割後的點
    public List<ClassifiedPointCloudData> grid_point_cloud_data_ { get; set; }//儲存依照分割點雲網格創造出的ClassifiedPointCloudData
    public List<string> all_id_;
    public GroundCutting()
    {
        this.vertical_grid_lines_ = new List<Line>();
        this.horizontal_grid_lines_ = new List<Line>();
        this.ground_points_ = new List<float[]>();
        this.grid_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.all_id_ = new List<string>();
    }
    public GroundCutting(List<float[]> ground_points)
    {
        this.vertical_grid_lines_ = new List<Line>();
        this.horizontal_grid_lines_ = new List<Line>();
        this.ground_points_ = new List<float[]>();
        this.grid_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.all_id_ = new List<string>();
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
    public void SetGroundPoints(List<float[]> ground_points)
    {
        this.ground_points_ = ground_points;
    }
    public void SetGroundPointsInGrid()
    {
        int x_grid_num = this.vertical_grid_lines_.Count - 1;
        int y_grid_num = this.horizontal_grid_lines_.Count - 1;
        //設定gird_的大小及創建空間
        this.grids_ = new List<float[]>[x_grid_num * y_grid_num];
        for (int i = 0; i < x_grid_num * y_grid_num; i++) this.grids_[i] = new List<float[]>();
        //將每一個點放入指定的Grid
        for (int i = 0; i < this.ground_points_.Count; i++)
        {
            int index_x = SetIndexOfPointInGrid(this.ground_points_[i], this.vertical_grid_lines_);
            int index_y = SetIndexOfPointInGrid(this.ground_points_[i], this.horizontal_grid_lines_);
            //Console.Write(i+"  x:"+index_x+"y:"+index_y+"   ");
            this.grids_[index_x + index_y * x_grid_num].Add(this.ground_points_[i]);
        }

    }
    public float Cross(Line line, float[] point3)
    {
        return (line.point2_x_ - line.point1_x_) * (point3[1] - line.point1_y_) - (line.point2_y_ - line.point1_y_) * (point3[0] - line.point1_x_);
    }
    public int SetIndexOfPointInGrid(float[] point, List<Line> grid_lines)
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
    public void SetGridsPointCloudData()
    {
        for (int i = 0; i < this.grids_.Length; i++)
        {
            ClassifiedPointCloudData point_cloud_data = new ClassifiedPointCloudData();
            point_cloud_data.points_ = this.grids_[i];
            point_cloud_data.SetMaxMin();
            point_cloud_data.id_name_ = "Grids" + i;
            this.all_id_.Add("Grids" + i);
            this.grid_point_cloud_data_.Add(point_cloud_data);
        }
    }
}
