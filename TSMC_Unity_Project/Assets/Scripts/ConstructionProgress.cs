using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ConstructionProgress : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private List<ClassifiedPointCloudData> classified_point_cloud_data_ { get; set; }
    private List<ClassifiedPointCloudData> verified_point_cloud_data_ { get; set; }
    private float standard_number_percent_ { get; set; }
    private float standard_height_percent_ { get; set; }
    private float standard_two_sides_percent_ { get; set; }
    private float standard_two_sides_span_ { get; set; }
    public ConstructionProgress(List<ClassifiedPointCloudData> classified_point_cloud_data, List<ClassifiedPointCloudData> verified_point_cloud_data)
    {
        this.classified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.verified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.classified_point_cloud_data_ = classified_point_cloud_data;
        this.verified_point_cloud_data_ = verified_point_cloud_data;
        this.standard_number_percent_ = 80;
        this.standard_height_percent_ = 80;
        this.standard_two_sides_percent_ = 80;
        this.standard_two_sides_span_ = 0.1f;
    }
    public ConstructionProgress()
    {
        this.classified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.verified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.standard_number_percent_ = 80;
        this.standard_height_percent_ = 80;
        this.standard_two_sides_percent_ = 80;
        this.standard_two_sides_span_ = 0.1f;
    }
    public void DoOneWorkItemDataProgress(WorkItemData one_work_item_data, float n_percent, float h_percent, float t_percent, float span)
    {
        bool s_number = true;
        bool s_height = true;
        bool s_two_sides = true;
        foreach (var i in one_work_item_data.id_name_)
        {
            int index1 = Find_ID_NameIndexFromClassifiedPointCloudDataType(i, this.classified_point_cloud_data_);
            int index2 = Find_ID_NameIndexFromClassifiedPointCloudDataType(i, this.verified_point_cloud_data_);
            if (index1 == -1 || index2 == -1)
            {
                //Console.WriteLine("Can't find the one work item ID (From DoOneWorkItemDataProgress)");
                continue;
            }
            if (one_work_item_data.standard_number_) s_number = JudgeByNumberPercent(this.classified_point_cloud_data_[index1], this.verified_point_cloud_data_[index2], n_percent);
            if (one_work_item_data.standard_height_) s_height = JudgeByHeightPercent(this.classified_point_cloud_data_[index1], this.verified_point_cloud_data_[index2], h_percent);
            if (one_work_item_data.standard_two_sides_) s_two_sides = JudgeByTwoSidesPercent(this.classified_point_cloud_data_[index1], this.verified_point_cloud_data_[index2], span, t_percent);
            if (s_number && s_height && s_two_sides)
            {
                classified_point_cloud_data_[index1].exist_ = true;
                verified_point_cloud_data_[index2].exist_ = true;
            }
            else
            {
                classified_point_cloud_data_[index1].exist_ = false;
                verified_point_cloud_data_[index2].exist_ = false;
            }
        }
    }
    public void DoOneWorkItemDataProgress(WorkItemData one_work_item_data)
    {
        bool s_number = true;
        bool s_height = true;
        bool s_two_sides = true;
        foreach (var i in one_work_item_data.id_name_)
        {
            int index1 = Find_ID_NameIndexFromClassifiedPointCloudDataType(i, this.classified_point_cloud_data_);
            int index2 = Find_ID_NameIndexFromClassifiedPointCloudDataType(i, this.verified_point_cloud_data_);
            if (index1 == -1 || index2 == -1)
            {
                //Console.WriteLine("Can't find the one work item ID (From DoOneWorkItemSchedule "+i);
                continue;
            }
            if (one_work_item_data.standard_number_) s_number = JudgeByNumberPercent(this.classified_point_cloud_data_[index1], this.verified_point_cloud_data_[index2], this.standard_number_percent_);
            if (one_work_item_data.standard_height_) s_height = JudgeByHeightPercent(this.classified_point_cloud_data_[index1], this.verified_point_cloud_data_[index2], this.standard_height_percent_);
            if (one_work_item_data.standard_two_sides_) s_two_sides = JudgeByTwoSidesPercent(this.classified_point_cloud_data_[index1], this.verified_point_cloud_data_[index2], this.standard_two_sides_span_, this.standard_two_sides_percent_);
            if (s_number && s_height && s_two_sides)
            {
                classified_point_cloud_data_[index1].exist_ = true;
                verified_point_cloud_data_[index2].exist_ = true;
                //Console.WriteLine("存在！");
            }
            else
            {
                classified_point_cloud_data_[index1].exist_ = false;
                verified_point_cloud_data_[index2].exist_ = false;
                // Console.WriteLine("不存在！");
            }
        }
    }
    public bool JudgeByHeightPercent(ClassifiedPointCloudData one_classified_point_cloud_data, ClassifiedPointCloudData one_verified_point_cloud_data, float percent)
    {
        if ((float)(one_verified_point_cloud_data.z_max_ - one_verified_point_cloud_data.z_min_) / (one_classified_point_cloud_data.z_max_ - one_classified_point_cloud_data.z_min_) * 100 >= percent) return true;
        else return false;
    }
    public bool JudgeByNumberPercent(ClassifiedPointCloudData one_classified_point_cloud_data, ClassifiedPointCloudData one_verified_point_cloud_data, float percent)
    {
        if ((float)one_verified_point_cloud_data.points_.Count / one_classified_point_cloud_data.points_.Count * 100 >= percent) return true;
        else return false;
    }
    public bool JudgeByTwoSidesPercent(ClassifiedPointCloudData one_classified_point_cloud_data, ClassifiedPointCloudData one_verified_point_cloud_data, float span, float percent)
    {
        float top_limit = one_classified_point_cloud_data.z_max_ - span;
        float down_limit = one_classified_point_cloud_data.z_min_ + span;
        List<float[]> clTop = new List<float[]>();
        List<float[]> clDown = new List<float[]>();
        List<float[]> veTop = new List<float[]>();
        List<float[]> veDown = new List<float[]>();
        for (int i = 0; i < one_classified_point_cloud_data.points_.Count; i++)
        {
            if (one_classified_point_cloud_data.points_[i][2] >= top_limit) clTop.Add(one_classified_point_cloud_data.points_[i]);
            else if (one_classified_point_cloud_data.points_[i][2] <= down_limit) clDown.Add(one_classified_point_cloud_data.points_[i]);
        }
        for (int i = 0; i < one_verified_point_cloud_data.points_.Count; i++)
        {
            if (one_verified_point_cloud_data.points_[i][2] >= top_limit) veTop.Add(one_verified_point_cloud_data.points_[i]);
            else if (one_verified_point_cloud_data.points_[i][2] <= down_limit) veDown.Add(one_verified_point_cloud_data.points_[i]);
        }
        if ((float)veTop.Count / clTop.Count * 100 >= percent && (float)veDown.Count / clDown.Count * 100 >= percent) return true;
        else return false;
    }
    public int Find_ID_NameIndexFromClassifiedPointCloudDataType(string name, List<ClassifiedPointCloudData> classified_point_cloud_data)
    {
        for (int i = 0; i < classified_point_cloud_data.Count; i++) if (classified_point_cloud_data[i].id_name_ == name) return i;
        return -1;
    }
    public int FindWorkItemNameIndex(string name, List<WorkItemData> data)
    {
        for (int i = 0; i < data.Count; i++) if (data[i].work_item_name_ == name) return i;
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
    public int CountOneFloorColumnExistNumber(FloorData one_floor_data)
    {
        int count = 0;
        foreach (var name in one_floor_data.WorkItemDataIfcColumn_.id_name_)
        {
            foreach (var data in this.classified_point_cloud_data_)
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
    public int CountOneFloorColumnNumber(FloorData one_floor_data)
    {
        int count = 0;
        foreach (var name in one_floor_data.WorkItemDataIfcColumn_.id_name_)
        {
            foreach (var data in this.classified_point_cloud_data_)
            {
                if (data.id_name_ == name)
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }
    public int CountOneFloorWallExistNumber(FloorData one_floor_data)
    {
        int count = 0;
        foreach (var name in one_floor_data.WorkItemDataIfcWallStandardCase_.id_name_)
        {
            foreach (var data in this.classified_point_cloud_data_)
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
    public int CountOneFloorWallNumber(FloorData one_floor_data)
    {
        int count = 0;
        foreach (var name in one_floor_data.WorkItemDataIfcWallStandardCase_.id_name_)
        {
            foreach (var data in this.classified_point_cloud_data_)
            {
                if (data.id_name_ == name)
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    //Write 系列
    public void WriteOneFloorExistPointCloudData(string document_path, string file_name, FloorData one_floor_data)
    {
        string path = document_path + '/' + file_name + ".txt";
        StreamWriter sw = new StreamWriter(path);
        //寫入牆的點雲
        foreach (var name in one_floor_data.WorkItemDataIfcWallStandardCase_.id_name_)
        {
            ClassifiedPointCloudData classified_point_cloud_data = GetClassifiedPointCloudDataByName(name, this.classified_point_cloud_data_);
            if (classified_point_cloud_data == null) continue;
            if (classified_point_cloud_data.exist_ == true)
            {
                for (int j = 0; j < classified_point_cloud_data.points_.Count; j++)
                {
                    sw.WriteLine(classified_point_cloud_data.points_[j][0] + " " + classified_point_cloud_data.points_[j][1] + " " + classified_point_cloud_data.points_[j][2] + " 0.333333 1 0.498039");
                }
            }
        }
        //寫入柱的點雲
        foreach (var name in one_floor_data.WorkItemDataIfcColumn_.id_name_)
        {
            ClassifiedPointCloudData classified_point_cloud_data = GetClassifiedPointCloudDataByName(name, this.classified_point_cloud_data_);
            if (classified_point_cloud_data == null) continue;
            if (classified_point_cloud_data.exist_ == true)
            {
                for (int j = 0; j < classified_point_cloud_data.points_.Count; j++)
                {
                    sw.WriteLine(classified_point_cloud_data.points_[j][0] + " " + classified_point_cloud_data.points_[j][1] + " " + classified_point_cloud_data.points_[j][2] + " 0.333333 1 0.498039");
                }
            }
        }
        sw.Flush();
        sw.Close();
    }
    public void WriteOneFloorNotExistPointCloudData(string document_path, string file_name, FloorData one_floor_data)
    {
        string path = document_path + '/' + file_name + ".txt";
        StreamWriter sw = new StreamWriter(path);
        //寫入牆的點雲
        foreach (var name in one_floor_data.WorkItemDataIfcWallStandardCase_.id_name_)
        {
            ClassifiedPointCloudData classified_point_cloud_data = GetClassifiedPointCloudDataByName(name, this.classified_point_cloud_data_);
            if (classified_point_cloud_data == null) continue;
            if (classified_point_cloud_data.exist_ == false)
            {
                for (int j = 0; j < classified_point_cloud_data.points_.Count; j++)
                {
                    sw.WriteLine(classified_point_cloud_data.points_[j][0] + " " + classified_point_cloud_data.points_[j][1] + " " + classified_point_cloud_data.points_[j][2] + " 1 0 0.498039");
                }
            }
        }
        //寫入柱的點雲
        foreach (var name in one_floor_data.WorkItemDataIfcColumn_.id_name_)
        {
            ClassifiedPointCloudData classified_point_cloud_data = GetClassifiedPointCloudDataByName(name, this.classified_point_cloud_data_);
            if (classified_point_cloud_data == null) continue;
            if (classified_point_cloud_data.exist_ == false)
            {
                for (int j = 0; j < classified_point_cloud_data.points_.Count; j++)
                {
                    sw.WriteLine(classified_point_cloud_data.points_[j][0] + " " + classified_point_cloud_data.points_[j][1] + " " + classified_point_cloud_data.points_[j][2] + " 1 0 0.498039");
                }
            }
        }
        sw.Flush();
        sw.Close();
    }
    public ClassifiedPointCloudData GetClassifiedPointCloudDataByName(string name, List<ClassifiedPointCloudData> classified_point_cloud_data)
    {
        for (int i = 0; i < classified_point_cloud_data.Count; i++)
        {
            if (name == classified_point_cloud_data[i].id_name_) return classified_point_cloud_data[i];
        }
        return null;
    }
}
