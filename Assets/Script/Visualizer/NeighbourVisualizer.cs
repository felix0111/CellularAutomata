using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class NeighbourVisualizer : MonoBehaviour
{
    public ComputeShader CellularShader => CellularScript.Instance.Shader;

    //shader stuff
    public RawImage TargetImage;
    private RenderTexture _renderTexture;

    void Start() {
        //init render texture
        _renderTexture = new RenderTexture(CellularScript.Size.x, CellularScript.Size.y, 24);
        _renderTexture.enableRandomWrite = true;
        _renderTexture.filterMode = FilterMode.Point;
        _renderTexture.Create();

        TargetImage.texture = _renderTexture;
    }

    void Update() {
        DispatchDebug();
    }

    public void Toggle(bool forceClose = false) {
        gameObject.SetActive(!forceClose && !gameObject.activeSelf);
    }

    private void DispatchDebug() {

        int debug = CellularShader.FindKernel("DebugGrowth");

        CellularShader.SetInt("KernelAmount", CellularScript.Instance.Kernels.Count);
        CellularShader.SetInt("KernelSize", CellularScript.Instance.Kernels[0].Kernel.Length);
        CellularShader.SetBuffer(debug, "Kernels", CellularScript.Instance.KernelBuffer);
        CellularShader.SetBuffer(debug, "GrowthParameters", CellularScript.Instance.GrowthParameterBuffer);
        CellularShader.SetTexture(debug, "BufferTexture", CellularScript.Instance.RenderTextures[0]);
        CellularShader.SetTexture(debug, "NewTexture", _renderTexture);
        CellularShader.Dispatch(debug, CellularScript.Size.x / 8, CellularScript.Size.y / 8, 1);
    }
}
