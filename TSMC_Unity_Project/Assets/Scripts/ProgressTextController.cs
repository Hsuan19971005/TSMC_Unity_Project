using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressTextController : MonoBehaviour
{
    Text floor_name_;
    Text wall_total_number_;
    Text wall_finished_number_;
    Text wall_ratio_number_;
    Text column_total_number_;
    Text column_finished_number_;
    Text column_ratio_number_;
    Text slab_total_number_;
    Text slab_finished_number_;
    Text slab_ratio_number_;
    // Start is called before the first frame update
    private void Awake()
    {
        floor_name_ = GameObject.Find("Floor_Name").GetComponent<Text>();
        wall_total_number_ = GameObject.Find("Wall_Total_Number").GetComponent<Text>();
        wall_finished_number_ = GameObject.Find("Wall_Finished_Number").GetComponent<Text>();
        wall_ratio_number_ = GameObject.Find("Wall_Ratio_Number").GetComponent<Text>();
        column_total_number_ = GameObject.Find("Column_Total_Number").GetComponent<Text>();
        column_finished_number_ = GameObject.Find("Column_Finished_Number").GetComponent<Text>();
        column_ratio_number_ = GameObject.Find("Column_Ratio_Number").GetComponent<Text>();
        slab_total_number_ = GameObject.Find("Slab_Total_Number").GetComponent<Text>();
        slab_finished_number_ = GameObject.Find("Slab_Finished_Number").GetComponent<Text>();
        slab_ratio_number_ = GameObject.Find("Slab_Ratio_Number").GetComponent<Text>();
    }
    void Start()
    {
        var progress = new ConstructionProgress();
        int index_of_floor = DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName();
        //int index_of_user_input_floor_name = progress.FindFloorDataIndex(DataBase.chosen_floor_name_, DataBase.ifc_floor_data_);
        floor_name_.text = DataBase.chosen_floor_name_;
        wall_total_number_.text = Convert.ToString(DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcWallStandardCase_.id_name_.Count);
        wall_finished_number_.text = Convert.ToString(progress.CountOneFloorWallExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_));
        wall_ratio_number_.text = Convert.ToString(progress.CountOneFloorWallExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_) / (float)DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcWallStandardCase_.id_name_.Count * 100f) + " %";

        column_total_number_.text = Convert.ToString(DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcColumn_.id_name_.Count);
        column_finished_number_.text = Convert.ToString(progress.CountOneFloorColumnExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_));
        column_ratio_number_.text = Convert.ToString(progress.CountOneFloorColumnExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_) / (float)DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcColumn_.id_name_.Count * 100f) + " %";

        slab_total_number_.text = Convert.ToString(DataBase.ifc_floor_data_[index_of_floor].WorkItemDataGrid_.id_name_.Count);
        slab_finished_number_.text = Convert.ToString(progress.CountOneFloorGridExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_));
        slab_ratio_number_.text = Convert.ToString(progress.CountOneFloorGridExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_) / (float)DataBase.ifc_floor_data_[index_of_floor].WorkItemDataGrid_.id_name_.Count * 100f) + " %";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
