using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorData : MonoBehaviour
{
    public string floor_name_ { get; set; }
    public string floor_ifc_code_ { get; set; }
    public float floor_height_level_ { get; set; }
    public WorkItemData WorkItemDataIfcColumn_ { get; set; }
    public WorkItemData WorkItemDataIfcWallStandardCase_ { get; set; }
    public WorkItemData WorkItemDataIfcSlab_ { get; set; }
    public WorkItemData WorkItemDataIfcBuildingElementProxy_ { get; set; }
    public List<Line> grid_line_ { get; set; }
    public FloorData()
    {
        this.WorkItemDataIfcColumn_ = new WorkItemData();
        this.WorkItemDataIfcWallStandardCase_ = new WorkItemData();
        this.WorkItemDataIfcSlab_ = new WorkItemData();
        this.WorkItemDataIfcBuildingElementProxy_ = new WorkItemData();
        //this.grid_line_ = new List<float[][]>();
        this.grid_line_ = new List<Line>();
    }
}
