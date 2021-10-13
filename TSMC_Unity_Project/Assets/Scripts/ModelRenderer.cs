using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ModelRenderer : MonoBehaviour
{
    #region Toggle
    Toggle walls_columns_toggle_;
    Toggle walls_columns_progress_toggle_;
    Toggle slabs_toggle_;
    Toggle slabs_progress_toggle_;
    Toggle grid_line_toggle_;
    Toggle others_toggle_;
    #endregion
    #region Model Information
    private Hashtable model_hashtable_ { get; set; }//Key=元件ID, Value=模型物件名稱
    public GameObject model_ { get; set; }//FBX Model
    private List<GameObject> walls_columns_gameobjects_ { get; set; }
    private List<GameObject> walls_columns_progress_gameobjects_ { get; set; }
    private List<GameObject> ground_gameobjects_ { get; set; }
    private List<GameObject> ground_progress_gameobjects_ { get; set; }
    private List<GameObject> unanalyzed_gameobjects_ { get; set; }
    private List<GameObject> grid_line_gameobjects_ { get; set; }
    private float max_y_of_ground_element { get; set; }
    #endregion

    private void Awake()
    {
        this.model_hashtable_ = new Hashtable();
        this.walls_columns_gameobjects_ = new List<GameObject>();
        this.walls_columns_progress_gameobjects_ = new List<GameObject>();
        this.ground_gameobjects_ = new List<GameObject>();
        this.ground_progress_gameobjects_ = new List<GameObject>();
        this.unanalyzed_gameobjects_ = new List<GameObject>();
        this.grid_line_gameobjects_ = new List<GameObject>();

        walls_columns_toggle_ = GameObject.Find("Walls_Columns_Toggle").GetComponent<Toggle>();
        walls_columns_progress_toggle_ = GameObject.Find("Walls_Columns_Progress_Toggle").GetComponent<Toggle>();
        slabs_toggle_ = GameObject.Find("Slabs_Toggle").GetComponent<Toggle>();
        slabs_progress_toggle_ = GameObject.Find("Slabs_Progress_Toggle").GetComponent<Toggle>();
        grid_line_toggle_ = GameObject.Find("Grid_Line_Toggle").GetComponent<Toggle>();
        others_toggle_ = GameObject.Find("Others_Toggle").GetComponent<Toggle>();
    }
    private void Start()
    {
        
        SetModel(DataBase.model_name_);
        SetModelHashtable();
        DivideModelChildrenIntoCorrespondingGameObjects();
        SetWallsColumnsProgressGameObjects();
        this.max_y_of_ground_element = FindMaxYOfFloorElementFromModel();
        SetGroundProgressGameObjects();
        SetGridLineGameObjects();
        ChangeUnanalyzedVisibility();
        ChangeWallsColumnsProgressVisibility();
    }
    private void Update()
    {
        
    }
    private void SetModel(string model_name)
    {
        this.model_ = GameObject.Find(model_name);
    }
    private void SetModelHashtable()
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
    private void DivideModelChildrenIntoCorrespondingGameObjects()
    {
        int index_of_ifc_floor = DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName();
        for (int i = 0; i < this.model_.transform.childCount; i++)
        {
            GameObject child = this.model_.transform.GetChild(i).gameObject;
            string child_name = child.name;
            if (child_name.LastIndexOf('[') == -1 || child_name.LastIndexOf(']') == -1) continue;
            child_name=child.name.Substring(child.name.LastIndexOf('[') + 1, child.name.LastIndexOf(']') - child.name.LastIndexOf('[') - 1);
            if (DataBase.ifc_floor_data_[index_of_ifc_floor].WorkItemDataIfcWallStandardCase_.id_name_.Contains(child_name) == true || DataBase.ifc_floor_data_[index_of_ifc_floor].WorkItemDataIfcColumn_.id_name_.Contains(child_name) == true)
            {
                this.walls_columns_gameobjects_.Add(child);
                continue;
            }
            else if(DataBase.ifc_floor_data_[index_of_ifc_floor].WorkItemDataIfcBuildingElementProxy_.id_name_.Contains(child_name) == true || DataBase.ifc_floor_data_[index_of_ifc_floor].WorkItemDataIfcSlab_.id_name_.Contains(child_name) == true)
            {
                this.ground_gameobjects_.Add(child);
                continue;
            }
            else
            {
                this.unanalyzed_gameobjects_.Add(child);
                continue;
            }
        }
    }
    private void SetWallsColumnsProgressGameObjects()
    {
        for(int i = 0; i < this.walls_columns_gameobjects_.Count; i++)
        {
            //catch gameobject's id
            string gameobject_id = this.walls_columns_gameobjects_[i].name;
            if (gameobject_id.LastIndexOf('[') == -1 || gameobject_id.LastIndexOf(']') == -1) continue;
            gameobject_id = gameobject_id.Substring(gameobject_id.LastIndexOf('[') + 1, gameobject_id.LastIndexOf(']') - gameobject_id.LastIndexOf('[') - 1);
            //check exist and add it to this.walls_columns_gameobjects_
            int index_of_classified_point_cloud_data = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(gameobject_id);
            if (DataBase.classified_point_cloud_data_[index_of_classified_point_cloud_data].exist_ == true) this.walls_columns_progress_gameobjects_.Add(this.walls_columns_gameobjects_[i]);
        }
    }
    public void SetGroundProgressGameObjects()
    {
        List<string> id_name_of_grid = DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].WorkItemDataGrid_.id_name_;
        //4個角的座標值先從右手坐標系轉換至左手坐標系，並且將Y座標設定為版元素最大Y值
        for (int i = 0; i < id_name_of_grid.Count; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                Transfer(DataBase.classified_point_cloud_data_[DataBase.FindIndexOfClassifiedPointCloudDataByIdName(id_name_of_grid[i])].four_corner_points[j]);
                DataBase.classified_point_cloud_data_[DataBase.FindIndexOfClassifiedPointCloudDataByIdName(id_name_of_grid[i])].four_corner_points[j][1] = this.max_y_of_ground_element+0.01f;
            }
        }
        //build new gameobjects and add them to this.ground_progress_gameobjects_
        for (int i = 0; i < id_name_of_grid.Count; i++)
        {
            ClassifiedPointCloudData point_cloud_data = DataBase.classified_point_cloud_data_[DataBase.FindIndexOfClassifiedPointCloudDataByIdName(id_name_of_grid[i])];
            GameObject obj = new GameObject();
            obj.name = point_cloud_data.id_name_;
            MeshRenderer mesh_renderer = obj.AddComponent<MeshRenderer>();
            mesh_renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            MeshFilter mesh_filter = obj.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(point_cloud_data.four_corner_points[0][0],point_cloud_data.four_corner_points[0][1],point_cloud_data.four_corner_points[0][2]),
                new Vector3(point_cloud_data.four_corner_points[1][0],point_cloud_data.four_corner_points[1][1],point_cloud_data.four_corner_points[1][2]),
                new Vector3(point_cloud_data.four_corner_points[2][0],point_cloud_data.four_corner_points[2][1],point_cloud_data.four_corner_points[2][2]),
                new Vector3(point_cloud_data.four_corner_points[3][0],point_cloud_data.four_corner_points[3][1],point_cloud_data.four_corner_points[3][2])
            };
            mesh.vertices = vertices;
            int[] tris = new int[6]
            {
                // lower left triangle
                0,1,3,
                // upper right triangle
                1,2,3
            };
            mesh.triangles = tris;
            Vector3[] normals = new Vector3[4]
            {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
            };
            mesh.normals = normals;
            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1)
            };
            mesh.uv = uv;
            mesh_filter.mesh = mesh;
            if (point_cloud_data.exist_ == true) mesh_renderer.material.color = Color.cyan;
            else
            {
                mesh_renderer.material.color = Color.white;
            }
            this.ground_progress_gameobjects_.Add(obj);
        }

    }
    public void SetGridLineGameObjects()
    {
        //轉換該層樓的住線座標，創建柱線GameObjects
        List<Line> grid_line = DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].grid_line_;
        for (int i = 0; i < grid_line.Count; i++)
        {
            Transfer(grid_line[i]);
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Vector3 direction = new Vector3(grid_line[i].point2_x_ - grid_line[i].point1_x_, grid_line[i].point2_y_ - grid_line[i].point1_y_, grid_line[i].point2_z_ - grid_line[i].point1_z_);
            obj.name = "grid_line_" + i;
            obj.transform.position = new Vector3(grid_line[i].mid_point_x_, this.max_y_of_ground_element + 0.01f, grid_line[i].mid_point_z_);
            obj.transform.localScale = new Vector3(0.05f, direction.magnitude / 2f, 0.05f);
            obj.transform.up = direction;
            obj.GetComponent<MeshRenderer>().material.color = Color.black;
            this.grid_line_gameobjects_.Add(obj);
        }
        
    }
    private void Transfer(float[]point)
    {
        point[0] *= -1;
        float tmp = point[1];
        point[1] = point[2];
        point[2] = tmp*-1f;
    }
    private void Transfer(Line line)
    {
        line.point1_x_ *= -1f;
        line.point2_x_ *= -1f;
        float tmp_1 = line.point1_y_;
        float tmp_2 = line.point2_y_;
        line.point1_y_ = line.point1_z_;
        line.point2_y_ = line.point2_z_;
        line.point1_z_ = tmp_1 * -1f;
        line.point2_z_ = tmp_2 * -1f;
        line.SetMidPoint();
    }
    private float FindMaxYOfFloorElementFromModel()
    {
        float max_y = 0;
        for(int i = 0; i < this.ground_gameobjects_.Count; i++)
        {
            Renderer rend = this.ground_gameobjects_[i].GetComponent<Renderer>();
            if (i == 0) max_y = rend.bounds.max.y;
            if (rend.bounds.max.y > max_y) max_y = rend.bounds.max.y;
        }
        return max_y;
    }

    #region Functions for Toggles
    public void ChangeWallsColumnsVisibility()
    {
        if (walls_columns_toggle_.isOn == true)
        {
            for(int i = 0; i < this.walls_columns_gameobjects_.Count; i++)
            {
                this.walls_columns_gameobjects_[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < this.walls_columns_gameobjects_.Count; i++)
            {
                this.walls_columns_gameobjects_[i].SetActive(false);
            }
        }
    }
    public void ChangeGroundVisibility()
    {
        if (slabs_toggle_.isOn == true)
        {
            for(int i = 0; i < this.ground_gameobjects_.Count; i++)
            {
                this.ground_gameobjects_[i].SetActive(true);
            }
        }
        else
        {
            for(int i = 0; i < this.ground_gameobjects_.Count; i++)
            {
                this.ground_gameobjects_[i].SetActive(false);
            }
        }
    }
    public void ChangeUnanalyzedVisibility()
    {
        if (others_toggle_.isOn == true)
        {
            for (int i = 0; i < this.unanalyzed_gameobjects_.Count; i++)
            {
                this.unanalyzed_gameobjects_[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < this.unanalyzed_gameobjects_.Count; i++)
            {
                this.unanalyzed_gameobjects_[i].SetActive(false);
            }
        }
    }
    public void ChangeWallsColumnsProgressVisibility()
    {
        if (walls_columns_progress_toggle_.isOn == true)
        {
            for (int i = 0; i < this.walls_columns_progress_gameobjects_.Count; i++)
            {
                if (this.walls_columns_progress_gameobjects_[i].GetComponent<MeshRenderer>() != null) this.walls_columns_progress_gameobjects_[i].GetComponent<MeshRenderer>().material.color=Color.blue;
            }
        }
        else
        {
            for (int i = 0; i < this.walls_columns_progress_gameobjects_.Count; i++)
            {
                if (this.walls_columns_progress_gameobjects_[i].GetComponent<MeshRenderer>() != null) this.walls_columns_progress_gameobjects_[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }
    public void ChangeGroundProgressVisibility()
    {
        if (slabs_progress_toggle_.isOn == true)
        {
            for (int i = 0; i < this.ground_progress_gameobjects_.Count; i++)
            {
                this.ground_progress_gameobjects_[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < this.ground_progress_gameobjects_.Count; i++)
            {
                this.ground_progress_gameobjects_[i].SetActive(false);
            }
        }
    }
    public void ChangeGridLinesVisibility()
    {
        
        if (grid_line_toggle_.isOn == true)
        {
            for (int i = 0; i < this.grid_line_gameobjects_.Count; i++)
            {
                this.grid_line_gameobjects_[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < this.grid_line_gameobjects_.Count; i++)
            {
                this.grid_line_gameobjects_[i].SetActive(false);
            }
        }

    }
    #endregion


}
