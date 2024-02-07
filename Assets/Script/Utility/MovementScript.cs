using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    private Camera _camera;
    private float CameraSpeed => 2f * (_camera.orthographicSize / 100f);
    private float CameraScrollSpeed => 5f * (_camera.orthographicSize / 100f);

    void Start() {
        _camera = Camera.main;
    }

    void Update() {
        if (Input.GetKey(KeyCode.W)) {
            _camera.transform.position += Vector3.up * CameraSpeed;
        } else if (Input.GetKey(KeyCode.S)) {
            _camera.transform.position += Vector3.down * CameraSpeed;
        }

        if (Input.GetKey(KeyCode.A)) {
            _camera.transform.position += Vector3.left * CameraSpeed;
        } else if (Input.GetKey(KeyCode.D)) {
            _camera.transform.position += Vector3.right * CameraSpeed;
        }

        if (Input.mouseScrollDelta.y > 0f) {
            _camera.orthographicSize -= CameraScrollSpeed;
        } else if (Input.mouseScrollDelta.y < 0f) {
            _camera.orthographicSize += CameraScrollSpeed;
        }
    }
}
