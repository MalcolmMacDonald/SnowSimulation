using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void AdvectVelocities(int b, ref Vector3[,,] d, Vector3[,,] d0, float dt)
    {

        float dt0 = dt * gridSize;

        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                for (int k = 1; k <= gridSize; k++)
                {

                    Vector3 newPos = dt0 * d[i, j, k];
                    d[i, j, k] = TrilinearInterpolation(newPos, d0);
                }
            }
        }
        SetVelocityBoundaries(b, ref d);
    }
}
