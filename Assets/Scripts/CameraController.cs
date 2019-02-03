using System.Collections;
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

        SimpleFluid fluid = FindObjectOfType<SimpleFluid>();
        Vector3 boundsCenter = Vector3.Scale(fluid.cellSize, new Vector3(fluid.gridSizeX + 2, fluid.gridSizeY + 2, fluid.gridSizeZ + 2) / 2f);
        transform.position = boundsCenter;
        transform.GetChild(0).localPosition = Vector3.back * boundsCenter.magnitude * 10f;
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
            //      rotationVelocity -= (rotationInput.normalized * (rotationInput.magnitude - maxRotationDistance)) * returnSpeed;
        }

        rotationInput += rotationVelocity;

        destinationRotation = Quaternion.AngleAxis(rotationInput.x, Vector3.up) * Quaternion.AngleAxis(rotationInput.y, Vector3.right);

        transform.rotation = Quaternion.Slerp(transform.rotation, destinationRotation, lerpSpeed);

    }
}
