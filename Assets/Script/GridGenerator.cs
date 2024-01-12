using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;

public class GridGenerator : MonoBehaviour {

    public GameObject GridElementPrefab;
    public float GridSize = 0.4f;
    public Color GridElementColor = Color.black;

    //TODO ad option to fill grid randomly
    public void InitGrid(int ElementsX, int ElementsY) {
        
        //Cleanup
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        //make grid
        CellularScript.Instance.Cells = new GridElementScript[ElementsX + 1, ElementsY + 1];
        for (int x = 0; x < ElementsX; x++) {
            for (int y = 0; y < ElementsY; y++) {
                CellularScript.Instance.Cells[x, y] = MakeGridElement(x, y, GridElementColor);
            }
        }
    }

    private GridElementScript MakeGridElement(int x, int y, Color color) {
        GameObject go = Instantiate(GridElementPrefab, new Vector3(x * GridSize, y * GridSize, 0f), Quaternion.identity, transform);
        go.GetComponent<SpriteRenderer>().color = color;
        go.GetComponent<SpriteRenderer>().sortingOrder = 1;
        go.GetComponent<GridElementScript>().GridElementPosition = new Vector2Int(x, y);
        go.GetComponent<GridElementScript>().IsLiving = false;
        go.transform.localScale *= GridSize;
        return go.GetComponent<GridElementScript>();
    }
}
