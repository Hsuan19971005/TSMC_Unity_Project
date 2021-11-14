using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    Image progress_bar_current_;
    Text progress_percent_number_;
    Text[] Remaining_Number_;//Hour, Minute, Second
    Text[] Estimated_Number_;//Hour, Minute, Second
    private int current_=0;
    private float max_classify_point_cloud_pregress_ = 95f;
    private bool is_progress_bar_turned_on_ = false;
    private float classify_seconds_per_million_points_ = 145f;

    private void Awake()
    {
        this.Remaining_Number_ = new Text[3];
        this.Remaining_Number_[0] = GameObject.Find("Remaining_Time_Title").transform.Find("Hour_number").GetComponent<Text>();
        this.Remaining_Number_[1] = GameObject.Find("Remaining_Time_Title").transform.Find("Minute_Number").GetComponent<Text>();
        this.Remaining_Number_[2] = GameObject.Find("Remaining_Time_Title").transform.Find("Second_Number").GetComponent<Text>();
        this.Estimated_Number_ = new Text[3];
        this.Estimated_Number_[0] = GameObject.Find("Estimated_Time_Title").transform.Find("Hour_number").GetComponent<Text>();
        this.Estimated_Number_[1] = GameObject.Find("Estimated_Time_Title").transform.Find("Minute_Number").GetComponent<Text>();
        this.Estimated_Number_[2] = GameObject.Find("Estimated_Time_Title").transform.Find("Second_Number").GetComponent<Text>();
        this.progress_bar_current_ = GameObject.Find("Progress_Bar_Current").GetComponent<Image>();
        this.progress_percent_number_ = GameObject.Find("Progress_Percent_Number").GetComponent<Text>();

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.current_=gameObject.GetComponent<Classifier>().index_of_classify_progress_;
        if (this.is_progress_bar_turned_on_ == true)
        {
            CalculateAndShowClassifyPointCloudPregress();
            CalculateAndShowRemainingTime();
        }
    }
    public void TurnOnProgressBar()
    {
        this.is_progress_bar_turned_on_ = true;
    }
    public void TurnOffProgressBar()
    {
        this.is_progress_bar_turned_on_ = false;
    }
    public void SetProgressBar100Percent()
    {
        this.progress_bar_current_.fillAmount=1;
        this.progress_percent_number_.text = "100";
        this.Remaining_Number_[0].text = "0";
        this.Remaining_Number_[1].text = "0";
        this.Remaining_Number_[2].text = "0";
        TurnOffProgressBar();
    }
    public void SetProgressBar95Percent()
    {
        this.progress_bar_current_.fillAmount = this.max_classify_point_cloud_pregress_/100f;
        this.progress_percent_number_.text = Convert.ToString(this.max_classify_point_cloud_pregress_);
        this.Remaining_Number_[0].text = "0";
        this.Remaining_Number_[1].text = "0";
        this.Remaining_Number_[2].text = "0";
        TurnOffProgressBar();
    }
    public void CalculateAndShowClassifyPointCloudPregress()
    {
        this.progress_bar_current_.fillAmount = (float)(this.current_+1) / (float)DataBase.real_points_.Count *this.max_classify_point_cloud_pregress_/100f;
        this.progress_percent_number_.text= Convert.ToString(Math.Round((float)(this.current_+1) / (float)DataBase.real_points_.Count * this.max_classify_point_cloud_pregress_,3));
    }
    public void ShowEstimatedTime()
    {
        float total_seconds = (float)DataBase.real_points_.Count / 1000000f * this.classify_seconds_per_million_points_;
        this.Estimated_Number_[0].text = Convert.ToString(Convert.ToInt32((total_seconds / 60f) / 60f));//Hour
        this.Estimated_Number_[1].text = Convert.ToString(Convert.ToInt32((total_seconds / 60f) % 60f));//Minute
        this.Estimated_Number_[2].text = Convert.ToString(Convert.ToInt32(total_seconds % 60f));//Seconds
    }
    public void CalculateAndShowRemainingTime()
    {
        float total_seconds = (float)(DataBase.real_points_.Count-this.current_-1) / 1000000f * this.classify_seconds_per_million_points_;
        this.Remaining_Number_[0].text = Convert.ToString(Convert.ToInt32((total_seconds / 60f) / 60f));//Hour
        this.Remaining_Number_[1].text = Convert.ToString(Convert.ToInt32((total_seconds / 60f) % 60f));//Minute 多加3分鐘給GroundCutting
        this.Remaining_Number_[2].text = Convert.ToString(Convert.ToInt32(total_seconds % 60f));//Seconds
    }
}
