using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    public static MenuManager Instance;

    public NeighbourVisualizer NeighbourVisualizer;
    public KernelVisualizer KernelVisualizer;
    public SettingsMenu SettingsMenu;

    void Awake() {
        Instance = this;
    }

    void Start() {
        CloseAllMenus();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) CloseAllMenus();
    }

    public void CloseAllMenus() {
        NeighbourVisualizer.Toggle(true);
        KernelVisualizer.Toggle(true);
        SettingsMenu.Toggle(true);
    }
}
