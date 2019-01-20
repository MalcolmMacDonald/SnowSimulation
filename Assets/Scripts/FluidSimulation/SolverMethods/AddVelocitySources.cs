using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void AddVelocitySources()
    {

        /*     for (int i = 1; i <= gridSize; i++)
            {
                for (int j = 1; j <= gridSize; j++)
                {
                    for (int k = 1; k <= gridSize; k++)
                    {
                        v[i, j, k] += dt * v0[i, j, k];
                    }
                }
            }*/


        AddMouseInput();
    }
    void AddMouseInput()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane simPlane = new Plane(Vector3.forward, 0);
        float rayDist = 0;

        simPlane.Raycast(mouseRay, out rayDist);

        Vector2 mousePos = mouseRay.origin + mouseRay.direction * rayDist;

        mousePos *= gridSize;
        mousePos.x = Mathf.Clamp(mousePos.x, 0, gridSize);
        mousePos.y = Mathf.Clamp(mousePos.y, 0, gridSize);


        mousePos = new Vector2((gridSize + 2) / 2f, (gridSize + 2) / 2f);
        // mousePos += Vector2.one * 0.5f;

        Vector3 currentDir = Vector3.forward * velocitySourceRate;//(mousePos - previousMousePos) * velocitySourceRate;

        // if (Input.GetMouseButton(0))
        {
            int centerIndex = ((gridSize + 2 - 1) / 2);
            velocities[centerIndex, centerIndex, 1] += currentDir;
            Debug.DrawRay((new Vector3(centerIndex, centerIndex, 0) + Vector3.one / 2f) * cellSize, -Vector3.forward);
        }

        previousMousePos = mousePos;
    }
}
