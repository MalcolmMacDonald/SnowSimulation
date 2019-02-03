using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{

    void AddVorticity(float multiplier, ref Vector3[,,] u, Vector3[,,] prevU)
    {
        Vector3[,,] curl = new Vector3[gridSizeX + 2, gridSizeY + 2, gridSizeZ + 2];

        for (int i = 1; i <= gridSizeX; i++)
        {
            for (int j = 1; j <= gridSizeY; j++)
            {
                for (int k = 1; k <= gridSizeZ; k++)
                {
                    curl[i, j, k] = (1f / 3f) * new Vector3(prevU[i + 1, j, k].x - prevU[i - 1, j, k].x,
                                                           prevU[i, j + 1, k].y - prevU[i, j - 1, k].y,
                                                           prevU[i, j, k + 1].z - prevU[i, j, k - 1].z);
                }
            }
        }
        SetVelocityBoundaries(ref curl);

        for (int i = 1; i <= gridSizeX; i++)
        {
            for (int j = 1; j <= gridSizeY; j++)
            {
                for (int k = 1; k <= gridSizeZ; k++)
                {
                    float dwdx = 0.5f * (curl[i + 1, j, k].x - curl[i - 1, j, k].x);
                    float dwdy = 0.5f * (curl[i, j + 1, k].y - curl[i, j - 1, k].y);
                    float dwdz = 0.5f * (curl[i, j, k + 1].z - curl[i, j, k - 1].z);

                    u[i, j, k] += new Vector3(dwdx, dwdy, dwdz).normalized * multiplier;
                }
            }
        }
    }
}
