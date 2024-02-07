using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class CellularScript : MonoBehaviour {

    public static CellularScript Instance;
    public MenuManager MenuManager;

    //cellular vars
    public static Vector2Int Size = new Vector2Int(256, 256);
    public List<KernelScript> Kernels = new();

    public float SimulationSpeed {
        get => Time.timeScale;
        set => Time.timeScale = value;
    }

    //shader stuff
    public ComputeShader Shader;
    public ComputeBuffer KernelBuffer, GrowthParameterBuffer;
    private int _kernelID;
    private int _currentMainTexture = 0;
    private int _currentBufferTexture = 1;
    public RenderTexture[] RenderTextures;
    public Renderer TargetRenderer;

    void Awake() {
        Instance = this;

        //init render texture
        CreateTextures();

        //init default kernel
        KernelScript ks = new KernelScript(MenuManager.KernelVisualizer.KernelRadius, 0.135f, 0.0115f);
        ks.KernelAlpha = 4;
        ks.CustomTorus();
        Kernels.Add(ks);

        //init shader
        _kernelID = Shader.FindKernel("Cellular");
        UpdateBuffer();
    }

    void Start() {
        RandomizeCells();
    }

    void FixedUpdate() {
        //copy actual texture to buffer texture
        Graphics.Blit(RenderTextures[_currentMainTexture], RenderTextures[_currentBufferTexture]);

        Shader.SetInt("KernelAmount", Kernels.Count);
        Shader.SetInt("KernelSize", Kernels[0].Kernel.Length);
        Shader.SetBuffer(_kernelID, "GrowthParameters", GrowthParameterBuffer);
        Shader.SetBuffer(_kernelID, "Kernels", KernelBuffer);
        Shader.SetTexture(_kernelID, "NewTexture", RenderTextures[_currentMainTexture]);
        Shader.SetTexture(_kernelID, "BufferTexture", RenderTextures[_currentBufferTexture]);

        //dispatch shader
        Shader.Dispatch(_kernelID, Size.x / 8, Size.y / 8, 1);
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.R)) RandomizeCells();

        if (Input.GetKeyDown(KeyCode.C)) ClearCells();

        if (Input.GetKeyDown(KeyCode.Q)) Application.Quit();

        if(Input.GetMouseButton(0)) DrawOnCellular(3);
    }

    public void CreateTextures() {
        RenderTextures = new RenderTexture[2];
        RenderTextures[0] = new RenderTexture(Size.x, Size.y, 24);
        RenderTextures[0].enableRandomWrite = true;
        RenderTextures[0].filterMode = FilterMode.Point;
        RenderTextures[0].Create();
        RenderTextures[1] = new RenderTexture(Size.x, Size.y, 24);
        RenderTextures[1].enableRandomWrite = true;
        RenderTextures[1].filterMode = FilterMode.Point;
        RenderTextures[1].Create();
        TargetRenderer.material.SetTexture("_MainTex", RenderTextures[_currentMainTexture]);
    }

    public void UpdateBuffer() {
        if (Kernels.Count == 0) return;

        //set vars
        if (KernelBuffer != null) KernelBuffer.Release();
        KernelBuffer = new ComputeBuffer(Kernels.Select(o => o.Kernel.Length).Sum(), sizeof(float));
        float[] kernels = new float[Kernels.Select(o => o.Kernel.Length).Sum()];
        int currentIndex = 0;
        for (int i = 0; i < Kernels.Count; i++) {
            for (int j = 0; j < Kernels[i].Kernel.Length; j++) {
                kernels[currentIndex] = Kernels[i].Kernel[j];
                currentIndex++;
            }
        }
        KernelBuffer.SetData(kernels);

        if (GrowthParameterBuffer != null) GrowthParameterBuffer.Release();
        GrowthParameterBuffer = new ComputeBuffer(Kernels.Count, sizeof(float) * 2);
        float2[] param = new float2[Kernels.Count];
        for (int i = 0; i < Kernels.Count; i++) {
            param[i] = new float2(Kernels[i].GrowthCenter, Kernels[i].GrowthWidth);
        }
        GrowthParameterBuffer.SetData(param);
    }

    public void DrawOnCellular(int radius) {
        Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, out RaycastHit hit2, 10000.0f);
        if (hit2.collider == null || hit2.collider.gameObject != gameObject) return;
        
        Vector2Int pixelPosition = new(Mathf.FloorToInt(hit2.textureCoord.x * Size.x), Mathf.FloorToInt(hit2.textureCoord.y * Size.y));

        for (int y = -radius; y <= radius; y++) {
            for (int x = -radius; x <= radius; x++) {
                //set pixel
                int setKernel = Shader.FindKernel("Set");
                Shader.SetInts("SetPixel", pixelPosition.x + x, pixelPosition.y + y);
                Shader.SetTexture(setKernel, "NewTexture", RenderTextures[_currentMainTexture]);
                Shader.Dispatch(setKernel, Size.x / 8, Size.y / 8, 1);
            }
        }
    }

    public void RandomizeCells() {
        int initKernel = Shader.FindKernel("Randomize");
        Shader.SetTexture(initKernel, "NewTexture", RenderTextures[_currentMainTexture]);
        Shader.SetFloat("random", Random.value);
        Shader.Dispatch(initKernel, Size.x / 8, Size.y / 8, 1);
    }

    public void ClearCells() {
        int initKernel = Shader.FindKernel("Randomize");
        Shader.SetTexture(initKernel, "NewTexture", RenderTextures[_currentMainTexture]);
        Shader.SetFloat("random", 0f);
        Shader.Dispatch(initKernel, Size.x / 8, Size.y / 8, 1);
    }

    public void SimulationSpeedChanged(Slider slider) {
        SimulationSpeed = slider.value;
    }

    void OnDestroy() {
        KernelBuffer.Release();
        GrowthParameterBuffer.Release();
    }
}

public static class Utility {

    public static string ToBitString(this BitArray bits) {
        var sb = new StringBuilder();

        for (int i = 0; i < bits.Count; i++) {
            char c = bits[i] ? '1' : '0';
            sb.Append(c);
        }

        return sb.ToString();
    }

}