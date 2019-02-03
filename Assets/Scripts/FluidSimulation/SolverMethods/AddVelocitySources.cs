using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void AddVelocitySources()
    {

        AddMouseInput();
    }
    void AddMouseInput()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane simPlane = new Plane(Vector3.forward, 0);
        Vector3 boundsCenter = Vector3.Scale(cellSize, new Vector3(gridSizeX + 2, gridSizeY + 2, gridSizeZ + 2) / 2f);
        Bounds simBox = new Bounds(boundsCenter, boundsCenter * 2f);
        float rayDist = 0;


        if (!simBox.IntersectRay(mouseRay, out rayDist))
        {
            return;

        }

        Vector3 mousePos = mouseRay.origin + mouseRay.direction * rayDist;

        Vector3 currentDir = Vector3.forward * velocitySourceRate;

        mousePos.x = Mathf.Clamp(mousePos.x, 0, gridScale.x);// * gridSizeX;
        mousePos.y = Mathf.Clamp(mousePos.y, 0, gridScale.y);// * gridSizeY;
        mousePos.z = Mathf.Clamp(mousePos.z, 0, gridScale.z);// * gridSizeZ;


        Vector3 pointToCenter = (gridScale / 2f) - mousePos;
        pointToCenter.x /= gridScale.x;
        pointToCenter.y /= gridScale.y;
        pointToCenter.z /= gridScale.z;

        Vector3 singleComponentDirection = Vector3.zero;

        int directionComponent = -1;
        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(pointToCenter[i]) == Mathf.Max(Mathf.Abs(pointToCenter.x), Mathf.Abs(pointToCenter.y), Mathf.Abs(pointToCenter.z)))
            {
                singleComponentDirection[i] = Mathf.Sign(pointToCenter[i]);
                directionComponent = i;
                break;
            }
        }

        //    Debug.DrawRay(mousePos, singleComponentDirection);

        mousePos.x /= gridScale.x;
        mousePos.y /= gridScale.y;
        mousePos.z /= gridScale.z;

        if (Input.GetMouseButton(0))
        {

            int squareSize = (int)velocityInputRadius * 2;

            int x = Mathf.FloorToInt(mousePos.x * gridSizeX) + 1;
            int y = Mathf.FloorToInt(mousePos.y * gridSizeY) + 1;
            int z = Mathf.FloorToInt(mousePos.z * gridSizeZ) + 1;
            //  velocities[x, y, z] += singleComponentDirection * velocitySourceRate;

            for (int i = -squareSize / 2; i < squareSize / 2; i++)
            {
                for (int j = -squareSize / 2; j < squareSize / 2; j++)
                {
                    float distance = Mathf.Sqrt((float)((i * i) + (j * j)));
                    if (distance < velocityInputRadius)
                    {

                        switch (directionComponent)
                        {
                            case 0:
                                x = mousePos.x > (1 / 2f) ? gridSizeX - 1 : 1;
                                y = Mathf.FloorToInt(mousePos.y * gridSizeY) + 1 + i;
                                z = Mathf.FloorToInt(mousePos.z * gridSizeZ) + 1 + j;
                                break;
                            case 1:
                                y = mousePos.y > (1 / 2f) ? gridSizeY - 1 : 1;
                                x = Mathf.FloorToInt(mousePos.x * gridSizeX) + 1 + i;
                                z = Mathf.FloorToInt(mousePos.z * gridSizeZ) + 1 + j;
                                break;
                            case 2:
                                z = mousePos.z > (1 / 2f) ? gridSizeZ - 1 : 1;
                                x = Mathf.FloorToInt(mousePos.x * gridSizeX) + 1 + i;
                                y = Mathf.FloorToInt(mousePos.y * gridSizeY) + 1 + j;
                                break;
                        }


                        if (x > 0 && x < gridSizeX + 1 && y > 0 && y < gridSizeY + 1 && z > 0 && z < gridSizeZ + 1)
                        {
                            float velocityFalloff = (1 - Mathf.Pow(distance / velocityInputRadius, 2));

                            velocities[x, y, z] += singleComponentDirection * velocitySourceRate;
                        }
                    }
                }

            }

        }



        /* if (Input.GetMouseButton(0))
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
                        if (x > 0 && x < gridSizeX && y > 0 && y < gridSizeY)
                        {
                            float velocityFalloff = (1 - Mathf.Pow(distance / velocityInputRadius, 2));
                            velocities[x, y, 1] += currentDir * velocityFalloff;

                        }
                    }

                }
            }

        }*/

        //   previousMousePos = mousePos;
    }
}
