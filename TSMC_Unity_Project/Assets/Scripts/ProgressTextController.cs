using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressTextController : MonoBehaviour
{
    public Transform UI_parent;//UI canvas for panel_1 and panel_1_recorded.
    public GameObject panel_1;//Panel for text when there's no recorded file.
    public GameObject panel_1_recorded;//Panel for text when there'a recorded file.
    private void Awake()
    {
    }
    void Start()
    {
        //With or without recorded file, call different tasks.
        if (DataBase.chosen_floor_name_ != DataBase.recorded_file_chosen_floor_name_) TasksOfOnlyCurrentProgress();
        else TasksOfCumulativeProgress();
    }

    void Update()
    {
        
    }
    public void TasksOfOnlyCurrentProgress()
    {
        GameObject panel_1 = Instantiate(this.panel_1,this.UI_parent) as GameObject;
        var progress = new ConstructionProgress();
        int index_of_floor = DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName();
        GameObject.Find("Floor_Name").GetComponent<Text>().text = DataBase.chosen_floor_name_;
        GameObject.Find("Wall_Total_Number").GetComponent<Text>().text = Convert.ToString(DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcWallStandardCase_.id_name_.Count);
        GameObject.Find("Wall_Finished_Number").GetComponent<Text>().text = Convert.ToString(progress.CountOneFloorWallExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_));
        GameObject.Find("Wall_Ratio_Number").GetComponent<Text>().text = Convert.ToString(Math.Round(progress.CountOneFloorWallExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_) / (float)DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcWallStandardCase_.id_name_.Count * 100f)) + " %";

        GameObject.Find("Column_Total_Number").GetComponent<Text>().text = Convert.ToString(DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcColumn_.id_name_.Count);
        GameObject.Find("Column_Finished_Number").GetComponent<Text>().text = Convert.ToString(progress.CountOneFloorColumnExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_));
        GameObject.Find("Column_Ratio_Number").GetComponent<Text>().text = Convert.ToString(Math.Round(progress.CountOneFloorColumnExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_) / (float)DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcColumn_.id_name_.Count * 100f)) + " %";

        GameObject.Find("Slab_Total_Number").GetComponent<Text>().text = Convert.ToString(DataBase.ifc_floor_data_[index_of_floor].WorkItemDataGrid_.id_name_.Count);
        GameObject.Find("Slab_Finished_Number").GetComponent<Text>().text = Convert.ToString(progress.CountOneFloorGridExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_));
        GameObject.Find("Slab_Ratio_Number").GetComponent<Text>().text = Convert.ToString(Math.Round(progress.CountOneFloorGridExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_) / (float)DataBase.ifc_floor_data_[index_of_floor].WorkItemDataGrid_.id_name_.Count * 100f)) + " %";
        //Fake**************************************************************************************************
        //GameObject.Find("Slab_Finished_Number").GetComponent<Text>().text = Convert.ToString(DataBase.ifc_floor_data_[index_of_floor].WorkItemDataGrid_.id_name_.Count);
        //GameObject.Find("Slab_Ratio_Number").GetComponent<Text>().text ="100 %";
    }
    public void TasksOfCumulativeProgress()
    {
        GameObject panel_1 = Instantiate(this.panel_1_recorded, this.UI_parent) as GameObject;
        var progress = new ConstructionProgress();
        int index_of_floor = DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName();
        GameObject.Find("Floor_Name").GetComponent<Text>().text = DataBase.chosen_floor_name_;
        //wall
        float wall_total=0, wall_new_finished=0, wall_new_ratio=0, wall_recorded_finished=0, wall_cumulative_ratio=0;
        wall_total= DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcWallStandardCase_.id_name_.Count;
        wall_recorded_finished = DataBase.recorded_file_wall_id_name_.Count;
        wall_new_finished = progress.CountOneFloorWallExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_);
        for (int i = 0; i < wall_recorded_finished; i++)
        {
            int index = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(DataBase.recorded_file_wall_id_name_[i]);
            if (index == -1) continue;
            else if(DataBase.classified_point_cloud_data_[index].exist_ == true) wall_new_finished--;
        }
        wall_new_ratio = (float)Math.Round(wall_new_finished / wall_total * 100f);
        wall_cumulative_ratio = (float)Math.Round((wall_new_finished + wall_recorded_finished) / wall_total *100f);
        GameObject.Find("Wall_Total_Number").GetComponent<Text>().text = Convert.ToString(wall_total);
        GameObject.Find("Wall_Finished_Number").GetComponent<Text>().text = Convert.ToString(wall_new_finished);
        GameObject.Find("Wall_Ratio_Number").GetComponent<Text>().text = Convert.ToString(wall_new_ratio) + " %";
        GameObject.Find("Wall_Cumulative_Finished_Number").GetComponent<Text>().text = Convert.ToString(wall_new_finished+wall_recorded_finished);
        GameObject.Find("Wall_Cumulative_Ratio_Number").GetComponent<Text>().text = Convert.ToString(wall_cumulative_ratio) + " %";

        //column
        float column_total = 0, column_new_finished = 0, column_new_ratio = 0, column_recorded_finished = 0, column_cumulative_ratio = 0;
        column_total = DataBase.ifc_floor_data_[index_of_floor].WorkItemDataIfcColumn_.id_name_.Count;
        column_recorded_finished = DataBase.recorded_file_column_id_name_.Count;
        column_new_finished = progress.CountOneFloorColumnExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_);
        for(int i = 0; i < column_recorded_finished; i++)
        {
            int index = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(DataBase.recorded_file_column_id_name_[i]);
            if (index == -1) continue;
            else if (DataBase.classified_point_cloud_data_[index].exist_ == true) column_new_finished--;
        }
        column_new_ratio = (float)Math.Round(column_new_finished / column_total * 100f);
        column_cumulative_ratio = (float)Math.Round((column_new_finished+column_recorded_finished)/column_total*100f);
        GameObject.Find("Column_Total_Number").GetComponent<Text>().text = Convert.ToString(column_total);
        GameObject.Find("Column_Finished_Number").GetComponent<Text>().text = Convert.ToString(column_new_finished);
        GameObject.Find("Column_Ratio_Number").GetComponent<Text>().text = Convert.ToString(column_new_ratio) + " %";
        GameObject.Find("Column_Cumulative_Finished_Number").GetComponent<Text>().text = Convert.ToString(column_new_finished+column_recorded_finished);
        GameObject.Find("Column_Cumulative_Ratio_Number").GetComponent<Text>().text = Convert.ToString(column_cumulative_ratio) + " %";

        //slab
        float slab_total = 0, slab_new_finished = 0, slab_new_ratio = 0, slab_recorded_finished = 0, slab_cumulative_ratio = 0;
        slab_total = DataBase.ifc_floor_data_[index_of_floor].WorkItemDataGrid_.id_name_.Count;
        slab_recorded_finished = DataBase.recorded_file_grid_id_name_.Count;
        slab_new_finished = progress.CountOneFloorGridExistNumber(DataBase.ifc_floor_data_[index_of_floor], DataBase.classified_point_cloud_data_);
        for (int i = 0; i < slab_recorded_finished; i++)
        {
            int index = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(DataBase.recorded_file_grid_id_name_[i]);
            if (index == -1) continue;
            else if (DataBase.classified_point_cloud_data_[index].exist_ == true) slab_new_finished--;
        }
        slab_new_ratio = (float)Math.Round(slab_new_finished / slab_total * 100f);
        slab_cumulative_ratio = (float)Math.Round((slab_new_finished + slab_recorded_finished) / slab_total * 100f);
        GameObject.Find("Slab_Total_Number").GetComponent<Text>().text = Convert.ToString(slab_total);
        GameObject.Find("Slab_Finished_Number").GetComponent<Text>().text = Convert.ToString(slab_new_finished);
        GameObject.Find("Slab_Ratio_Number").GetComponent<Text>().text = Convert.ToString(slab_new_ratio) + " %";
        GameObject.Find("Slab_Cumulative_Finished_Number").GetComponent<Text>().text = Convert.ToString(slab_new_finished + slab_recorded_finished);
        GameObject.Find("Slab_Cumulative_Ratio_Number").GetComponent<Text>().text = Convert.ToString(slab_cumulative_ratio) + " %";
        //Fake********************************************************************
        //GameObject.Find("Slab_Finished_Number").GetComponent<Text>().text = Convert.ToString(slab_total);
        //GameObject.Find("Slab_Ratio_Number").GetComponent<Text>().text = "100 %";
        //GameObject.Find("Slab_Cumulative_Finished_Number").GetComponent<Text>().text = Convert.ToString(slab_total);
        //GameObject.Find("Slab_Cumulative_Ratio_Number").GetComponent<Text>().text = "100 %";
    }
}
