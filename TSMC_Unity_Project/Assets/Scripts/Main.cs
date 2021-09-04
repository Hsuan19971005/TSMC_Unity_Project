using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Main : MonoBehaviour
{
    public string fbx_name;
    // Start is called before the first frame update
    void Start()
    {
        string path1 = "D:\\UnityAllFile/TSMC_Unity_Project/AP06-FAB_Grid.ifc";
        string path2 = "D:\\UnityAllFile/TSMC_Unity_Project/0716TSMC竹南廠房_虛擬點雲_1cm稀疏.txt";//點雲
        //string path3 = "D:\\UnityAllFile/TSMC_Unity_Project/0813TSMC竹南_已分類虛擬點雲_3cm稀疏.txt";
        string path5 = "D:\\UnityAllFile/TSMC_Unity_Project/已完成未完成";
        float deviation = 0.3f;
        var real_points = new List<float[]>();
        var classified_point_cloud_data = new List<ClassifiedPointCloudData>();
        var verified_point_cloud_data = new List<ClassifiedPointCloudData>();
        var ifc_floor_data = new List<FloorData>();
        //******************************************第一階段******************************************************//
        
        Input input = new Input(path1, path2);
        input.IniciateClassifiedPointCloudData(fbx_name);
        input.DeleteClassifiedPointCloudDataNotInIfcFloorData(input.classified_point_cloud_data_, input.all_id_);
        ifc_floor_data = input.GetIfcFloorData();
        Debug.Log("FBX model child:");
        for(int i = 0; i < input.classified_point_cloud_data_.Count; i++)
        {
            Debug.Log("ID: "+input.classified_point_cloud_data_[i].id_name_);
        }
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

        //real_points = input.GetRealPoints();
        //classified_point_cloud_data = input.GetClassifiedPointCloudData();
        Debug.Log("IFC解析完成 第一階段結束");
        
        
        //**Console.WriteLine("3樓的Slab ID");
        //for(int i=0;i<ifc_floor_data[3].WorkItemDataIfcSlab_.id_name_.Count;i++) Console.WriteLine(ifc_floor_data[3].WorkItemDataIfcSlab_.id_name_[i]+" ");
        //input.SetGrounCuttingAllGridLines("F30");
        //input.SetGroundCuttingAllGroundPoints("F30");
        //input.SetGroundCuttingPointsInGrid();
        /**顯示地面網格各自點數
        for (int i = 0; i < input.ground_cutting_.grids_.Length; i++)
        {
            string str = Convert.ToString(input.ground_cutting_.grids_[i].Count);
            Console.Write(str.PadLeft(7));
            if (i % 22 == 21) Console.WriteLine();
        }
        Console.WriteLine("Vertical line:" + input.ground_cutting_.vertical_grid_lines_.Count);
        for (int i = 0; i < input.ground_cutting_.vertical_grid_lines_.Count; i++)
        {
            Console.Write("(" + input.ground_cutting_.vertical_grid_lines_[i].point1_x_ + ", " + input.ground_cutting_.vertical_grid_lines_[i].point1_y_ + ")");
            Console.Write("(" + input.ground_cutting_.vertical_grid_lines_[i].point2_x_ + ", " + input.ground_cutting_.vertical_grid_lines_[i].point2_y_ + ")");
            Console.WriteLine();
        }
        */



        //******************************************第二階段**************************************************//
        /**
        //這邊先自己設定判斷標準，設定數量判斷模式開啟
        foreach (var i in ifc_floor_data)
        {
            i.WorkItemDataIfcBuildingElementProxy_.standard_number_ = true;
            i.WorkItemDataIfcColumn_.standard_number_ = true;
            i.WorkItemDataIfcSlab_.standard_number_ = true;
            i.WorkItemDataIfcWallStandardCase_.standard_number_ = true;
        }
        //顯示可分析樓層，並且指示使用者選一個樓層分析
        Console.Write("可分析樓層:   ");
        for (int i = 0; i < ifc_floor_data.Count; i++) Console.Write(ifc_floor_data[i].floor_name_ + "   ");
        Console.Write("\n輸入欲分析樓層名稱:");
        //string user_input_floor_name = Console.ReadLine();
        string user_input_floor_name = "F30";
        Console.WriteLine("開始進度分析...");
        //分析該樓層的柱牆WorkItemData
        var progress = new ConstructionProgress(classified_point_cloud_data, verified_point_cloud_data);
        int index_of_user_input_floor_name = -1;
        index_of_user_input_floor_name = progress.FindFloorDataIndex(user_input_floor_name, ifc_floor_data);
        Console.WriteLine("柱 分析中...");
        progress.DoOneWorkItemDataProgress(ifc_floor_data[index_of_user_input_floor_name].WorkItemDataIfcColumn_);
        Console.WriteLine("牆 分析中...");
        progress.DoOneWorkItemDataProgress(ifc_floor_data[index_of_user_input_floor_name].WorkItemDataIfcWallStandardCase_);
        */
        //******************************************第三階段**************************************************
        /**
        //顯示分析結果
        Console.WriteLine("光達掃描 柱 該樓層總數量:" + ifc_floor_data[index_of_user_input_floor_name].WorkItemDataIfcColumn_.id_name_.Count);
        Console.WriteLine("光達掃描 柱 待分析數量:" + progress.CountOneFloorColumnNumber(ifc_floor_data[index_of_user_input_floor_name]));
        Console.WriteLine("光達掃描 柱 已完成數量:" + progress.CountOneFloorColumnExistNumber(ifc_floor_data[index_of_user_input_floor_name]));
        Console.WriteLine("柱 完成比例： " + progress.CountOneFloorColumnExistNumber(ifc_floor_data[index_of_user_input_floor_name]) / (float)progress.CountOneFloorColumnNumber(ifc_floor_data[index_of_user_input_floor_name]) * 100f + "%");
        Console.WriteLine("光達掃描 牆 該樓層總數量:" + ifc_floor_data[index_of_user_input_floor_name].WorkItemDataIfcWallStandardCase_.id_name_.Count);
        Console.WriteLine("光達掃描 牆 待分析數量:" + progress.CountOneFloorWallNumber(ifc_floor_data[index_of_user_input_floor_name]));
        Console.WriteLine("光達掃描 牆 已完成數量:" + progress.CountOneFloorWallExistNumber(ifc_floor_data[index_of_user_input_floor_name]));
        Console.WriteLine("牆 完成比例： " + progress.CountOneFloorWallExistNumber(ifc_floor_data[index_of_user_input_floor_name]) / (float)progress.CountOneFloorWallNumber(ifc_floor_data[index_of_user_input_floor_name]) * 100f + "%");
        Console.WriteLine("輸出點雲檔中...");
        progress.WriteOneFloorExistPointCloudData(path5, "已完成虛擬點雲", ifc_floor_data[index_of_user_input_floor_name]);
        progress.WriteOneFloorNotExistPointCloudData(path5, "未完成虛擬點雲", ifc_floor_data[index_of_user_input_floor_name]);
        Console.WriteLine("點雲檔輸出完成");

        //verify.WriteVerifiedPointData(path5, "認可點雲");

        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
