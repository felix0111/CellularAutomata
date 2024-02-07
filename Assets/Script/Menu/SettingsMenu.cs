using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    public Slider GrowthMiddleSlider, GrowthWidthSlider;

    void Start() {
        /*
        GrowthMiddleSlider.onValueChanged.AddListener(o => CellularScript.Instance.GrowthMiddle = o);
        GrowthWidthSlider.onValueChanged.AddListener(o => CellularScript.Instance.GrowthWidth = o);
        GrowthMiddleSlider.value = CellularScript.Instance.GrowthMiddle;
        GrowthWidthSlider.value = CellularScript.Instance.GrowthWidth;
        */
    }

    public void OnSizeChange(TMP_Dropdown dropdown) {
        var size = dropdown.options[dropdown.value].text.Split('x');
        CellularScript.Size = new Vector2Int(int.Parse(size[0]), int.Parse(size[1]));
        CellularScript.Instance.CreateTextures();
        CellularScript.Instance.RandomizeCells();
    }

    public void Toggle(bool forceClose = false) {
        gameObject.SetActive(!forceClose && !gameObject.activeSelf);
    }

}
