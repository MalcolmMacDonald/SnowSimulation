using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    Quaternion destinationRotation;
    float horizontalRotation;
    float verticalRotation;
    public float sensitivity;
    public float lerpSpeed;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetMouseButton(1))
        {
            horizontalRotation += Input.GetAxis("Mouse X") * sensitivity;
            verticalRotation += Input.GetAxis("Mouse Y") * -sensitivity;

        }
        destinationRotation = Quaternion.AngleAxis(horizontalRotation, Vector3.up) * Quaternion.AngleAxis(verticalRotation, Vector3.right);

        transform.rotation = Quaternion.Slerp(transform.rotation, destinationRotation, lerpSpeed);

    }
}
