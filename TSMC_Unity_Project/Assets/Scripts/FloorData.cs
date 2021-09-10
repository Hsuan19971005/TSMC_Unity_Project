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
    public WorkItemData WorkItemDataGrid_ { get; set; }
    public List<Line> grid_line_ { get; set; }
    public FloorData()
    {
        this.WorkItemDataIfcColumn_ = new WorkItemData(300);
        this.WorkItemDataIfcWallStandardCase_ = new WorkItemData(623);
        this.WorkItemDataIfcSlab_ = new WorkItemData();
        this.WorkItemDataIfcBuildingElementProxy_ = new WorkItemData();
        this.WorkItemDataGrid_ = new WorkItemData(623);
        this.grid_line_ = new List<Line>();
    }
}
