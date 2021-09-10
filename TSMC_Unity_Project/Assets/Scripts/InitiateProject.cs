using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using UnityEngine.Networking;

/**InitiateProject 功能說明
 * 1. 讀取IFC檔
 * 2. 讀取真實點雲
 * 3. 建置ifc_floor_data_，並傳入至DataBase
 * 4. 建置real_points_，並傳入至DataBase
 */
public class InitiateProject : MonoBehaviour
{
    public List<float[]> real_points_ { get; set; }//儲存真實點雲 x y z
    public List<ClassifiedPointCloudData> classified_point_cloud_data_ { get; set; }
    public List<string> ifc_code_of_ifcrelcontainedinspatialstructure_ { get; set; }//樓層的ifc code
    public List<string> ifc_code_of_ifcgrid_ { get; set; }//IFCGRID網格的ifc code
    public List<List<string>> ifc_all_data_ { get; set; }//ifc每一行的資料，已被拆解處理
    public Hashtable ifc_hashtable_ { get; set; }//ifc的hashtable
    public List<FloorData> ifc_floor_data_ { get; set; }//樓層為基準之整體資訊
    public List<string> all_id_ { get; set; }//記錄所有待分析的ID名稱
    public InitiateProject(string ifc_file_path, string real_points_path ,string fbx_model_name)
    {
        this.real_points_ = new List<float[]>();
        this.classified_point_cloud_data_ = new List<ClassifiedPointCloudData>();
        this.ifc_code_of_ifcrelcontainedinspatialstructure_ = new List<string>();
        this.ifc_code_of_ifcgrid_ = new List<string>();
        this.ifc_all_data_ = new List<List<string>>();
        this.ifc_hashtable_ = new Hashtable();
        this.ifc_floor_data_ = new List<FloorData>();
        this.all_id_ = new List<string>();
        OpenAndProcessIfcFile(ifc_file_path);
        SetIfcFloorData();
        SetFloorDataCorrespondingIFCGRID(this.ifc_floor_data_, this.ifc_code_of_ifcgrid_);
        SetRealPoints(real_points_path);
        CreateClassifiedPointCloudDataByModel(fbx_model_name);
        DeleteClassifiedPointCloudDataNotInIfcFloorData(this.classified_point_cloud_data_, this.all_id_);
        //開始GroundCutting


    }
    public void OpenAndProcessIfcFile(string file_path)
    {
        if (!File.Exists(file_path))
        {
            Debug.Log("找不到IFC檔案位置：" + file_path);
            return;
        }
        StreamReader sr = new StreamReader(file_path);
        //跳過前面的ifc版本資訊
        while (true)
        {
            if (sr.ReadLine() == "DATA;") break;
        }
        int index_number = 0;//用以製作hashtable的value
                             //開始讀取IFC Model內容並製作ifc_hashtable
        while (true)
        {
            string context_of_line = sr.ReadLine();
            if (context_of_line == "ENDSEC;") break;//到最後一行跳出
            List<string> context_of_line_seperate = SplitIfcFileOneLine(context_of_line);//對該行進行分解
            if (context_of_line_seperate[1] == "IFCRELCONTAINEDINSPATIALSTRUCTURE") this.ifc_code_of_ifcrelcontainedinspatialstructure_.Add(context_of_line_seperate[0]);//紀錄出現IFCRELCONTAINEDINSPATIALSTRUCTURE的ifc_code
            if (context_of_line_seperate[1] == "IFCGRID") this.ifc_code_of_ifcgrid_.Add(context_of_line_seperate[0]);//紀錄出現IFCGRID的ifc_code
            ifc_all_data_.Add(context_of_line_seperate);
            this.ifc_hashtable_.Add(context_of_line_seperate[0], index_number);
            index_number++;
        }
    }
    public List<string> SplitIfcFileOneLine(string context_of_line)
    {
        List<string> final_list = new List<string>();
        //分割第一個 "#XXXX= "
        final_list.Add(context_of_line.Substring(0, context_of_line.IndexOf("=")));
        context_of_line = context_of_line.Remove(0, context_of_line.IndexOf("=") + 2);//移除到 = 後面一個字元
        //分割 "IFCXXXXX("
        final_list.Add(context_of_line.Substring(0, context_of_line.IndexOf("(")));
        context_of_line = context_of_line.Remove(0, context_of_line.IndexOf("(") + 1);//移除到 (
        context_of_line = context_of_line.Remove(context_of_line.LastIndexOf(")"), 2);//去除 ");"
        List<string> list = new List<string>(context_of_line.Split(','));
        RemoveSpaceInListOfString(list);
        final_list.AddRange(list);
        return final_list;
    }
    public void SetIfcFloorData()
    {
        for (int i = 0; i < ifc_code_of_ifcrelcontainedinspatialstructure_.Count; i++)
        {
            this.ifc_floor_data_.Add(CreatOneFloorData(ifc_code_of_ifcrelcontainedinspatialstructure_[i]));
        }
    }
    public FloorData CreatOneFloorData(string ifc_code)
    {
        FloorData floor_data = new FloorData();
        floor_data.floor_ifc_code_ = ifc_code;
        SearchInIFCRELCONTAINEDINSPATIALSTRUCTUR(floor_data, ifc_code);
        return floor_data;
    }
    public void SearchInIFCRELCONTAINEDINSPATIALSTRUCTUR(FloorData floor_data, string ifc_code)
    {
        List<string> list = ifc_all_data_[Convert.ToInt32(ifc_hashtable_[ifc_code])];
        for (int i = 0; i < list.Count; i++)
        {
            List<string> list_after_split =new List<string>(list[i].Split('(', ')'));//去除可能存在的"(",")"
            RemoveSpaceInListOfString(list_after_split);
            if (list_after_split[0].Length >= 1 && list_after_split[0][0] == '#') LinkToIfcNumberAndDecideToAcitvateSetFloorDataMethod(floor_data, list_after_split[0]);
        }
    }
    public void LinkToIfcNumberAndDecideToAcitvateSetFloorDataMethod(FloorData floor_data, string ifc_code)
    {
        List<string> list = this.ifc_all_data_[Convert.ToInt32(ifc_hashtable_[ifc_code])];
        //Console.WriteLine("確認名稱:"+list[1]);
        if (list[1] == "IFCCOLUMN") { SetOneFloorDataInIFCCOLUMN(floor_data, ifc_code); }
        else if (list[1] == "IFCWALLSTANDARDCASE") { SetOneFloorDataInIFCWALLSTANDARDCASE(floor_data, ifc_code); }
        else if (list[1] == "IFCBUILDINGSTOREY") { SetOneFloorDataInIFCBUILDINGSTOREY(floor_data, ifc_code); }
        else if (list[1] == "IFCSLAB") { SetOneFloorDataInIFCSLAB(floor_data, ifc_code); }
        else if (list[1] == "IFCBUILDINGELEMENTPROXY") { SetOneFloorDataInIFCBUILDINGELEMENTPROXY(floor_data, ifc_code); }

    }
    public void SetOneFloorDataInIFCCOLUMN(FloorData floor_data, string ifc_code)
    {
        List<string> one_line_ifc_all_data = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[ifc_code])];
        List<string> list_for_id = new List<string>(one_line_ifc_all_data[9].Split('\''));//分離"'"，取柱ID
        RemoveSpaceInListOfString(list_for_id);
        floor_data.WorkItemDataIfcColumn_.id_name_.Add(list_for_id[0]);//寫入柱ID
        this.all_id_.Add(list_for_id[0]);
    }
    public void SetOneFloorDataInIFCWALLSTANDARDCASE(FloorData floor_data, string ifc_code)
    {
        List<string> one_line_ifc_all_data = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[ifc_code])];
        List<string> list_for_id = new List<string>(one_line_ifc_all_data[9].Split('\''));//分離"'"，取牆ID
        RemoveSpaceInListOfString(list_for_id);
        floor_data.WorkItemDataIfcWallStandardCase_.id_name_.Add(list_for_id[0]);//寫入牆ID
        this.all_id_.Add(list_for_id[0]);
    }
    public void SetOneFloorDataInIFCBUILDINGSTOREY(FloorData floor_data, string ifc_code)
    {
        List<string> one_line_ifc_all_data = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[ifc_code])];
        List<string> list_for_floor_name = new List<string>(one_line_ifc_all_data[4].Split('\''));//取樓層
        RemoveSpaceInListOfString(list_for_floor_name);
        floor_data.floor_name_ = list_for_floor_name[0];//寫入樓層
        floor_data.floor_height_level_ = float.Parse(one_line_ifc_all_data[11]);//寫入樓層高度
    }
    public void SetOneFloorDataInIFCSLAB(FloorData floor_data, string ifc_code)
    {
        List<string> one_line_ifc_all_data = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[ifc_code])];
        List<string> list_for_id = new List<string>(one_line_ifc_all_data[9].Split('\''));//分離"'"，取版ID
        RemoveSpaceInListOfString(list_for_id);
        //IFC中版的ID有可能重複，故先確認是否重複，若重複，則不再增加ID名稱
        foreach (var i in floor_data.WorkItemDataIfcSlab_.id_name_)
        {
            if (i == list_for_id[0]) return;
        }
        floor_data.WorkItemDataIfcSlab_.id_name_.Add(list_for_id[0]);//寫入版ID
        this.all_id_.Add(list_for_id[0]);
    }
    public void SetOneFloorDataInIFCBUILDINGELEMENTPROXY(FloorData floor_data, string ifc_code)
    {
        List<string> one_line_ifc_all_data = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[ifc_code])];
        List<string> list_for_id = new List<string>(one_line_ifc_all_data[9].Split('\''));//分離"'"，取Proxy ID
        RemoveSpaceInListOfString(list_for_id);
        floor_data.WorkItemDataIfcBuildingElementProxy_.id_name_.Add(list_for_id[0]);//寫入Proxy ID
        this.all_id_.Add(list_for_id[0]);
    }
    public void SetOneFloorDataInIFCGRID(FloorData floor_data, string ifc_code)
    {
        List<string> one_line_of_ifc_all_data = ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[ifc_code])];//依照ifc_code取得該行資料
        if (one_line_of_ifc_all_data[1] == "IFCPOLYLINE")
        {
            Line final_line = new Line();
            //IFCCARTESIANPOINT 1
            List<string> cartesian1 = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[one_line_of_ifc_all_data[2].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]])];//依照ifc_code取得第一個IFCCARTESIANPOINT資料
            final_line.point1_x_ = float.Parse(cartesian1[2].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]) / 1000f;
            final_line.point1_y_ = float.Parse(cartesian1[3].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]) / 1000f;
            //IFCCARTESIANPOINT 2
            List<string> cartesian2 = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[one_line_of_ifc_all_data[3].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]])];//依照ifc_code取得第一個IFCCARTESIANPOINT資料
            final_line.point2_x_ = float.Parse(cartesian2[2].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]) / 1000f;
            final_line.point2_y_ = float.Parse(cartesian2[3].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]) / 1000f;

            final_line.SetMidPoint();
            floor_data.grid_line_.Add(final_line);
        }
        else if (one_line_of_ifc_all_data[1] == "IFCOWNERHISTORY" || one_line_of_ifc_all_data[1] == "IFCPRODUCTDEFINITIONSHAPE" || one_line_of_ifc_all_data[1] == "IFCLOCALPLACEMENT");
        else
        {
            for (int i = 2; i < one_line_of_ifc_all_data.Count; i++)
            {
                string[] array = one_line_of_ifc_all_data[i].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);//去除可能存在的"(",")"
                if (array[0].Length >= 1 && array[0][0] == '#') SetOneFloorDataInIFCGRID(floor_data, array[0]);
            }
        }
        return;
    }
    public void SetRealPoints(string file_path)
    {
        if (!File.Exists(file_path)) Console.WriteLine("找不到真實點雲位置：" + file_path);
        else ReadPoints(file_path, this.real_points_);
    }
    public void ReadPoints(string path, List<float[]> points)
    {
        StreamReader sr = new StreamReader(path);
        while (!sr.EndOfStream)
        {
            float[] tmp_array_float = new float[3];
            string[] tmp_array_string = sr.ReadLine().Split(' ');
            for (int i = 0; i < 3; i++) tmp_array_float[i] = float.Parse(tmp_array_string[i]);
            points.Add(tmp_array_float);
        }
        sr.Close();
    }
    public int FindIndexOfClassifiedPointCloudDataByIdName(string id_name, List<ClassifiedPointCloudData> classified_point_cloud_data)
    {
        for (int i = 0; i < classified_point_cloud_data.Count; i++)
        {
            if (classified_point_cloud_data[i].id_name_ == id_name) return i;
        }
        return -1;
    }
    public int FindIndexOfFloorDataByFloorName(string floor_name, List<FloorData> floor_data)
    {
        for (int i = 0; i < floor_data.Count; i++)
        {
            if (floor_name == floor_data[i].floor_name_) return i;
        }
        return -1;
    }
    public void DeleteClassifiedPointCloudDataNotInIfcFloorData(List<ClassifiedPointCloudData> classified_point_cloud_data, List<string> id_name)
    {
        for (int i = 0; i < classified_point_cloud_data.Count; i++)
        {
            if (!id_name.Contains(classified_point_cloud_data[i].id_name_))
            {
                classified_point_cloud_data.RemoveAt(i);
                i--;
            }
            
        }
    }
    public void RemoveSpaceInListOfString(List<string>list_of_string)
    {
        for(int i = 0; i < list_of_string.Count; i++)
        {
            if (list_of_string[i] == "")
            {
                list_of_string.RemoveAt(i);
                i--;
            }
        }
        return;
    }
    public void CreateClassifiedPointCloudDataByModel(string fbx_model_name)
    {
        GameObject father_gameobject = GameObject.Find(fbx_model_name);
        for(int i = 0; i < father_gameobject.transform.childCount; i++)
        {
            string child_name = father_gameobject.transform.GetChild(i).gameObject.name;
            if (child_name.Length <= 2) continue;
            else if (child_name.LastIndexOf('[') == -1 || child_name.LastIndexOf(']')==-1) continue;
            string id = child_name.Substring(child_name.LastIndexOf('[') + 1, child_name.LastIndexOf(']') - child_name.LastIndexOf('[') - 1);
            ClassifiedPointCloudData data = new ClassifiedPointCloudData();
            data.id_name_ = id;
            this.classified_point_cloud_data_.Add(data);
        }
    }
    //Get系列
    public List<FloorData> GetIfcFloorData()
    {
        return this.ifc_floor_data_;
    }
    public List<ClassifiedPointCloudData> GetClassifiedPointCloudData()
    {
        return this.classified_point_cloud_data_;
    }
    public List<float[]> GetRealPoints()
    {
        return this.real_points_;
    }
    //IFC Grid 系列
    public void SetFloorDataCorrespondingIFCGRID(List<FloorData> all_floor_data, List<string> ifc_code_of_ifcgrid)
    {
        for (int i = 0; i < all_floor_data.Count; i++)
        {
            string corresponding_ifcgrid = null;//儲存該樓層對應之IFCGRID的ifc_code
                                                //尋找該樓層對應之IFCGRID的ifc_code
            for (int j = 0; j < ifc_code_of_ifcgrid.Count; j++)
            {
                if (all_floor_data[i].floor_height_level_ == FindFloorLevelInIFCGRID(ifc_code_of_ifcgrid[j]))
                {
                    corresponding_ifcgrid = ifc_code_of_ifcgrid[j];
                    break;
                }
            }
            //依照IFCGRID的ifc_code設定該樓層的grid_line_
            if (corresponding_ifcgrid == null) all_floor_data[i].grid_line_ = null;//沒找到該樓層的柱線
            else
            {
                SetOneFloorDataInIFCGRID(all_floor_data[i], corresponding_ifcgrid);
            }
        }

    }
    public float FindFloorLevelInIFCGRID(string ifc_code)
    {
        List<string> context1 = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[ifc_code])];//依照ifc_code取得IFCGRID資料
        List<string> context2 = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[context1[7].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]])];//依照ifc_code取得IFCLOCALPLACEMENT資料
        List<string> context3 = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[context2[3].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]])];//依照ifc_code取得IFCAXIS2PLACEMENT3D資料
        List<string> context4 = this.ifc_all_data_[Convert.ToInt32(this.ifc_hashtable_[context3[2].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]])];//依照ifc_code取得IFCCARTESIANPOINT資料
        return float.Parse(context4[4].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0]);
    }
    
}

