using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public string fbx_name;
    public float radius = 0.50f;
    // Start is called before the first frame update
    void Start()
    {
        string ifc_file_path = "D:\\UnityAllFile/TSMC_Unity_Project/AP06-FAB_Grid.ifc";
        string real_points_path = "D:\\UnityAllFile/TSMC_Unity_Project/0813_TSMC竹南_真實點雲_1cm.txt";
        //******************************************第一階段，資料讀取******************************************************//
        DataBase.model_name_ = fbx_name;
        InitiateProject initiate = new InitiateProject(ifc_file_path, real_points_path,DataBase.model_name_);
        DataBase.ifc_floor_data_ = initiate.GetIfcFloorData();
        DataBase.real_points_ = initiate.GetRealPoints();
        DataBase.classified_point_cloud_data_ = initiate.GetClassifiedPointCloudData();
        /**check ifc_floor_data
        for(int i = 0; i < ifc_floor_data.Count; i++)
        {
            Debug.Log("Floor name:"+ifc_floor_data[i].floor_name_+"  Floor height:"+ifc_floor_data[i].floor_height_level_);
            Debug.Log("Column count:"+ifc_floor_data[i].WorkItemDataIfcColumn_.id_name_.Count);
            Debug.Log("Wall count:"+ifc_floor_data[i].WorkItemDataIfcWallStandardCase_.id_name_.Count);
            Debug.Log("Building count:"+ifc_floor_data[i].WorkItemDataIfcBuildingElementProxy_.id_name_.Count);
            Debug.Log("Slab count:"+ifc_floor_data[i].WorkItemDataIfcSlab_.id_name_.Count);
        }
        */
        //******************************************第二階段，真實點雲分類**************************************************//
        DataBase.chosen_floor_name_ ="F30";
        Classifier classifier = new Classifier(DataBase.real_points_, DataBase.classified_point_cloud_data_, radius);
        //******************************************第三階段，樓板點雲分割**************************************************
        GroundCutting ground = new GroundCutting();
        ground.SetAllGridLines(DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].grid_line_);
        //Debug.Log("ground.SetAllGridLines成功");
        ground.CollectGroundPointsByOneIfcFloorData(DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()]);
        //Debug.Log("ground.CollectGroundPointsByOneIfcFloorData成功");
        ground.ClassifyGroundPointsIntoGridsContainingPoints();
        //Debug.Log("ground.ClassifyGroundPointsIntoGridsContainingPoints成功");
        ground.SetGridClassifiedPointCloudData();
        //Debug.Log("ground.SetGridClassifiedPointCloudData成功");
        ground.SetWorkItemDataGrid();
        //Debug.Log("ground.SetWorkItemDataGrid成功");
        /**
        ground.PassGroundCuttingMemberDataToDataBase(DataBase.classified_point_cloud_data_, DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].WorkItemDataGrid_);
        Debug.Log("ground.PassGroundCuttingMemberDataToDataBase成功");

        Debug.Log("grid_classified_point_cloud_data_.Count="+ground.grid_classified_point_cloud_data_.Count);
        for(int i = 0; i < ground.grid_classified_point_cloud_data_.Count; i++)
        {
            Debug.Log(ground.grid_classified_point_cloud_data_[i].id_name_+"  "+ground.grid_classified_point_cloud_data_[i].points_.Count);
        }
        Debug.Log("work_item_data_grid_.id_name_.Count=" + ground.work_item_data_grid_.id_name_.Count);
        for(int i = 0; i < ground.work_item_data_grid_.id_name_.Count; i++)
        {
            Debug.Log(ground.work_item_data_grid_.id_name_[i]);
        }
        */
        //******************************************第四階段，進度判斷**************************************************
        var progress = new ConstructionProgress(DataBase.classified_point_cloud_data_, DataBase.ifc_floor_data_);
        int index_of_user_input_floor_name = -1;
        index_of_user_input_floor_name = progress.FindFloorDataIndex(DataBase.chosen_floor_name_, DataBase.ifc_floor_data_);
        progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataIfcColumn_);
        progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataIfcWallStandardCase_);
        progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataGrid_);

        //******************************************第五階段，顯示分析結果**************************************************
        Debug.Log("光達掃描 柱 該樓層總數量:" + DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataIfcColumn_.id_name_.Count);
        Debug.Log("光達掃描 柱 已完成數量:" + progress.CountOneFloorColumnExistNumber(DataBase.ifc_floor_data_[index_of_user_input_floor_name]));
        Debug.Log("柱 完成比例： " + progress.CountOneFloorColumnExistNumber(DataBase.ifc_floor_data_[index_of_user_input_floor_name]) / (float)progress.CountOneFloorColumnNumber(DataBase.ifc_floor_data_[index_of_user_input_floor_name]) * 100f + "%");
        Debug.Log("光達掃描 牆 該樓層總數量:" + DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataIfcWallStandardCase_.id_name_.Count);
        Debug.Log("光達掃描 牆 已完成數量:" + progress.CountOneFloorWallExistNumber(DataBase.ifc_floor_data_[index_of_user_input_floor_name]));
        Debug.Log("牆 完成比例： " + progress.CountOneFloorWallExistNumber(DataBase.ifc_floor_data_[index_of_user_input_floor_name]) / (float)progress.CountOneFloorWallNumber(DataBase.ifc_floor_data_[index_of_user_input_floor_name]) * 100f + "%");
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("AnalysisResultScene");
        }
    }
}
