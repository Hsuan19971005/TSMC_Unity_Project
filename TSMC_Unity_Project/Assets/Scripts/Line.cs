using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //line test
    public float point1_x_ { get; set; }
    public float point1_y_ { get; set; }
    public float point2_x_ { get; set; }
    public float point2_y_ { get; set; }
    public float mid_point_x_ { get; set; }
    public float mid_point_y_ { get; set; }
    public Line()
    {

    }
    public Line(float point1_x, float point1_y, float point2_x, float point2_y)
    {
        this.point1_x_ = point1_x;
        this.point1_y_ = point1_y;
        this.point2_x_ = point2_x;
        this.point2_y_ = point2_y;
        SetMidPoint();
    }
    public void SetMidPoint()
    {
        this.mid_point_x_ = (this.point1_x_ + this.point2_x_) / 2;
        this.mid_point_y_ = (this.point1_y_ + this.point2_y_) / 2;
    }
    public void ShowLine()
    {
        Debug.Log("(" + this.point1_x_ + ", " + this.point1_y_ + ") (" + this.point2_x_ + ", " + this.point2_y_ + ")");
    }
    public void SetInYOrder()
    {
        //point1的y要比較小
        if (this.point1_y_ > this.point2_y_)
        {
            float x = this.point1_x_;
            float y = this.point1_y_;
            this.point1_x_ = this.point2_x_;
            this.point1_y_ = this.point2_y_;
            this.point2_x_ = x;
            this.point2_y_ = y;
        }
    }
    public void SetInXOrder()
    {
        //point1的y要比較小
        if (this.point1_x_ > this.point2_x_)
        {
            float x = this.point1_x_;
            float y = this.point1_y_;
            this.point1_x_ = this.point2_x_;
            this.point1_y_ = this.point2_y_;
            this.point2_x_ = x;
            this.point2_y_ = y;
        }
    }
}
