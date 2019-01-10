using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{

    void AddVorticity(float multiplier, ref Vector3[,,] u, Vector3[,,] prevU)
    {

        Vector3[,,] curl = new Vector3[gridSize + 2, gridSize + 2, gridSize + 2];
        float h = 1f / gridSize;


        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                for (int k = 1; k <= gridSize; k++)
                {
                    curl[i, j, k] = 0.5f * new Vector3((prevU[i + 1, j, k].x - prevU[i - 1, j, k].x), prevU[i, j + 1, k].y - prevU[i, j - 1, k].y, prevU[i, j, k + 1].z - prevU[i, j, k - 1].z);
                }
            }
        }


        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                for (int k = 1; k <= gridSize; k++)
                {
                    float dwdx = (curl[i + 1, j, k].x - curl[i - 1, j, k].x) * 0.5f;
                    float dwdy = (curl[i, j + 1, k].y - curl[i, j - 1, k].y) * 0.5f;
                    float dwdz = (curl[i, j, k + 1].y - curl[i, j, k - 1].y) * 0.5f;


                    Vector3 normCurl = new Vector3(dwdx, dwdy, dwdz).normalized * multiplier;
                    //  Vector2 gradient = 0.5f * new Vector2(curl[i + 1, j].magnitude - curl[i - 1, j].magnitude, +curl[i, j + 1].magnitude - curl[i, j - 1].magnitude).normalized;

                    //   Vector2 multipliedVorticity = Vector3.Cross(curl[i, j], gradient) * h * vorticityMutiplier;
                    if (drawVorticity)
                    {
                        //      Debug.DrawRay(new Vector2(0.5f + i, 0.5f + j) * cellSize, Vector2.ClampMagnitude(multipliedVorticity, cellSize), Color.yellow);
                    }
                    u[i, j, k] += normCurl;
                }
            }
        }
    }
}
