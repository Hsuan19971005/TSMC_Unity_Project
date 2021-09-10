using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataBase 
{
    public static List<FloorData> ifc_floor_data_ = new List<FloorData>();
    public static List<ClassifiedPointCloudData> classified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
    public static List<float[]> real_points_ = new List<float[]>();
    public static string model_name_;
    public static string chosen_floor_name_;
    public static int FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()
    {
        for(int i = 0; i < ifc_floor_data_.Count; i++)
        {
            if (chosen_floor_name_ == ifc_floor_data_[i].floor_name_) return i;
        }
        return -1;
    }
    public static int FindIndexOfClassifiedPointCloudDataByIdName(string id_name)
    {
        for(int i = 0; i < classified_point_cloud_data_.Count; i++)
        {
            if (classified_point_cloud_data_[i].id_name_ == id_name) return i;
        }
        return -1;
    }
}
