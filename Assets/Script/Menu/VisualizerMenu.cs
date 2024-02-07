using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class VisualizerMenu : MonoBehaviour {

    public void Toggle(bool forceClose = false) {
        gameObject.SetActive(!forceClose && !gameObject.activeSelf);
    }
}
