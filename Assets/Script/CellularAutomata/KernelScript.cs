using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KernelScript {

    private int _kernelRadius;
    public int KernelRadius {
        get => _kernelRadius;
        set {
            _kernelRadius = value;
            Kernel = new float[KernelLength * KernelLength];
            CustomTorus();
        }
    }
    public int KernelLength => KernelRadius * 2 + 1;
    public List<float> KernelLayers = new ();
    public int KernelAlpha = 1;
    public float[] Kernel;

    public float GrowthCenter;
    public float GrowthWidth;

    public KernelScript(int kernelRadius, float growthCenter, float growthWidth)
    {
        KernelRadius = kernelRadius;
        GrowthCenter = growthCenter;
        GrowthWidth = growthWidth;

        KernelLayers.Add(1f);
    }

    public void SetPixel(Vector2Int pixelPosition, float val) {
        //2d point to 1d array index
        int kernelIndex = pixelPosition.x + pixelPosition.y * KernelLength;

        //set point in kernel and update
        Kernel[kernelIndex] = Mathf.Clamp(val, 0f, 1f);

        CellularScript.Instance.UpdateBuffer();
    }

    public float GetPixel(Vector2Int pixelPosition) {
        //2d point to 1d array index
        int kernelIndex = pixelPosition.x + pixelPosition.y * KernelLength;

        return Kernel[kernelIndex];
    }

    public void SquareKernel() {
        for (int y = -KernelRadius; y <= KernelRadius; y++) {
            for (int x = -KernelRadius; x <= KernelRadius; x++) {
                int arrayIndex = x + KernelRadius + (y + KernelRadius) * KernelLength;

                if (x == 0 && y == 0) {
                    Kernel[arrayIndex] = 0f;
                    continue;
                }

                Kernel[arrayIndex] = 1;
            }
        }

        //normalize
        float sum = Kernel.Sum(o => o);
        Kernel = Kernel.Select(o => o / sum).ToArray();

        CellularScript.Instance.UpdateBuffer();
    }

    public void HardCircleKernel() {
        for (int y = -KernelRadius; y <= KernelRadius; y++) {
            for (int x = -KernelRadius; x <= KernelRadius; x++) {
                int arrayIndex = x + KernelRadius + (y + KernelRadius) * KernelLength;
                Kernel[arrayIndex] = Vector2.Distance(new Vector2(0, 0), new Vector2(x, y)) <= KernelRadius ? 1f : 0f;
            }
        }

        //normalize
        float sum = Kernel.Sum(o => o);
        Kernel = Kernel.Select(o => o / sum).ToArray();

        CellularScript.Instance.UpdateBuffer();
    }

    public void HardTorusKernel(float minRadius) {
        for (int y = -KernelRadius; y <= KernelRadius; y++) {
            for (int x = -KernelRadius; x <= KernelRadius; x++) {
                int arrayIndex = x + KernelRadius + (y + KernelRadius) * KernelLength;
                Kernel[arrayIndex] = Vector2.Distance(Vector2.zero, new Vector2(x, y)) <= KernelRadius + 0.1f && Vector2.Distance(Vector2.zero, new Vector2(x, y)) >= minRadius ? 1f : 0f;
            }
        }

        //normalize
        float sum = Kernel.Sum(o => o);
        Kernel = Kernel.Select(o => o / sum).ToArray();

        CellularScript.Instance.UpdateBuffer();
    }

    public void SmoothTorusKernel() {
        for (int y = -KernelRadius; y <= KernelRadius; y++) {
            for (int x = -KernelRadius; x <= KernelRadius; x++) {
                int arrayIndex = x + KernelRadius + (y + KernelRadius) * KernelLength;
                float dist = Vector2.Distance(Vector2.zero, new Vector2(x, y));
                Kernel[arrayIndex] = Gauss(dist / KernelRadius, 0.5f, 0.15f);
            }
        }

        //normalize
        float sum = Kernel.Sum(o => o);
        Kernel = Kernel.Select(o => o / sum).ToArray();

        CellularScript.Instance.UpdateBuffer();
    }

    public void SmoothCircleKernel() {
        for (int y = -KernelRadius; y <= KernelRadius; y++) {
            for (int x = -KernelRadius; x <= KernelRadius; x++) {
                int arrayIndex = x + KernelRadius + (y + KernelRadius) * KernelLength;
                Kernel[arrayIndex] = Vector2.Distance(new Vector2(0, 0), new Vector2(x, y));
            }
        }

        //normalize and invert
        float sum = Kernel.Sum(o => o);
        float max = Kernel.Max(o => o);
        Kernel = Kernel.Select(o => (max - o) / sum).ToArray();

        CellularScript.Instance.UpdateBuffer();
    }

    public void CustomTorus() {
        for (int y = -KernelRadius; y <= KernelRadius; y++) {
            for (int x = -KernelRadius; x <= KernelRadius; x++) {
                int arrayIndex = x + KernelRadius + (y + KernelRadius) * KernelLength;
                float dist = Vector2.Distance(Vector2.zero, new Vector2(x, y)) / KernelRadius;

                int layers = KernelLayers.Count;
                float final = 0f;
                for (int i = 0; i < layers; i++) {
                    if (dist <= (i + 1) / (float)layers) {
                        final = KernelLayers[i] * CustomCoreFunction(layers * dist - i, KernelAlpha);
                        break;
                    }
                }
                    
                Kernel[arrayIndex] = Mathf.Clamp(final, 0f, 1f);
            }
        }

        //normalize
        float sum = Kernel.Sum(o => o);
        Kernel = Kernel.Select(o => o / sum).ToArray();

        CellularScript.Instance.UpdateBuffer();
    }

    

    public void RandomizeKernel() {
        for (int i = 0; i < Kernel.Length; i++) {
            Kernel[i] = Random.value;
        }

        //normalize
        float sum = Kernel.Sum(o => o);
        Kernel = Kernel.Select(o => o / sum).ToArray();

        CellularScript.Instance.UpdateBuffer();
    }

    public void ClearKernel() {
        for (int i = 0; i < Kernel.Length; i++) {
            Kernel[i] = Random.value;
        }

        CellularScript.Instance.UpdateBuffer();
    }

    public float Gauss(float x, float middle, float width) {
        return Mathf.Exp(-Mathf.Pow((x - middle) / width, 2) / 2);
    }

    public static float CustomCoreFunction(float input, float alpha) {
        return Mathf.Pow(4f * input * (1f - input), alpha);
    }

    public float[] GetKernelDistribution() {
        float[] dist = new float[KernelLength];

        for (int y = -KernelRadius; y <= KernelRadius; y++) {
            for (int x = -KernelRadius; x <= KernelRadius; x++) {
                dist[x + KernelRadius] += Kernel[x + KernelRadius + (y + KernelRadius) * KernelLength];
            }
        }

        return dist;
    }
}