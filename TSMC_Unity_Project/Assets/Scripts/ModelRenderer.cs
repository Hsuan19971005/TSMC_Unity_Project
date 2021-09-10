using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ModelRenderer : MonoBehaviour
{
    public Hashtable model_hashtable_ { get; set; }
    public GameObject model_ { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        model_hashtable_ = new Hashtable();
        SetModel();
        SetModelHashtable();
        ChangeModelColor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetModelHashtable()
    {
        for(int i = 0; i < this.model_.transform.childCount; i++)
        {
            string child_name = this.model_.transform.GetChild(i).gameObject.name;
            if (child_name.Length <= 2) continue;
            else if (child_name.LastIndexOf('[') == -1 || child_name.LastIndexOf(']') == -1) continue;
            string child_id_name = child_name.Substring(child_name.LastIndexOf('[') + 1, child_name.LastIndexOf(']') - child_name.LastIndexOf('[') - 1);
            this.model_hashtable_.Add(child_id_name, child_name);
        }
    }
    public void SetModel()
    {
        this.model_ = GameObject.Find(DataBase.model_name_);
    }
    public void ChangeModelColor()
    {
        int index_of_ifc_floor = DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName();
        //設定已完成建築元件的顏色為綠色
        //柱
        for(int i = 0; i < DataBase.ifc_floor_data_[index_of_ifc_floor].WorkItemDataIfcColumn_.id_name_.Count; i++)
        {
            int index_of_classified_point_cloud_data = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(DataBase.ifc_floor_data_[index_of_ifc_floor].WorkItemDataIfcColumn_.id_name_[i]);
            if (index_of_classified_point_cloud_data == -1) continue;
            if (DataBase.classified_point_cloud_data_[index_of_classified_point_cloud_data].exist_ == true && this.model_hashtable_.Contains(DataBase.classified_point_cloud_data_[index_of_classified_point_cloud_data].id_name_) == true)
            {
                string child_name=Convert.ToString(model_hashtable_[DataBase.classified_point_cloud_data_[index_of_classified_point_cloud_data].id_name_]);
                GameObject game_object = GameObject.Find(child_name);
                game_object.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }
        //牆
        for (int i = 0; i < DataBase.ifc_floor_data_[index_of_ifc_floor].WorkItemDataIfcWallStandardCase_.id_name_.Count; i++)
        {
            int index_of_classified_point_cloud_data = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(DataBase.ifc_floor_data_[index_of_ifc_floor].WorkItemDataIfcWallStandardCase_.id_name_[i]);
            if (index_of_classified_point_cloud_data == -1) continue;
            if (DataBase.classified_point_cloud_data_[index_of_classified_point_cloud_data].exist_ == true && this.model_hashtable_.Contains(DataBase.classified_point_cloud_data_[index_of_classified_point_cloud_data].id_name_) == true)
            {
                string child_name = Convert.ToString(model_hashtable_[DataBase.classified_point_cloud_data_[index_of_classified_point_cloud_data].id_name_]);
                GameObject game_object = GameObject.Find(child_name);
                game_object.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }
    }


}
