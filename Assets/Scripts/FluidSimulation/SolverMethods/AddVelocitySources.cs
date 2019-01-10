using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void AddVelocitySources()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane simPlane = new Plane(Vector3.forward, 0);
        float rayDist = 0;

        simPlane.Raycast(mouseRay, out rayDist);

        Vector2 mousePos = mouseRay.origin + mouseRay.direction * rayDist;

        mousePos *= gridSize;
        mousePos.x = Mathf.Clamp(mousePos.x, 0, gridSize);
        mousePos.y = Mathf.Clamp(mousePos.y, 0, gridSize);
        mousePos += Vector2.one * 0.5f;

        Vector3 currentDir = Vector3.forward * velocitySourceRate;//(mousePos - previousMousePos) * velocitySourceRate;

        if (Input.GetMouseButton(0))
        {
            velocities[(int)mousePos.x, (int)mousePos.y, 1] = currentDir;
        }

        previousMousePos = mousePos;
    }
}
