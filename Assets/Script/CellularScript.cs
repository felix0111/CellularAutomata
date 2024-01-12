using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CellularScript : MonoBehaviour {

    public static CellularScript Instance;

    private GridGenerator _gridGenerator;

    public Vector2Int Size = new Vector2Int(100, 100);
    public float SimulationSpeed = 0.1f;

    public GridElementScript[,] Cells;

    //16 bits -> left == first combination; right == last combination
    private BitArray _ruleSet = new BitArray(16);

    //ui
    public TMP_Text RuleSetText;
    public TMP_Text FPSText;

    void Awake() {
        Instance = this;
        _gridGenerator = GetComponent<GridGenerator>();

        RandomRuleSet();
    }

    // Start is called before the first frame update
    void Start() {
        _gridGenerator.InitGrid(Size.x, Size.y);

        StartCoroutine(UpdateRoutine());
    }


    void Update() {
        CalculateFPS();

        if (Input.GetKey(KeyCode.A)) {
            Camera.main.transform.position += Vector3.left;
        } else if (Input.GetKey(KeyCode.D)) {
            Camera.main.transform.position += Vector3.right;
        }

        if (Input.GetKey(KeyCode.W)) {
            Camera.main.transform.position += Vector3.up;
        } else if (Input.GetKey(KeyCode.S)) {
            Camera.main.transform.position += Vector3.down;
        }

        if (Input.GetKey(KeyCode.KeypadPlus)) {
            Camera.main.orthographicSize -= 0.1f;
        } else if (Input.GetKey(KeyCode.KeypadMinus)) {
            Camera.main.orthographicSize += 0.1f;
        }

        if (Input.GetKey(KeyCode.R)) RandomRuleSet();

        if(Input.GetKey(KeyCode.Escape)) Application.Quit();
    }

    private float _fpsDelta = 0f;
    private float _frameCount = 0;
    private void CalculateFPS() {
        if (_frameCount < 10) {
            _fpsDelta += 1f / Time.deltaTime;
            _frameCount++;
        } else {
            FPSText.text = (_fpsDelta / _frameCount).ToString("F1") + " FPS";
            _fpsDelta = 0f;
            _frameCount = 0;
        }
    }

    //TODO add option to not change all cells at once but step after step
    IEnumerator UpdateRoutine() {
        while (true) {
            yield return new WaitForSeconds(SimulationSpeed);

            bool[,] cellCache = new bool[Size.x, Size.y];

            for (int y = 0; y < Size.y; y++) {
                for (int x = 0; x < Size.x; x++) {
                    cellCache[x, y] = GetNewState(x, y);
                }
            }

            for (int y = 0; y < Size.y; y++) {
                for (int x = 0; x < Size.x; x++) {
                    if (cellCache[x, y]) {
                        MakeLiving(x, y);
                    } else {
                        MakeDead(x, y);
                    }
                }
            }
        }
    }

    private bool GetNewState(int x, int y) {
        bool left = x - 1 >= 0 && Cells[x - 1, y].IsLiving;
        bool right = x + 1 < Size.x && Cells[x + 1, y].IsLiving;
        bool up = y + 1 < Size.y && Cells[x, y + 1].IsLiving;
        bool down = y - 1 >= 0 && Cells[x, y - 1].IsLiving;

        bool newState = false;

        for (int i = 0; i < 16; i++)
        {
            var bits = new BitArray(new int[] { i });

            //if matching combination found
            if (left == bits[0] && right == bits[1] && up == bits[2] && down == bits[3])
            {
                newState = _ruleSet[i];
            }
        }

        return newState;

        /*
        16 different combinations

        left right up down
        0000
        0001
        0010
        0011
        0100
        0101
        0110
        0111
        1000
        1001
        1010
        1011
        1100
        1101
        1110
        1111
        */
    }

    private void RandomRuleSet() {
        for (int i = 0; i < _ruleSet.Length; i++) {
            _ruleSet[i] = Random.value > 0.5f;
        }

        RuleSetText.text = "RuleSet: " + _ruleSet.ToBitString();
    }

    public void SetRuleSetButton(TMP_InputField input) {
        for (int i = 0; i < 16; i++) {
            if (i >= input.text.Length) {
                _ruleSet[i] = false;
                continue;
            }

            _ruleSet[i] = int.Parse(input.text[i].ToString()) != 0;
        }

        RuleSetText.text = "RuleSet: " + _ruleSet.ToBitString();
    }

    public void ChangeGridSizeButton(GameObject inputsParent) {
        int xValue = int.Parse(inputsParent.transform.Find("InputX").GetComponent<TMP_InputField>().text);
        int yValue = int.Parse(inputsParent.transform.Find("InputY").GetComponent<TMP_InputField>().text);
        if (xValue <= 0 || yValue <= 0) return;

        StopAllCoroutines();

        Size = new Vector2Int(xValue, yValue);

        Start();
    }

    public void SimulationSpeedChanged(Slider slider) {
        SimulationSpeed = slider.value;
    }

    public void MakeLiving(int x, int y) {
        Cells[x, y].GetComponent<SpriteRenderer>().color = Color.white;
        Cells[x, y].IsLiving = true;
    }

    public void MakeDead(int x, int y) {
        Cells[x, y].GetComponent<SpriteRenderer>().color = _gridGenerator.GridElementColor;
        Cells[x, y].IsLiving = false;
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