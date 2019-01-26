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


        //mousePos = new Vector2((gridSize + 2) / 2f, (gridSize + 2) / 2f);
        // mousePos += Vector2.one * 0.5f;

        Vector3 currentDir = Vector3.forward * velocitySourceRate;//(mousePos - previousMousePos) * velocitySourceRate;

        if (Input.GetMouseButton(0))
        {
            int squareSize = (int)velocityInputRadius * 2;

            for (int i = -squareSize / 2; i < squareSize / 2; i++)
            {
                for (int j = -squareSize / 2; j < squareSize / 2; j++)
                {
                    float distance = Mathf.Sqrt((float)((i * i) + (j * j)));
                    if (distance < velocityInputRadius)
                    {
                        int x = (int)mousePos.x + 1 + i;
                        int y = (int)mousePos.y + 1 + j;
                        if (x > 0 && x < gridSize && y > 0 && y < gridSize)
                        {
                            float velocityFalloff = (1 - Mathf.Pow(distance / velocityInputRadius, 2));
                            velocities[x, y, 1] += currentDir * velocityFalloff;

                        }
                    }

                }
            }

            // Debug.DrawRay((new Vector3(centerIndex, centerIndex, 0) + Vector3.one / 2f) * cellSize, -Vector3.forward);
        }

        previousMousePos = mousePos;
    }
}
