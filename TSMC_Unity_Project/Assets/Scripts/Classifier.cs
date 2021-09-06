using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Classifier : MonoBehaviour
{
    
    public List<ClassifiedPointCloudData> classified_point_cloud_data_ { get; set; }
    public List<float[]> real_points_ { get; set; }
    public float radius_ { get; set; }
    public Classifier(List<float[]> real_points, List<ClassifiedPointCloudData> classified_point_cloud_data, float radius)
    {
        SetRealPoints(real_points);
        SetClassifiedPointCloudData(classified_point_cloud_data);
        SetRadius(radius);
        ClassifyRealPointsIntoClassifiedPointCloudData(this.real_points_, this.classified_point_cloud_data_, this.radius_);
    }
    public Classifier() { }
    public void ClassifyRealPointsIntoClassifiedPointCloudData(List<float[]>real_points,List<ClassifiedPointCloudData>classified_point_cloud_data,float radius)
    {
        for (int i = 0; i < real_points.Count; i++) 
        {
            string final_object = "";
            Vector3 one_point_vector3 = new Vector3(real_points[i][0],real_points[i][1],real_points[i][2]);
            Collider[] hit_object = Physics.OverlapSphere(one_point_vector3, radius);
            //點的球狀空間碰撞到模型的幾種情形，1.沒撞到，跳到下一個點 2.撞到一個，設定final_object的name 3.撞到多個，找出最近的object，設定final_object的name
            if (hit_object.Length == 0) continue;
            else if (hit_object.Length==1)
            {
                final_object = hit_object[0].name;
            }
            else
            {
                //找最接近的object
                Collider min_hit_object = hit_object[0];
                int index_of_min_hit_object = 0;
                for(int j = 1; j < hit_object.Length; j++)
                {
                    if ((one_point_vector3 - min_hit_object.ClosestPointOnBounds(one_point_vector3)).magnitude > (one_point_vector3 - hit_object[j].ClosestPointOnBounds(one_point_vector3)).magnitude)
                    {
                        min_hit_object = hit_object[j];
                        index_of_min_hit_object = j;
                    }
                }
                //找第二接近的object
                Collider second_min_hit_object = new Collider();
                int loop_count = 0;
                for(int j = 0; j < hit_object.Length; j++)
                {
                    //第2接近的點不能跟第1接近的點重複
                    if (j == index_of_min_hit_object) continue;
                    else if (loop_count == 0)
                    {
                        second_min_hit_object = hit_object[j];
                        loop_count++;
                        continue;
                    }
                    else
                    {
                        if((one_point_vector3 - second_min_hit_object.ClosestPointOnBounds(one_point_vector3)).magnitude > (one_point_vector3 - hit_object[j].ClosestPointOnBounds(one_point_vector3)).magnitude)
                        {
                            second_min_hit_object = hit_object[j];
                        }
                    }
                }
                //如果2個最接近的object距離點同距離，則直接跳到下一個點，否則設定final_object的name
                if ((one_point_vector3 - min_hit_object.ClosestPointOnBounds(one_point_vector3)).magnitude == (one_point_vector3 - second_min_hit_object.ClosestPointOnBounds(one_point_vector3)).magnitude) continue;
                else final_object = min_hit_object.name;
            }
            //取object []中的ID名稱
            final_object = final_object.Substring(final_object.LastIndexOf('[') + 1,final_object.LastIndexOf(']')-final_object.LastIndexOf('[')-1);
            //從一串classified_point_cloud_data中找id_name_相符的，將點接在points_中
            foreach(var one_point_cloud_data in classified_point_cloud_data)
            {
                if (one_point_cloud_data.id_name_ == final_object)
                {
                    float[] one_point_array = new float[3] { real_points[i][0], real_points[i][1], real_points[i][2] };
                    one_point_cloud_data.points_.Add(one_point_array);
                    break;
                }
            }
        }
    }
    public void ShowMeshInClassifiedPointCloudData(List<ClassifiedPointCloudData>classified_point_cloud_data)
    {
        for(int i = 0; i < classified_point_cloud_data.Count; i++)
        {
            GameObject point_object = new GameObject();
            point_object.name = classified_point_cloud_data[i].id_name_;
            point_object.AddComponent<MeshRenderer>();
            point_object.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            Material material = new Material(Shader.Find("Standard"));
            point_object.GetComponent<MeshFilter>().mesh = mesh;
            point_object.GetComponent<MeshRenderer>().material = material;
            Vector3[] points = new Vector3[classified_point_cloud_data[i].points_.Count];
            Color[] colors = new Color[classified_point_cloud_data[i].points_.Count];
            int[] indecies = new int[classified_point_cloud_data[i].points_.Count];
            for(int j = 0; j < classified_point_cloud_data[i].points_.Count; j++)
            {
                Vector3 vector3 = new Vector3(classified_point_cloud_data[i].points_[j][0], classified_point_cloud_data[i].points_[j][1], classified_point_cloud_data[i].points_[j][2]);
                points[j] = vector3;
                colors[j] = Color.yellow;
                indecies[j] = j;
            }
            mesh.vertices = points;
            mesh.colors = colors;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);
        }
    }

    //Set系列
    public void SetRealPoints(List<float[]> real_points)
    {
        this.real_points_ = real_points;
    }
    public void SetClassifiedPointCloudData(List<ClassifiedPointCloudData>classified_point_cloud_data)
    {
        this.classified_point_cloud_data_ = classified_point_cloud_data;
    }
    public void SetRadius(float radius)
    {
        this.radius_ = radius;
    }

}
