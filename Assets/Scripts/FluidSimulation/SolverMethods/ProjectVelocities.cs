using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{

    void ProjectVelocities(ref Vector3[,,] u)
    {
        float h = 1f / (float)gridSize;
        float[,,] pressure = new float[gridSize + 2, gridSize + 2, gridSize + 2];
        float[,,] divergence = new float[gridSize + 2, gridSize + 2, gridSize + 2];

        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                for (int k = 1; k <= gridSize; k++)
                {
                    float xDiv = (u[i + 1, j, k].x - u[i - 1, j, k].x) / (float)gridSize;
                    float yDiv = (u[i, j + 1, k].y - u[i, j - 1, k].y) / (float)gridSize;
                    float zDiv = (u[i, j, k + 1].z - u[i, j, k - 1].z) / (float)gridSize;
                    divergence[i, j, k] = -(1f / 3f) * (xDiv + yDiv + zDiv);
                }
            }
        }

        SetFloatBoundaries(ref divergence);
        SetFloatBoundaries(ref pressure);

        //float[,,] pCopy = pressure;
        for (int q = 0; q < solverIterations; q++)
        {
            for (int i = 1; i <= gridSize; i++)
            {
                for (int j = 1; j <= gridSize; j++)
                {
                    for (int k = 1; k <= gridSize; k++)
                    {
                        pressure[i, j, k] =
                        (
                          divergence[i, j, k]
                        + pressure[i - 1, j, k]
                        + pressure[i + 1, j, k]
                        + pressure[i, j - 1, k]
                        + pressure[i, j + 1, k]
                        + pressure[i, j, k - 1]
                        + pressure[i, j, k + 1]
                        ) / 6f;
                    }
                }
            }

            SetFloatBoundaries(ref pressure);
        }

        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                for (int k = 1; k <= gridSize; k++)
                {
                    u[i, j, k].x -= 0.5f * gridSize * (pressure[i + 1, j, k] - pressure[i - 1, j, k]);
                    u[i, j, k].y -= 0.5f * gridSize * (pressure[i, j + 1, k] - pressure[i, j - 1, k]);
                    u[i, j, k].z -= 0.5f * gridSize * (pressure[i, j, k + 1] - pressure[i, j, k - 1]);
                }
            }
        }

        SetVelocityBoundaries(ref u);
    }
}
