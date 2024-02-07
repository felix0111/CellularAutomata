using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderInfo : MonoBehaviour {
    public TMP_Text Info;
    public Slider Slider;

    public string Prefix;

    void Awake() {
        Slider.onValueChanged.AddListener(OnSliderUpdate);
    }

    public void OnSliderUpdate(float val) {
        Info.text = Prefix + val.ToString("#0.000");
    }

}
