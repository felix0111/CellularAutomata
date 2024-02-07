using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSScript : MonoBehaviour {

    private TMP_Text _text;

    // Start is called before the first frame update
    void Start() {
        _text = GetComponent<TMP_Text>();
    }

    void Update() {
        CalculateFPS();
    }

    private float _fpsDelta = 0f;
    private float _frameCount = 0;

    private void CalculateFPS() {
        if (_frameCount < 10) {
            _fpsDelta += 1f / Time.deltaTime * Time.timeScale;
            _frameCount++;
        } else {
            _text.text = (_fpsDelta / _frameCount).ToString("F1") + " FPS";
            _fpsDelta = 0f;
            _frameCount = 0;
        }
    }
}
