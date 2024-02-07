using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KernelVisualizer : MonoBehaviour {

    public KernelScript Kernel;

    public ComputeShader Shader => CellularScript.Instance.Shader;

    public RawImage TargetImage;
    private RenderTexture _renderTexture;

    //ui stuff
    public TMP_Dropdown KernelSelector, KernelSizeSelector;
    public RectTransform SettingsContent;
    public Slider AlphaSlider;
    public GameObject KernelLayerSettingsPrefab;
    private List<GameObject> _activePrefabs = new();

    public int KernelRadius => int.Parse(KernelSizeSelector.options[KernelSizeSelector.value].text.Split("px")[0]);

    // Start is called before the first frame update
    void Start()
    {
        //find kernel
        Kernel = CellularScript.Instance.Kernels[0];

        //init render texture
        CreateTexture();

        //init ui stuff
        KernelSelector.onValueChanged.AddListener(o => {
            Kernel = CellularScript.Instance.Kernels[o];
            RefreshSettings();
        });
        KernelSizeSelector.onValueChanged.AddListener(o => {
            foreach (var kernel in CellularScript.Instance.Kernels) {
                kernel.KernelRadius = KernelRadius;
            }
            CreateTexture();
            DispatchImage();
        });
        AlphaSlider.onValueChanged.AddListener(o => {
            Kernel.KernelAlpha = (int)o;
            Kernel.CustomTorus();
            DispatchImage();
        });
        RefreshSettings();
    }

    void Update() {
        if(Input.GetMouseButton(0) || Input.GetMouseButton(1)) DrawOnKernel();
    }

    public void Toggle(bool forceClose = false) {
        gameObject.SetActive(!forceClose && !gameObject.activeSelf);
    }

    private void CreateTexture() {
        _renderTexture = new RenderTexture(Kernel.KernelLength, Kernel.KernelLength, 24);
        _renderTexture.enableRandomWrite = true;
        _renderTexture.filterMode = FilterMode.Point;
        _renderTexture.Create();
        TargetImage.texture = _renderTexture;
    }

    public void RefreshSettings() {
        int currentSelected = KernelSelector.value;
        KernelSelector.ClearOptions();
        List<string> names = new List<string>(CellularScript.Instance.Kernels.Count);
        for (int i = 0; i < names.Capacity; i++) {
            names.Add("Kernel " + (i + 1));
        }
        KernelSelector.AddOptions(names);
        KernelSelector.SetValueWithoutNotify(currentSelected);

        AlphaSlider.value = Kernel.KernelAlpha;

        for (int i = 0; i < _activePrefabs.Count; i++) {
            Destroy(_activePrefabs[i]);
        }

        for (int i = 0; i < Kernel.KernelLayers.Count; i++) {
            int cpy = i;
            GameObject prefab = Instantiate(KernelLayerSettingsPrefab, SettingsContent);
            prefab.GetComponentInChildren<Slider>().onValueChanged.AddListener(o => {
                Kernel.KernelLayers[cpy] = o;
                Kernel.CustomTorus();
                DispatchImage();
            });
            prefab.GetComponentInChildren<SliderInfo>().Prefix = "Beta " + (i+1) + ": ";
            prefab.GetComponentInChildren<Slider>().value = Kernel.KernelLayers[i];
            _activePrefabs.Add(prefab);
        }

        DispatchImage();
    }

    public void AddKernel() {
        KernelScript ks = new KernelScript(KernelRadius, 0.135f, 0.0115f);
        ks.KernelAlpha = 4;
        ks.CustomTorus();

        CellularScript.Instance.Kernels.Add(ks);
        CellularScript.Instance.UpdateBuffer();

        RefreshSettings();
        KernelSelector.value++;
    }

    public void RemoveKernel() {
        if (CellularScript.Instance.Kernels.Count == 1) return;
        CellularScript.Instance.Kernels.RemoveAt(KernelSelector.value);
        CellularScript.Instance.UpdateBuffer();

        RefreshSettings();
        KernelSelector.value--;
    }

    public void AddKernelLayer() {
        Kernel.KernelLayers.Add(0.5f);
        Kernel.CustomTorus();
        RefreshSettings();
    }

    public void RemoveKernelLayer() {
        Kernel.KernelLayers.RemoveAt(Kernel.KernelLayers.Count-1);
        Kernel.CustomTorus();
        RefreshSettings();
    }

    public void DispatchImage() {
        int debug = Shader.FindKernel("DebugKernel");

        Shader.SetInt("KernelToDebug", KernelSelector.value);
        Shader.SetInt("KernelSize", Kernel.Kernel.Length);
        Shader.SetBuffer(debug, "Kernels", CellularScript.Instance.KernelBuffer);
        Shader.SetTexture(debug, "NewTexture", _renderTexture);
        Shader.Dispatch(debug, Kernel.KernelLength, Kernel.KernelLength, 1);
    }

    private Vector2Int MousePositionToPixelPosition() {
        Vector2 normPos = Input.mousePosition - TargetImage.transform.position;
        normPos = new Vector2(normPos.x / TargetImage.rectTransform.rect.width + TargetImage.rectTransform.pivot.x, normPos.y / TargetImage.rectTransform.rect.height + TargetImage.rectTransform.pivot.y);
        return new Vector2Int(Mathf.FloorToInt(normPos.x * Kernel.KernelLength), Mathf.FloorToInt(normPos.y * Kernel.KernelLength));
    }

    public void DrawOnKernel() {
        var pixelPos = MousePositionToPixelPosition();
        if (pixelPos.x < 0 || pixelPos.y < 0 || pixelPos.x >= Kernel.KernelLength || pixelPos.y >= Kernel.KernelLength) return;

        if (Input.GetMouseButton(0)) {
            //add value to pixel
            Kernel.SetPixel(pixelPos, Kernel.GetPixel(pixelPos) + 0.02f * Time.deltaTime);
        } else if (Input.GetMouseButton(1)) {
            //remove value to pixel
            Kernel.SetPixel(pixelPos, Kernel.GetPixel(pixelPos) - 0.02f * Time.deltaTime);
        }

        //update kernel visualizer
        DispatchImage();
    }

    public void OnRandomizeButton() {
        Kernel.RandomizeKernel();
        DispatchImage();
    }

    public void OnClearButton() {
        Kernel.ClearKernel();
        DispatchImage();
    }
}
