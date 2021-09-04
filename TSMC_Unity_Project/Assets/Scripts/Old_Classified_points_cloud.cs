using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
/*此Script的功能為：
 *1.讀取點雲txt檔
 * 2.依據碰撞分類點雲
 * 3.顯示分類後的點雲
 * 4.分類的點雲分批寫入txt檔
 * 資料儲存在Data中
 */

/* 使用說明：
 * (1)將模型解除convert unit後，放入Hierarchy中，請先將待偵測模型完全解除Prefab，並對每個子物件附上Mesh Collider
 * (2)創建一個物件，命名為分類器，並附上此Script
 * (4)在Asset中創建欲存放txt檔的資料夾
 * (5)在分類器 Inspector 視窗中，填入點雲檔之檔名(不需要加上.txt)、存放輸出txt檔之資料夾名稱
 */

public class Old_Classified_points_cloud : MonoBehaviour
{
    public string PointCloud_txtFile_Name = " ";//要開啟的未分類點雲檔案名稱
    public string Write_Document_Name = " ";    //要存放分類後txt檔的資料夾名稱
    public string Write_File_Name = " ";    //要存放分類後虛擬點雲txt檔的名稱
    public float radius = 0.01f;                //分類半徑，不可設為0
    List<MyDATA> Data = new List<MyDATA>();     //點雲儲存處
    void Start()
    {
        /*********************************讀取txt，分類點位後載入Data***********************************************/
        List<Vector3> Data_Vector3_02 = new List<Vector3>();
        Read(this.PointCloud_txtFile_Name, Data_Vector3_02);    //讀取txt
        for (int i = 0; i < Data_Vector3_02.Count; i++) Classsified_point(Data_Vector3_02[i], Data);//依據碰撞分類點位後載入Data.points
        Debug.Log("第一階段結束");
        // 我把顯示功能關掉了，如果有需要顯示就啟動下面這行
        //for (int i = 0; i < Data.Count; i++) Creat_Mesh(Data[i]);    //顯示點雲
        /*********************************依照分類匯出多個txt***********************************************/
        //for (int i = 0; i < Data.Count; i++) Write_txt(Data[i].Object_name, Data[i].points, this.Write_Document_Name);
        WriteAllPointDataInOneTxtFile(this.Write_Document_Name, this.Write_File_Name, this.Data);
        Debug.Log("第二階段結束");
    }
    void Read(string name, List<Vector3> myList)
    {
        string path = Application.dataPath + "/" + name + ".txt";
        FileInfo f = new FileInfo(path);
        StreamReader r;
        string s = "";
        if (f.Exists) r = new StreamReader(path);
        else
        {
            Debug.Log("點雲txt文件不在");
            return;
        }
        while ((s = r.ReadLine()) != null)
        {
            string[] words = s.Split(' ');
            Vector3 xyz = new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2]));
            myList.Add(xyz);
        }
        return;
    }
    void Classsified_point(Vector3 point, List<MyDATA> Data)
    {
        Collider[] hitColliders = Physics.OverlapSphere(point, this.radius);
        if (hitColliders.Length == 0) return;//沒撞到
        string finalObj = " ";//最接近的物件名稱
        List<float> distance = new List<float>();
        if (hitColliders.Length == 1) finalObj = hitColliders[0].name;//僅撞到一個
        else//撞到不只一個
        {
            for (int i = 0; i < hitColliders.Length; i++) distance.Add((point - hitColliders[i].ClosestPointOnBounds(point)).magnitude);//算最近點距離到矩陣
            finalObj = FindMin(distance, hitColliders);
        }
        if (Data.Count == 0)//Data什麼都沒接，接上一個class
        {
            MyDATA data = new MyDATA();
            data.points = new List<Vector3>();
            data.Object_name = finalObj;
            data.points.Add(point);
            Data.Add(data);
            return;
        }
        for (int i = 0; i < Data.Count; i++)
        {
            if (Data[i].Object_name == finalObj)
            {
                Data[i].points.Add(point);
                return;
            }
            if (i == Data.Count - 1)//找不到分類，接上一個class
            {
                MyDATA data = new MyDATA();
                data.points = new List<Vector3>();
                data.Object_name = finalObj;
                data.points.Add(point);
                Data.Add(data);
                return;
            }
        }
    }
    string FindMin(List<float> distance, Collider[] hitColliders)
    {
        float Min = distance[0]; int MinNum = 0;
        float Min2 = 9999;
        for (int i = 1; i < distance.Count; i++)//find Min
        {
            if (distance[i] < Min) { Min = distance[i]; MinNum = i; }
        }
        for (int i = 0; i < distance.Count; i++)//find 2nd Min
        {
            if (i == MinNum) ;
            else if (distance[i] < Min2) Min2 = distance[i];
        }
        if (Min == Min2) return " ";//最小兩個一樣，回傳空
        else return hitColliders[MinNum].name;//有最小，回傳物件名字
    }
    void Creat_Mesh(MyDATA data)
    {
        int indeceisLimit = 65535;
        int start = 0;
        int num = data.points.Count;
        while (start < num)
        {
            if (start + indeceisLimit - 1 > num) Creat_Mesh_within_65535(data.points, data.Object_name, start, num);
            else Creat_Mesh_within_65535(data.points, data.Object_name, start, start + indeceisLimit - 1);
            start += indeceisLimit;
        }
    }
    void Creat_Mesh_within_65535(List<Vector3> data, string name, int start, int end)
    {
        GameObject pointObj = new GameObject(name);
        pointObj.AddComponent<MeshRenderer>();
        pointObj.AddComponent<MeshFilter>();
        Mesh meshNeed = new Mesh();
        Material mat = new Material(Shader.Find("Standard"));
        pointObj.GetComponent<MeshFilter>().mesh = meshNeed;
        pointObj.GetComponent<MeshRenderer>().material = mat;
        Vector3[] points = new Vector3[end - start + 1];
        Color[] colors = new Color[end - start + 1];
        int[] indecies = new int[end - start + 1];
        for (int i = 0; i < end - start; i++)
        {
            points[i] = data[start + i];
            colors[i] = Color.yellow;
            indecies[i] = i;
        }
        meshNeed.vertices = points;
        meshNeed.colors = colors;
        meshNeed.SetIndices(indecies, MeshTopology.Points, 0);
    }
    void Write_txt(string name, List<Vector3> Data_Vector3, string myPath)
    {
        string[] word = name.Split('[', ']');  //******** ID的名稱格式：[ID]
        string fileName;
        string filePath = Application.dataPath + "/" + myPath + "/";
        if (word.Length >= 2)
        {
            fileName = word[1] + ".txt";
        }
        else return;
        FileStream fs = new FileStream(filePath + fileName, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < Data_Vector3.Count; i++) sw.Write(Data_Vector3[i].x + " " + Data_Vector3[i].y + " " + Data_Vector3[i].z + "\n");
        sw.Close();
        fs.Close();
    }
    public void WriteAllPointDataInOneTxtFile(string folder_name, string file_name, List<MyDATA> data)
    {
        string file_path = Application.dataPath + "/" + folder_name + "/" + file_name + ".txt";
        //FileStream fs = new FileStream(file_path, FileMode.Create);
        StreamWriter sw = new StreamWriter(file_path);
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].Object_name.Length >= 2)
            {
                string[] word = data[i].Object_name.Split('[', ']');
                for (int j = 0; j < data[i].points.Count; j++)
                {
                    sw.Write(data[i].points[j].x + " " + data[i].points[j].y + " " + data[i].points[j].z + " " + word[1] + "\n");
                }
                sw.Flush();

            }
        }
        sw.Close();
        //fs.Close();
    }
    public class MyDATA
    {
        public string Object_name;
        public List<Vector3> points;
    }
}
