using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void AdvectVelocities(ref Vector3[,,] d, Vector3[,,] d0, float dt)
    {
        Vector3 dt0 = dt * new Vector3(gridSizeX, gridSizeY, gridSizeZ);

        for (int i = 1; i <= gridSizeX; i++)
        {
            for (int j = 1; j <= gridSizeY; j++)
            {
                for (int k = 1; k <= gridSizeZ; k++)
                {

                    Vector3 newPos = new Vector3(i, j, k) - Vector3.Scale(dt0, d0[i, j, k]);
                    d[i, j, k] = TrilinearInterpolation(newPos, d0);
                }
            }
        }

        SetVelocityBoundaries(ref d);
    }
}
