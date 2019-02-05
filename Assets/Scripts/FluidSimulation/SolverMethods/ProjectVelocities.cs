using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{

    void ProjectVelocities(ref Vector3[] u)
    {
        Vector3 h = new Vector3(1f / gridSizeX, 1f / gridSizeY, 1f / gridSizeZ);
        float[] pressure = new float[(gridSizeX + 2) * (gridSizeY + 2) * (gridSizeZ + 2)];
        float[] divergence = new float[(gridSizeX + 2) * (gridSizeY + 2) * (gridSizeZ + 2)];

        for (int i = 1; i <= gridSizeX; i++)
        {
            for (int j = 1; j <= gridSizeY; j++)
            {
                for (int k = 1; k <= gridSizeZ; k++)
                {
                    float xDiv = (u[ArrayIndex(i + 1, j, k)].x - u[ArrayIndex(i - 1, j, k)].x) / (float)gridSizeX;
                    float yDiv = (u[ArrayIndex(i, j + 1, k)].y - u[ArrayIndex(i, j - 1, k)].y) / (float)gridSizeY;
                    float zDiv = (u[ArrayIndex(i, j, k + 1)].z - u[ArrayIndex(i, j, k - 1)].z) / (float)gridSizeZ;
                    divergence[ArrayIndex(i, j, k)] = -(1f / 3f) * (xDiv + yDiv + zDiv);
                }
            }
        }

        SetFloatBoundaries(ref divergence);
        SetFloatBoundaries(ref pressure);
        float[] pCopy = (float[])pressure.Clone();

        for (int q = 0; q < solverIterations; q++)
        {

            for (int i = 1; i <= gridSizeX; i++)
            {
                for (int j = 1; j <= gridSizeY; j++)
                {
                    for (int k = 1; k <= gridSizeZ; k++)
                    {
                        pressure[ArrayIndex(i, j, k)] =
                        (
                          divergence[ArrayIndex(i, j, k)]
                        + pCopy[ArrayIndex(i - 1, j, k)]
                        + pCopy[ArrayIndex(i + 1, j, k)]
                        + pCopy[ArrayIndex(i, j - 1, k)]
                        + pCopy[ArrayIndex(i, j + 1, k)]
                        + pCopy[ArrayIndex(i, j, k - 1)]
                        + pCopy[ArrayIndex(i, j, k + 1)]
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
                    u[ArrayIndex(i, j, k)].x -= 0.5f * gridSizeX * (pressure[ArrayIndex(i + 1, j, k)] - pressure[ArrayIndex(i - 1, j, k)]);
                    u[ArrayIndex(i, j, k)].y -= 0.5f * gridSizeY * (pressure[ArrayIndex(i, j + 1, k)] - pressure[ArrayIndex(i, j - 1, k)]);
                    u[ArrayIndex(i, j, k)].z -= 0.5f * gridSizeZ * (pressure[ArrayIndex(i, j, k + 1)] - pressure[ArrayIndex(i, j, k - 1)]);
                }
            }
        }

        SetVelocityBoundaries(ref u);
    }
}
