﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    Quaternion destinationRotation;
    Vector2 rotationInput;
    public float maxRotationDistance;
    public float sensitivity;
    public float lerpSpeed;
    public float returnSpeed;
    // Use this for initialization
    void Start()
    {
        rotationInput = new Vector2();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rotationVelocity = new Vector2();

        if (Input.GetMouseButton(1))
        {
            rotationVelocity.x = Input.GetAxis("Mouse X") * sensitivity;
            rotationVelocity.y = Input.GetAxis("Mouse Y") * -sensitivity;
        }

        if (rotationInput.magnitude > maxRotationDistance)
        {
            rotationVelocity -= (rotationInput.normalized * (rotationInput.magnitude - maxRotationDistance)) * returnSpeed;
        }

        rotationInput += rotationVelocity;

        destinationRotation = Quaternion.AngleAxis(rotationInput.x, Vector3.up) * Quaternion.AngleAxis(rotationInput.y, Vector3.right);

        transform.rotation = Quaternion.Slerp(transform.rotation, destinationRotation, lerpSpeed);

    }
}
