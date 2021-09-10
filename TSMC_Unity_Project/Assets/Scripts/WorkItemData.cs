using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkItemData : MonoBehaviour
{
    public List<string> id_name_ { get; set; }
    public int points_number_threshold_ { get; set; }
    public WorkItemData()
    {
        this.id_name_ = new List<string>();
        this.points_number_threshold_ = 623;
    }
    public WorkItemData(int points_number_threshold)
    {
        this.id_name_ = new List<string>();
        this.points_number_threshold_ = points_number_threshold;
    }
}
