using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.LeftAlt) ||
            Input.GetMouseButton(0))
            return;

        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}