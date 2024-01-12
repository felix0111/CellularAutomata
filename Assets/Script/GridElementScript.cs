using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElementScript : MonoBehaviour {
    
    public Vector2Int GridElementPosition;
    public bool IsLiving;


    void OnMouseDown() {
        CellularScript.Instance.MakeLiving(GridElementPosition.x, GridElementPosition.y);
    }
}
