using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    #region UI COMPONENT
    Text state_text_;
    Dropdown floor_choice_dropdown_;
    Text[] scroll_view_content_;
    Button start_analyze_button_;
    Button change_scene_button_;
    Toggle recorded_file_toggle_;
    #endregion

    #region TASKS CONTROLL
    private bool can_read_point_cloud_;//控制是否可以開始讀取真實點雲檔
    private bool can_read_recorded_file;//控制是否可以開始讀取記錄檔
    private bool can_classify_point_cloud_;//控制是否可以開始點雲分類
    private bool can_analyzing_construction_progresss_;//控制是否可以開始計算進度結果
    #endregion

    [SerializeField] public string fbx_name_;//分析之模型名稱
    private float radius_ = 0.30f;//點雲分類使用之球狀空間半徑
    private string real_points_path_;//真實點雲檔路徑
    private string recorded_file_path_;//記錄檔路徑
    [SerializeField] private string ifc_file_name;//IFC檔位於StreamingAssets的名稱
    private string ifc_file_path_;//IFC檔路徑
    InitiateProject initiate = new InitiateProject();

    private void Awake()
    {
        this.state_text_ = GameObject.Find("State_Text").GetComponent<Text>();
        this.floor_choice_dropdown_ = GameObject.Find("Floor_Choice_Dropdown").GetComponent<Dropdown>();
        this.scroll_view_content_ = new Text[3];
        this.scroll_view_content_[0] = GameObject.Find("IFC_Content").transform.Find("Floor_Text").GetComponent<Text>();
        this.scroll_view_content_[1] = GameObject.Find("IFC_Content").transform.Find("Walls_Text").GetComponent<Text>();
        this.scroll_view_content_[2] = GameObject.Find("IFC_Content").transform.Find("Columns_Text").GetComponent<Text>();
        this.start_analyze_button_ = GameObject.Find("Start_Analysis_Button").GetComponent<Button>();
        this.change_scene_button_ = GameObject.Find("Change_Scene_Button").GetComponent<Button>();
        this.recorded_file_toggle_ = GameObject.Find("Recorded_File_Toggle").GetComponent<Toggle>();

        this.can_read_point_cloud_ = false;
        this.can_classify_point_cloud_ = false;
        this.can_analyzing_construction_progresss_ = false;
    }
    void Start()
    {
        //讀取IFC檔案
        this.ifc_file_path_ = Application.streamingAssetsPath + "/" + this.ifc_file_name + ".ifc";
        initiate.OpenAndProcessIfcFile(this.ifc_file_path_);
        initiate.SetIfcFloorData();
        initiate.SetFloorDataCorrespondingIFCGRID(initiate.ifc_floor_data_, initiate.ifc_code_of_ifcgrid_);
        DataBase.ifc_floor_data_ = initiate.GetIfcFloorData();
        //Set for dropdown
        this.floor_choice_dropdown_.ClearOptions();
        for (int i = 0; i < DataBase.ifc_floor_data_.Count; i++)
        {
            this.floor_choice_dropdown_.AddOptions(new List<Dropdown.OptionData> { new Dropdown.OptionData(DataBase.ifc_floor_data_[i].floor_name_) });
        }
        DataBase.chosen_floor_name_ = DataBase.ifc_floor_data_[this.floor_choice_dropdown_.value].floor_name_;
        this.floor_choice_dropdown_.onValueChanged.AddListener(delegate { DropdownItemSelected(this.floor_choice_dropdown_); });
        //Set for scroll view
        for (int i = 0; i < DataBase.ifc_floor_data_.Count; i++)
        {
            this.scroll_view_content_[0].text += DataBase.ifc_floor_data_[i].floor_name_ + "\n";
            this.scroll_view_content_[1].text += DataBase.ifc_floor_data_[i].WorkItemDataIfcWallStandardCase_.id_name_.Count + "\n";
            this.scroll_view_content_[2].text += DataBase.ifc_floor_data_[i].WorkItemDataIfcColumn_.id_name_.Count + "\n";
        }
    }
    void Update()
    {
        /**點雲檔路徑正確，Start Button修改了bool時，開始讀檔
        */
        if (this.can_read_point_cloud_ == true)
        {
            this.can_read_point_cloud_ = false;
            this.state_text_.text += "Start reading point cloud file." + System.DateTime.Now + "\n";
            gameObject.GetComponent<ProgressBarController>().TurnOnProgressBar();//開啟進度條功能
            gameObject.GetComponent<CircleProgress>().StartCircleProgress();//啟動運轉圓圈
            StartCoroutine(JobsOfReadingPointCloud());
        }
        /**讀檔完成，使用者按下StarAnalyze Button，開始分類點雲
         * 包含所有點雲分類至各元件、取出樓板點雲依照柱線分割、回傳分類分割結果至DataBase
         * this.can_classify_point_cloud由StarAnalyze Button控制*/
        if (this.can_classify_point_cloud_ == true)
        {
            this.can_classify_point_cloud_ = false;
            StartCoroutine(JobsOfClassifyingAllPointCloud());
        }
        /**點雲整體分類、分割完成，開始計算工程進度
         * 工程進度計算完成即可啟動轉換場景的Button
         * this.can_analyzing_construction_progresss_由JobsOfBasicClassifyingPointCloud()執行結束時控制*/
        if (this.can_analyzing_construction_progresss_ == true)
        {
            gameObject.GetComponent<ProgressBarController>().SetProgressBar95Percent();//設定進度條進度95%
            gameObject.GetComponent<CircleProgress>().EndCircleProgress();//結束運轉圓圈
            this.can_analyzing_construction_progresss_ = false;
            var progress = new ConstructionProgress(DataBase.classified_point_cloud_data_, DataBase.ifc_floor_data_);
            int index_of_user_input_floor_name = progress.FindFloorDataIndex(DataBase.chosen_floor_name_, DataBase.ifc_floor_data_);
            progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataIfcColumn_);
            progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataIfcWallStandardCase_);
            progress.DoOneWorkItemDataProgress(DataBase.ifc_floor_data_[index_of_user_input_floor_name].WorkItemDataGrid_);
            gameObject.GetComponent<ProgressBarController>().SetProgressBar100Percent();//設定進度條進度100%
            this.change_scene_button_.interactable = true;
        }
    }
    #region METHODS ATTACHED TO UI
    public void ChangeSceneButton()
    {
        SceneManager.LoadScene("AnalysisResultScene");
    }
    public void StartAnalyzeButton()
    {
        
        DataBase.date_[0] = GameObject.Find("Year_InputField").GetComponent<InputField>().text;//Date:year to DataBase
        DataBase.date_[1] = GameObject.Find("Month_InputField").GetComponent<InputField>().text;//Date:month to DataBase
        DataBase.date_[2] = GameObject.Find("Date_InputField").GetComponent<InputField>().text;//Date:day to DataBase
        string real_points_path = GameObject.Find("Point_Cloud_InputField").GetComponent<InputField>().text;
        string recored_file_path = GameObject.Find("Recorded_File_InputField").GetComponent<InputField>().text;
        bool real_points_state = false;//Check whether real point cloud file can be read.
        bool recorded_state = true;//Check whether recorded file can be read.
        if (!File.Exists(real_points_path))this.state_text_.text += "Point cloud file path doesn't exist.\n";
        else real_points_state = true;
        if (this.recorded_file_toggle_.isOn == true && !File.Exists(recored_file_path)) {
            this.state_text_.text += "Recorded file path doesn't exist.\n";
            recorded_state = false;
        }
        if(real_points_state&&recorded_state)
        {
            //set file paths
            this.real_points_path_ = real_points_path;
            this.recorded_file_path_ = recored_file_path;
            //activate tasks
            this.can_read_point_cloud_ = true;
            this.can_read_recorded_file = true;
            //lock UI components
            GameObject.Find("Year_InputField").GetComponent<InputField>().interactable = false;//lock Date:year
            GameObject.Find("Month_InputField").GetComponent<InputField>().interactable = false;//lock Date:month
            GameObject.Find("Date_InputField").GetComponent<InputField>().interactable = false;//lock Date:day
            GameObject.Find("Recorded_File_InputField").GetComponent<InputField>().interactable = false;//lock Recorded_file_inputField
            this.recorded_file_toggle_.interactable = false;//lock recored_file_toggle
        }
    }
    public void DetectRecordedToggleState()
    {
        //如果Recorded File打勾，Recorded輸入欄位互動開啟
        if (this.recorded_file_toggle_.isOn == true)
        {
            GameObject.Find("Recorded_File_Button").GetComponent<Button>().interactable = true;
            GameObject.Find("Recorded_File_InputField").GetComponent<InputField>().interactable = true;
        }
        else
        {
            GameObject.Find("Recorded_File_Button").GetComponent<Button>().interactable = false;
            GameObject.Find("Recorded_File_InputField").GetComponent<InputField>().interactable = false;
        }
    }
    #endregion
    #region COROUTINE
    IEnumerator JobsOfReadingPointCloud()
    {
        yield return null;
        //Read point cloud
        initiate.SetRealPoints(this.real_points_path_);
        initiate.CreateClassifiedPointCloudDataByModel(DataBase.chosen_floor_name_);
        initiate.DeleteClassifiedPointCloudDataNotInIfcFloorData(initiate.classified_point_cloud_data_, initiate.all_id_);
        yield return null;
        if (this.can_read_recorded_file == true && this.recorded_file_toggle_.isOn==true)
        {
            ReadRecordedFile(this.recorded_file_path_);
        }
        //Return data to DataBase
        DataBase.real_points_ = initiate.GetRealPoints();
        DataBase.classified_point_cloud_data_ = initiate.GetClassifiedPointCloudData();
        //Show Analysis Estimated Time
        gameObject.GetComponent<ProgressBarController>().ShowEstimatedTime();
        //Can start classify point cloud
        this.can_classify_point_cloud_ = true;
        this.state_text_.text += "Start analyzing point cloud.\n";
        //Point cloud file format is correct, turn off some UI components.
        this.start_analyze_button_.interactable = false;
        this.floor_choice_dropdown_.interactable = false;
    }
    IEnumerator JobsOfClassifyingAllPointCloud()
    {
        //點雲基本分類
        yield return StartCoroutine(JobsOfBasicClassifyingPointCloud());
        this.state_text_.text += "Basic analysis is done. \nStart analyzing slab points."+System.DateTime.Now+"\n";
        //樓板點雲分割前置作業
        gameObject.GetComponent<GroundCutting>().SetAllGridLines(DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].grid_line_);
        gameObject.GetComponent<GroundCutting>().SetGridsContainingPoints();
        gameObject.GetComponent<GroundCutting>().SetWorkItemDataGrid();
        gameObject.GetComponent<GroundCutting>().CollectGroundPointsByOneIfcFloorData(DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()]);
        //樓板點雲分割
        yield return StartCoroutine(gameObject.GetComponent<GroundCutting>().ClassifyGroundPointsIntoGridsContainingPoints());
        gameObject.GetComponent<GroundCutting>().SetGridClassifiedPointCloudData();
        //設定分割結果至資料中
        DataBase.classified_point_cloud_data_.AddRange(gameObject.GetComponent<GroundCutting>().GetGridClassifiedPointCloudData());
        DataBase.ifc_floor_data_[DataBase.FindIndexOfIfcFloorDataByFloorNameByChosenFloorName()].WorkItemDataGrid_ = gameObject.GetComponent<GroundCutting>().GetWorkItemDataGrid();
        this.state_text_.text += "Whole analysis is done."+System.DateTime.Now+"\nPush the button to see the analysis result.\n";
        this.can_analyzing_construction_progresss_ = true;
    }
    IEnumerator JobsOfBasicClassifyingPointCloud()
    {
        //點雲依照Model分類
        gameObject.GetComponent<Classifier>().SetRadius(this.radius_);
        gameObject.GetComponent<Classifier>().SetClassifiedPointCloudData(DataBase.classified_point_cloud_data_);
        gameObject.GetComponent<Classifier>().SetRealPoints(DataBase.real_points_);
        yield return StartCoroutine(gameObject.GetComponent<Classifier>().ClassifyRealPointsIntoClassifiedPointCloudData());
    }
    #endregion
    private void DropdownItemSelected(Dropdown dropdown)
    {
        DataBase.chosen_floor_name_ = DataBase.ifc_floor_data_[dropdown.value].floor_name_;
    }
    private void ReadRecordedFile(string path)
    {
        StreamReader sr = new StreamReader(path);
        if (sr.ReadLine() != "#TSMC BIM LIDAR PROGRESS TRACKING#")
        {
            this.state_text_.text += "Recorded file format is wrong! Please check it.";
            return;
        }
        DataBase.recorded_file_chosen_floor_name_ = sr.ReadLine();//Chosen floor name of recorded file
        DataBase.recorded_file_date_ = sr.ReadLine();//Analysis input date of recorded file
        while (!sr.EndOfStream)
        {
            string content = sr.ReadLine();
            string head_content = content.Substring(0, content.IndexOf("="));
            if (head_content == "#WALL")
            {
                DataBase.recorded_file_wall_id_name_.Add(content.Remove(0,content.IndexOf("=")+1));
            }
            else if (head_content == "#COLUMN")
            {
                DataBase.recorded_file_column_id_name_.Add(content.Remove(0, content.IndexOf("=") + 1));
            }
            else if (head_content == "#GROUND")
            {
                DataBase.recorded_file_grid_id_name_.Add(content.Remove(0, content.IndexOf("=") + 1));
            }
        }
    }

}
