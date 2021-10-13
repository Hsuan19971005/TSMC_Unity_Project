using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    GameObject point_cloud_inputfield_;
    GameObject ifc_inputfield_;
    public string fbx_name;
    public float radius = 0.30f;
    public string point_cloud_file_name;
    // Start is called before the first frame update
    private void Awake()
    {
        point_cloud_inputfield_ = GameObject.Find("Point_Cloud_InputField");
        ifc_inputfield_=GameObject.Find("IFC_InputField");

    }
    void Start()
    {
        //Debug.Log("執行前時間:" + System.DateTime.Now);//紀錄時間
        //string ifc_file_path = "D:\\UnityAllFile/TSMC_Unity_Project/AP06-FAB_Grid.ifc";
        //string real_points_path = "D:\\UnityAllFile/TSMC_Unity_Project/0813_TSMC竹南_真實點雲_1cm.txt";
        //string real_points_path = "D:\\UnityAllFile/TSMC_Unity_Project/0716TSMC竹南廠房_虛擬點雲_1cm稀疏.txt";
        string ifc_file_path = "D:\\UnityAllFile/TSMC_Unity_Project/F20-F30.ifc";
        string real_points_path = "D:\\UnityAllFile/TSMC_Unity_Project/"+point_cloud_file_name+".txt";

        //******************************************第一階段，資料讀取******************************************************//
        DataBase.model_name_ = fbx_name;
        InitiateProject initiate = new InitiateProject(ifc_file_path, real_points_path,DataBase.model_name_);
        DataBase.ifc_floor_data_ = initiate.GetIfcFloorData();
        DataBase.real_points_ = initiate.GetRealPoints();
        DataBase.classified_point_cloud_data_ = initiate.GetClassifiedPointCloudData();
        Debug.Log("開始分類時間:" + System.DateTime.Now);//紀錄時間
        //******************************************第二階段，真實點雲分類**************************************************//
        DataBase.chosen_floor_name_ ="F30";
        Classifier classifier = new Classifier(DataBase.real_points_, DataBase.classified_point_cloud_data_, radius);
        //******************************************第三階段，樓板點雲分割**************************************************
        GroundCutting ground = new GroundCutting();
        ground.SetAllGridLines(DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].grid_line_);
        ground.CollectGroundPointsByOneIfcFloorData(DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()]);
        ground.ClassifyGroundPointsIntoGridsContainingPoints();
        ground.SetGridClassifiedPointCloudData();
        ground.SetWorkItemDataGrid();
        DataBase.classified_point_cloud_data_.AddRange(ground.GetGridClassifiedPointCloudData());
        DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].WorkItemDataGrid_ = ground.GetWorkItemDataGrid();
        //******************************************第四階段，進度判斷**************************************************
        var progress = new ConstructionProgress(DataBase.classified_point_cloud_data_, DataBase.ifc_floor_data_);
        int index_of_user_input_floor_name = progress.FindFloorDataIndex(DataBase.chosen_floor_name_, DataBase.ifc_floor_data_);
        progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataIfcColumn_);
        progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataIfcWallStandardCase_);
        progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataGrid_);
        /*************換場景**************/
        //Debug.Log("執行完時間:" + System.DateTime.Now);//紀錄時間



        SceneManager.LoadScene("AnalysisResultScene");
        
    }
    // Update is called once per frame
    void Update()
    {
    }
}
