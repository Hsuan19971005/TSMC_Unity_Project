using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ConstructionProgress 
{
    private List<ClassifiedPointCloudData> classified_point_cloud_data_ { get; set; }
    private List<FloorData> ifc_floor_data_ { get; set; }
 
    public ConstructionProgress(List<ClassifiedPointCloudData> classified_point_cloud_data,List<FloorData>ifc_floor_data)
    {
        this.classified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.ifc_floor_data_ = new List<FloorData>();
        this.classified_point_cloud_data_ = classified_point_cloud_data;
        this.ifc_floor_data_ = ifc_floor_data;
    }
    public ConstructionProgress()
    {
        this.classified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.ifc_floor_data_ = new List<FloorData>();
    }
    public void DoOneWorkItemDataProgress(WorkItemData one_work_item_data)
    {
        foreach (var name in one_work_item_data.id_name_)
        {
            int index_of_name = Find_ID_NameIndexFromClassifiedPointCloudDataType(name, this.classified_point_cloud_data_);
            if (index_of_name == -1 ) continue;
            if (this.classified_point_cloud_data_[index_of_name].points_.Count >= one_work_item_data.points_number_threshold_) this.classified_point_cloud_data_[index_of_name].exist_ = true;
        }
    }
    public int Find_ID_NameIndexFromClassifiedPointCloudDataType(string name, List<ClassifiedPointCloudData> classified_point_cloud_data)
    {
        for (int i = 0; i < classified_point_cloud_data.Count; i++) if (classified_point_cloud_data[i].id_name_ == name) return i;
        return -1;
    }
    public int FindFloorDataIndex(string name, List<FloorData> ifc_floor_data)
    {
        for (int i = 0; i < ifc_floor_data.Count; i++)
        {
            if (name == ifc_floor_data[i].floor_name_) return i;
        }
        return -1;
    }
    public int CountOneFloorColumnExistNumber(FloorData one_floor_data,List<ClassifiedPointCloudData>classified_point_cloud_data)
    {
        int count = 0;
        foreach (var name in one_floor_data.WorkItemDataIfcColumn_.id_name_)
        {
            foreach (var data in classified_point_cloud_data)
            {
                if (data.id_name_ == name && data.exist_ == true)
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }
    public int CountOneFloorWallExistNumber(FloorData one_floor_data, List<ClassifiedPointCloudData> classified_point_cloud_data)
    {
        int count = 0;
        foreach (var name in one_floor_data.WorkItemDataIfcWallStandardCase_.id_name_)
        {
            foreach (var data in classified_point_cloud_data)
            {
                if (data.id_name_ == name && data.exist_ == true)
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }
    public int CountOneFloorGridExistNumber(FloorData one_floor_data, List<ClassifiedPointCloudData> classified_point_cloud_data)
    {
        int count = 0;
        foreach (var name in one_floor_data.WorkItemDataGrid_.id_name_)
        {
            foreach (var data in classified_point_cloud_data)
            {
                if (data.id_name_ == name && data.exist_ == true)
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }
    //Write 系列

    public ClassifiedPointCloudData GetClassifiedPointCloudDataByName(string name, List<ClassifiedPointCloudData> classified_point_cloud_data)
    {
        for (int i = 0; i < classified_point_cloud_data.Count; i++)
        {
            if (name == classified_point_cloud_data[i].id_name_) return classified_point_cloud_data[i];
        }
        return null;
    }
}
