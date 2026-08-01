using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCameraTurn : MonoBehaviour
{
    [Header("設定")]
    public float sensitivity = 0.2f;

    [Header("回転を有効にする")]
    public bool rotateHorizontal = true; // 左右
    public bool rotateVertical = true;   // 上下

    [Header("角度制限")]
    public bool limitHorizontal = false;
    public float minHorizontal = -180f;
    public float maxHorizontal = 180f;

    public bool limitVertical = true;
    public float minVertical = -90f;
    public float maxVertical = 90f;

    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        if (rotateHorizontal)
            yRotation += mouseDelta.x * sensitivity;

        if (rotateVertical)
            xRotation -= mouseDelta.y * sensitivity;

        if (limitHorizontal)
            yRotation = Mathf.Clamp(yRotation, minHorizontal, maxHorizontal);

        if (limitVertical)
            xRotation = Mathf.Clamp(xRotation, minVertical, maxVertical);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}