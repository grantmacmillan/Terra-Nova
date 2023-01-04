using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerLook : MonoBehaviour
{
    public Camera camera;
    public Transform orientation;

    private float xRotation = 0f, yRotation;

    public float xSensitivity = 30f;

    public float ySensitivity = 30f;

    void Start()
    {
        camera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Look(Vector2 input)
    {
        float mouseX = input.x * Time.fixedDeltaTime * xSensitivity;
        float mouseY = input.y * Time.fixedDeltaTime * ySensitivity;

        /*xRotation -= (mouseY * Time.fixedDeltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.fixedDeltaTime) * xSensitivity);*/

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
