using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildTest : MonoBehaviour
{
    GameObject father;
    Transform child;
    // Start is called before the first frame update
    void Start()
    {
        father = GameObject.Find("Cube");
        Debug.Log("father name:"+father.name);
        Debug.Log("childCount:" + father.transform.childCount);
        this.child = this.father.transform.GetChild(0);
        Debug.Log("GetChild(0):" + this.child.gameObject.name);
        string name = this.child.gameObject.name;
        string back_name = name.Substring(name.LastIndexOf('(')+1,1);
        Debug.Log("back_name:" + back_name);
        Debug.Log("After name.Substring, name.length:"+name.Length);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
