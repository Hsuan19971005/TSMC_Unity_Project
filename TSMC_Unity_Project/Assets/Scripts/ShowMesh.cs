using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ShowMesh : MonoBehaviour
{
    public string PointCloud_txtFile_Name = " ";//要開啟的點雲檔案名稱
    void Start()
    {
        List<Vector3> Data_Vector3_02 = new List<Vector3>();
        Read(this.PointCloud_txtFile_Name, Data_Vector3_02);//讀取txt
        Creat_Mesh(Data_Vector3_02);    //顯示點雲
    }
    void Read(string name, List<Vector3> myList)
    {
        string path = "D:\\UnityAllFile/TSMC_Unity_Project/" + name + ".txt";
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
    void Creat_Mesh(List<Vector3> data)
    {
        int indeceisLimit = 65535;
        int start = 0;
        int num = data.Count;
        while (start < num)
        {
            if (start + indeceisLimit - 1 > num) Creat_Mesh_within_65535(data, start, num);
            else Creat_Mesh_within_65535(data, start, start + indeceisLimit - 1);
            start += indeceisLimit;
        }
    }
    void Creat_Mesh_within_65535(List<Vector3> data, int start, int end)
    {
        GameObject pointObj = new GameObject();
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
}
