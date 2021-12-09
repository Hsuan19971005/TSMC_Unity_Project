using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using SFB;


public class ModelRenderer : MonoBehaviour
{
    #region TOGGLE
    Toggle walls_columns_toggle_;
    Toggle walls_columns_progress_toggle_;
    Toggle slabs_toggle_;
    Toggle slabs_progress_toggle_;
    Toggle grid_line_toggle_;
    Toggle others_toggle_;
    #endregion
    #region MODEL INFORMATION
    private Hashtable model_hashtable_ { get; set; }//Key=元件ID, Value=模型物件名稱
    public GameObject model_ { get; set; }//FBX Model
    private List<GameObject> walls_columns_gameobjects_ { get; set; }//All wall & column gameobjects in model
    private List<GameObject> walls_columns_current_progress_gameobjects_ { get; set; }//wall & column gameobjects which have been done in construction site
    private List<GameObject> walls_columns_recorded_progress_gameobjects_ { get; set; }//wall & column gameobjects which are recorded in file but not contained in walls_columns_current_progress_gameobjects_
    private List<GameObject> ground_gameobjects_ { get; set; }//All proxy & slab gameobjects in model
    private List<GameObject> ground_progress_gameobjects_ { get; set; }//gameobjects which are all quads in different color to present ground progress
    private List<GameObject> unanalyzed_gameobjects_ { get; set; }//gameobjects not in analysis
    private List<GameObject> grid_line_gameobjects_ { get; set; }//gameobjects which are grid lines
    private float max_y_of_ground_element { get; set; }//max y amoung gound relating components
    #endregion

    private void Awake()
    {
        this.model_hashtable_ = new Hashtable();
        this.walls_columns_gameobjects_ = new List<GameObject>();
        this.walls_columns_current_progress_gameobjects_ = new List<GameObject>();
        this.walls_columns_recorded_progress_gameobjects_ = new List<GameObject>();
        this.ground_gameobjects_ = new List<GameObject>();
        this.ground_progress_gameobjects_ = new List<GameObject>();
        this.unanalyzed_gameobjects_ = new List<GameObject>();
        this.grid_line_gameobjects_ = new List<GameObject>();
        
    }
    private void Start()
    {
        SetModel(DataBase.chosen_floor_name_);
        DeleteUnanalyzedModel();
        SetModelHashtable();
        DivideModelChildrenIntoCorrespondingGameObjects();
        SetWallsColumnsCurrentProgressGameObjects();
        this.max_y_of_ground_element = FindMaxYOfFloorElementFromModel();
        SetGroundProgressGameObjects();
        SetGridLineGameObjects();
        ChangeUnanalyzedVisibility();
        ChangeWallsColumnsProgressVisibility();

        //Recorded file's chosen floor name is the same as the analysis chosen floor name.
        if (DataBase.recorded_file_chosen_floor_name_ == DataBase.chosen_floor_name_)
        {
            //Change ground progress gameobjects' color
            ChangeRecordedGroundProgressGameObjectsColor();
            //Find walls & columns' gameobjects. And they are contained in recorded file but not contained in current progress gameobjects.
            SetWallsColumnsRecordedProgress();
            ChangeWallsColumnsProgressVisibility();
        }
        //Command line control output screenshot
        if (DataBase.output_file_path_ != null)
        {
            StartCoroutine(CommandLineScreenShot());
        }
    }
    private void Update()
    {
        
    }
    private void SetModel(string model_name)
    {
        this.model_ = GameObject.Find(model_name);
        //Disable the camera in FBX model
        for(int i = 0; i < this.model_.transform.childCount; i++)
        {
            if(this.model_.transform.GetChild(i).name== "3D 視圖: {3D}")
            {
                this.model_.transform.GetChild(i).gameObject.SetActive(false);
                return;
            }
        }
    }
    private void DeleteUnanalyzedModel()
    {
        for(int i = 0; i < DataBase.ifc_floor_data_.Count; i++)
        {
            if (DataBase.ifc_floor_data_[i].floor_name_ == DataBase.chosen_floor_name_) continue;
            GameObject gobj = GameObject.Find(DataBase.ifc_floor_data_[i].floor_name_);
            if (gobj == null) continue;
            gobj.SetActive(false);
        }
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
            //模型子物件沒有[]則跳過
            if (child_name.LastIndexOf('[') == -1 || child_name.LastIndexOf(']') == -1) continue;
            //取出[]中的ID，判斷是否是wall, column, proxy, slab類別，否則歸入unanalyzed物件類別
            //歸入walls_columns_gameobjects_, ground_gameobjects_, unanalyzed_gameobjects_的其中一個
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
    private void SetWallsColumnsCurrentProgressGameObjects()
    {
        for(int i = 0; i < this.walls_columns_gameobjects_.Count; i++)
        {
            //catch gameobject's id
            string gameobject_id = this.walls_columns_gameobjects_[i].name;
            if (gameobject_id.LastIndexOf('[') == -1 || gameobject_id.LastIndexOf(']') == -1) continue;
            gameobject_id = gameobject_id.Substring(gameobject_id.LastIndexOf('[') + 1, gameobject_id.LastIndexOf(']') - gameobject_id.LastIndexOf('[') - 1);
            //check exist and add it to this.walls_columns_gameobjects_
            int index_of_classified_point_cloud_data = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(gameobject_id);
            if (index_of_classified_point_cloud_data == -1) continue;
            if (DataBase.classified_point_cloud_data_[index_of_classified_point_cloud_data].exist_ == true) this.walls_columns_current_progress_gameobjects_.Add(this.walls_columns_gameobjects_[i]);
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
            if (point_cloud_data.exist_ == true) mesh_renderer.material.color = new Color(255f/255f,153f/255f,51f/255f,1f);//Orange
            else
            {
                mesh_renderer.material.color = Color.white;
            }
            this.ground_progress_gameobjects_.Add(obj);
            //Fake****************************************************************************
            //mesh_renderer.material.color = new Color(179f / 255f, 71f / 255f, 0f, 1);
        }

    }
    public void SetGridLineGameObjects()
    {
        //轉換該層樓的住線座標，創建柱線GameObjects，創建柱線字體
        List<Line> grid_line = DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].grid_line_;
        for (int i = 0; i < grid_line.Count; i++)
        {
            Transfer(grid_line[i]);//轉換座標
            //產生柱線
            GameObject line_gobj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Vector3 direction = new Vector3(grid_line[i].point2_x_ - grid_line[i].point1_x_, grid_line[i].point2_y_ - grid_line[i].point1_y_, grid_line[i].point2_z_ - grid_line[i].point1_z_);
            line_gobj.name = "grid_line_" + i;
            line_gobj.transform.position = new Vector3(grid_line[i].mid_point_x_, this.max_y_of_ground_element + 0.01f, grid_line[i].mid_point_z_);
            line_gobj.transform.localScale = new Vector3(0.1f, direction.magnitude / 2f, 0.1f);
            line_gobj.transform.up = direction;
            line_gobj.GetComponent<MeshRenderer>().material.color = Color.black;
            this.grid_line_gameobjects_.Add(line_gobj);

            //產生柱線字體
            GameObject line_name_gobj1=new GameObject();
            line_name_gobj1.name = grid_line[i].name_+"_1";
            line_name_gobj1.transform.position = line_gobj.transform.position+direction/1.95f;
            line_name_gobj1.transform.rotation = Quaternion.Euler(new Vector3(90f, 0, 0));
            line_name_gobj1.AddComponent<MeshRenderer>();
            line_name_gobj1.AddComponent<TextMesh>();
            line_name_gobj1.GetComponent<TextMesh>().text = grid_line[i].name_;
            line_name_gobj1.GetComponent<TextMesh>().characterSize = 2;
            line_name_gobj1.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            line_name_gobj1.GetComponent<TextMesh>().alignment = TextAlignment.Center;
            line_name_gobj1.GetComponent<TextMesh>().color = Color.white;
            this.grid_line_gameobjects_.Add(line_name_gobj1);

            GameObject line_name_gobj2 = new GameObject();
            line_name_gobj2.name = grid_line[i].name_ + "_2";
            line_name_gobj2.transform.position = line_gobj.transform.position + direction / 1.95f*-1f;
            line_name_gobj2.transform.rotation = Quaternion.Euler(new Vector3(90f, 0, 0));
            line_name_gobj2.AddComponent<MeshRenderer>();
            line_name_gobj2.AddComponent<TextMesh>();
            line_name_gobj2.GetComponent<TextMesh>().text = grid_line[i].name_;
            line_name_gobj2.GetComponent<TextMesh>().characterSize = 2;
            line_name_gobj2.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            line_name_gobj2.GetComponent<TextMesh>().alignment = TextAlignment.Center;
            line_name_gobj2.GetComponent<TextMesh>().color = Color.white;
            this.grid_line_gameobjects_.Add(line_name_gobj2);
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
    public void ChangeRecordedGroundProgressGameObjectsColor()
    {
        for (int i = 0; i < DataBase.recorded_file_grid_id_name_.Count; i++)
        {
            GameObject.Find(DataBase.recorded_file_grid_id_name_[i]).GetComponent<MeshRenderer>().material.color = new Color(179f / 255f, 71f / 255f, 0f, 1);//Brown
        }
    }
    public void SetWallsColumnsRecordedProgress()
    {
        for (int i = 0; i < DataBase.recorded_file_wall_id_name_.Count; i++)
        {
            string gobj_name = this.model_hashtable_[DataBase.recorded_file_wall_id_name_[i]].ToString();
            GameObject gobj = GameObject.Find(gobj_name);
            this.walls_columns_recorded_progress_gameobjects_.Add(gobj);
            if (this.walls_columns_current_progress_gameobjects_.IndexOf(gobj) != -1) this.walls_columns_current_progress_gameobjects_.RemoveAt(this.walls_columns_current_progress_gameobjects_.IndexOf(gobj));//current progress gameobjects contain previous wall preogress gameobject; thus, remove it from current progress gameobjects.
        }
        for (int i = 0; i < DataBase.recorded_file_column_id_name_.Count; i++)
        {
            string gobj_name = this.model_hashtable_[DataBase.recorded_file_column_id_name_[i]].ToString();
            GameObject gobj = GameObject.Find(gobj_name);
            this.walls_columns_recorded_progress_gameobjects_.Add(gobj);
            if (this.walls_columns_current_progress_gameobjects_.IndexOf(gobj) != -1) this.walls_columns_current_progress_gameobjects_.RemoveAt(this.walls_columns_current_progress_gameobjects_.IndexOf(gobj));//current progress gameobjects contain previous column preogress gameobject; thus, remove it from current progress gameobjects.
        }
    }
    IEnumerator CommandLineScreenShot()
    {
        Camera myCam = Camera.main;
        myCam.transform.position = new Vector3(-80, 250, -200);
        myCam.transform.rotation = Quaternion.Euler(new Vector3(60, 0, 0));
        GameObject.Find("SreenShot_InputField").GetComponent<InputField>().text = DataBase.output_file_path_;
        GameObject.Find("Screen_Shot_Button").GetComponent<Button>().onClick.Invoke();
        yield return null;
        GameObject.Find("Reposition_Button").GetComponent<Button>().onClick.Invoke();
    }

    #region FUNCTIONS FOR TOGGLES
    public void ChangeWallsColumnsVisibility()
    {
        if (GameObject.Find("Walls_Columns_Toggle").GetComponent<Toggle>().isOn == true)
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
        if (GameObject.Find("Slabs_Toggle").GetComponent<Toggle>().isOn == true)
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
        if (GameObject.Find("Others_Toggle").GetComponent<Toggle>().isOn == true)
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
        if (GameObject.Find("Walls_Columns_Progress_Toggle").GetComponent<Toggle>().isOn == true)
        {
            for (int i = 0; i < this.walls_columns_current_progress_gameobjects_.Count; i++)
            {
                if (this.walls_columns_current_progress_gameobjects_[i].GetComponent<MeshRenderer>() != null) this.walls_columns_current_progress_gameobjects_[i].GetComponent<MeshRenderer>().material.color=new Color(255f / 255f, 153f / 255f, 51f / 255f, 1f);//Orange
            }
            for(int i = 0; i < this.walls_columns_recorded_progress_gameobjects_.Count; i++)
            {
                if (this.walls_columns_recorded_progress_gameobjects_[i].GetComponent<MeshRenderer>() != null) this.walls_columns_recorded_progress_gameobjects_[i].GetComponent<MeshRenderer>().material.color = new Color(179f / 255f, 71f / 255f, 0f, 1);//Brown
            }
        }
        else
        {
            for (int i = 0; i < this.walls_columns_current_progress_gameobjects_.Count; i++)
            {
                if (this.walls_columns_current_progress_gameobjects_[i].GetComponent<MeshRenderer>() != null) this.walls_columns_current_progress_gameobjects_[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
            for (int i = 0; i < this.walls_columns_recorded_progress_gameobjects_.Count; i++)
            {
                if (this.walls_columns_recorded_progress_gameobjects_[i].GetComponent<MeshRenderer>() != null) this.walls_columns_recorded_progress_gameobjects_[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }
    public void ChangeGroundProgressVisibility()
    {
        if (GameObject.Find("Slabs_Progress_Toggle").GetComponent<Toggle>().isOn == true)
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
        
        if (GameObject.Find("Grid_Line_Toggle").GetComponent<Toggle>().isOn == true)
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
    #region FUNCTIONS FOR BUTTONS
    public void OutputRecordedFile()
    {
        string data_path = GameObject.Find("OutputFile_InputField").GetComponent<InputField>().text;
        if (!Directory.Exists(data_path)) return;
        if (data_path[data_path.Length - 1] == '/') data_path.Remove(data_path.Length-1,1);
        StreamWriter sw = new StreamWriter(data_path+"/ConstructionProgressRecordedFile.txt");
        sw.WriteLine("#TSMC BIM LIDAR PROGRESS TRACKING#");
        sw.WriteLine(DataBase.point_cloud_file_path_);
        sw.WriteLine(DataBase.chosen_floor_name_);
        sw.WriteLine(DataBase.date_[0] + "/" + DataBase.date_[1] + "/" + DataBase.date_[2]);
        //Finished wall. Leave its id behind #WALL=
        List<string> wall_id = DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].WorkItemDataIfcWallStandardCase_.id_name_;
        for (int i = 0; i < wall_id.Count; i++)
        {
            int index = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(wall_id[i]);
            if (index == -1) continue;
            if (DataBase.classified_point_cloud_data_[index].exist_ == true) sw.WriteLine("#WALL=" + wall_id[i]);
        }
        //Finished column. Leave its id behind #COLUMN=
        List<string> column_id = DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].WorkItemDataIfcColumn_.id_name_;
        for (int i = 0; i < column_id.Count; i++)
        {
            int index = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(column_id[i]);
            if (index == -1) continue;
            if (DataBase.classified_point_cloud_data_[index].exist_ == true) sw.WriteLine("#COLUMN=" + column_id[i]);
        }
        //Finished ground quad. Leave its id behind #GROUND=
        List<string> grid_id = DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].WorkItemDataGrid_.id_name_;
        for (int i = 0; i < grid_id.Count; i++)
        {
            int index = DataBase.FindIndexOfClassifiedPointCloudDataByIdName(grid_id[i]);
            if (index == -1) break;
            if (DataBase.classified_point_cloud_data_[index].exist_ == true) sw.WriteLine("#GROUND=" + grid_id[i]);
        }
        //Contain recorded file progress
        if (DataBase.recorded_file_chosen_floor_name_ == DataBase.chosen_floor_name_)
        {
            for(int i = 0; i < DataBase.recorded_file_wall_id_name_.Count; i++)
            {
                sw.WriteLine("#WALL=" + DataBase.recorded_file_wall_id_name_[i]);
            }
            for(int i = 0; i < DataBase.recorded_file_column_id_name_.Count; i++)
            {
                sw.WriteLine("#COLUMN=" + DataBase.recorded_file_column_id_name_[i]);
            }
            for(int i = 0; i < DataBase.recorded_file_grid_id_name_.Count; i++)
            {
                sw.WriteLine("#GROUND=" + DataBase.recorded_file_grid_id_name_[i]);
            }
        }
        sw.Flush();
        sw.Close();
        Debug.Log("Output is done.");
    }
    public void ScreenShot()
    {
        string data_path = GameObject.Find("SreenShot_InputField").GetComponent<InputField>().text;
        if (!Directory.Exists(data_path)) return;
        if (data_path[data_path.Length - 1] == '/') data_path.Remove(data_path.Length-1,1);
        string time = Convert.ToString(System.DateTime.Now);
        for(int i = 0; i < time.Length; i++)
        {
            if(time[i]=='/' || time[i] == ':')
            {
                time = time.Remove(i, 1);
                i--;
            }
        }
        ScreenCapture.CaptureScreenshot(data_path+"/ConstructionProgressShot"+time+".png");
    }
    public void ScreenShotFileBrowser()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Screen shot folder", "", false);
        GameObject.Find("SreenShot_InputField").GetComponent<InputField>().text = paths[0];
    }
    public void OutputRecordedFileBrowser()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Recorded file folder", "", false);
        GameObject.Find("OutputFile_InputField").GetComponent<InputField>().text = paths[0];
    }
    #endregion


}
