using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassifiedPointCloudData
{
    public string id_name_ { get; set; }
    public bool exist_ { get; set; }
    public float x_max_ { get; set; }
    public float x_min_ { get; set; }
    public float y_max_ { get; set; }
    public float y_min_ { get; set; }
    public float z_max_ { get; set; }
    public float z_min_ { get; set; }
    public List<float[]> points_ { get; set; }
    public List<float[]> four_corner_points { get; set; }
    public ClassifiedPointCloudData()
    {
        this.points_ = new List<float[]>();
        this.four_corner_points = new List<float[]>();
        this.exist_ = false;
    }
    public void SetMaxMin(List<float[]> points)
    {
        if (points.Count <= 1) return;
        this.x_max_ = points[0][0];
        this.x_min_ = points[0][0];
        this.y_max_ = points[0][1];
        this.y_min_ = points[0][1];
        this.z_max_ = points[0][2];
        this.z_min_ = points[0][2];
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i][0] > this.x_max_) this.x_max_ = points[i][0];
            if (points[i][0] < this.x_min_) this.x_min_ = points[i][0];
            if (points[i][1] > this.y_max_) this.y_max_ = points[i][1];
            if (points[i][1] < this.y_min_) this.y_min_ = points[i][1];
            if (points[i][2] > this.z_max_) this.z_max_ = points[i][2];
            if (points[i][2] < this.z_min_) this.z_min_ = points[i][2];
        }
    }
    public void SetMaxMin()
    {
        if (this.points_.Count <= 1) return;
        this.x_max_ = this.points_[0][0];
        this.x_min_ = this.points_[0][0];
        this.y_max_ = this.points_[0][1];
        this.y_min_ = this.points_[0][1];
        this.z_max_ = this.points_[0][2];
        this.z_min_ = this.points_[0][2];
        for (int i = 1; i < this.points_.Count; i++)
        {
            if (this.points_[i][0] > this.x_max_) this.x_max_ = this.points_[i][0];
            if (this.points_[i][0] < this.x_min_) this.x_min_ = this.points_[i][0];
            if (this.points_[i][1] > this.y_max_) this.y_max_ = this.points_[i][1];
            if (this.points_[i][1] < this.y_min_) this.y_min_ = this.points_[i][1];
            if (this.points_[i][2] > this.z_max_) this.z_max_ = this.points_[i][2];
            if (this.points_[i][2] < this.z_min_) this.z_min_ = this.points_[i][2];
        }
    }
}
