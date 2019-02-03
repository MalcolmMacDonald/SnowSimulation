using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{

    void ProjectVelocities(ref Vector3[,,] u)
    {
        Vector3 h = new Vector3(1f / gridSizeX, 1f / gridSizeY, 1f / gridSizeZ);
        float[,,] pressure = new float[gridSizeX + 2, gridSizeY + 2, gridSizeZ + 2];
        float[,,] divergence = new float[gridSizeX + 2, gridSizeY + 2, gridSizeZ + 2];

        for (int i = 1; i <= gridSizeX; i++)
        {
            for (int j = 1; j <= gridSizeY; j++)
            {
                for (int k = 1; k <= gridSizeZ; k++)
                {
                    float xDiv = (u[i + 1, j, k].x - u[i - 1, j, k].x) / (float)gridSizeX;
                    float yDiv = (u[i, j + 1, k].y - u[i, j - 1, k].y) / (float)gridSizeY;
                    float zDiv = (u[i, j, k + 1].z - u[i, j, k - 1].z) / (float)gridSizeZ;
                    divergence[i, j, k] = -(1f / 3f) * (xDiv + yDiv + zDiv);
                }
            }
        }

        SetFloatBoundaries(ref divergence);
        SetFloatBoundaries(ref pressure);
        float[,,] pCopy = (float[,,])pressure.Clone();

        for (int q = 0; q < solverIterations; q++)
        {

            for (int i = 1; i <= gridSizeX; i++)
            {
                for (int j = 1; j <= gridSizeY; j++)
                {
                    for (int k = 1; k <= gridSizeZ; k++)
                    {
                        pressure[i, j, k] =
                        (
                          divergence[i, j, k]
                        + pCopy[i - 1, j, k]
                        + pCopy[i + 1, j, k]
                        + pCopy[i, j - 1, k]
                        + pCopy[i, j + 1, k]
                        + pCopy[i, j, k - 1]
                        + pCopy[i, j, k + 1]
                        ) / 6f;
                    }
                }
            }

            SetFloatBoundaries(ref pressure);
        }

        for (int i = 1; i <= gridSizeX; i++)
        {
            for (int j = 1; j <= gridSizeY; j++)
            {
                for (int k = 1; k <= gridSizeZ; k++)
                {
                    u[i, j, k].x -= 0.5f * gridSizeX * (pressure[i + 1, j, k] - pressure[i - 1, j, k]);
                    u[i, j, k].y -= 0.5f * gridSizeY * (pressure[i, j + 1, k] - pressure[i, j - 1, k]);
                    u[i, j, k].z -= 0.5f * gridSizeZ * (pressure[i, j, k + 1] - pressure[i, j, k - 1]);
                }
            }
        }

        SetVelocityBoundaries(ref u);
    }
}
