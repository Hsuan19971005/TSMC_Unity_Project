using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkItemData : MonoBehaviour
{
    public string work_item_name_ { get; set; }
    public List<string> id_name_ { get; set; }
    public bool standard_number_ { get; set; }
    public bool standard_height_ { get; set; }
    public bool standard_two_sides_ { get; set; }
    public float radius_ { get; set; }
    public WorkItemData()
    {
        this.id_name_ = new List<string>();
        this.radius_ = 0.5f;
        standard_height_ = false;
        standard_number_ = false;
        standard_two_sides_ = false;
    }
}
