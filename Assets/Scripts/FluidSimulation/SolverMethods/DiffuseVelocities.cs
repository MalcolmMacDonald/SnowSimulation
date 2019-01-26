using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{

    void DiffuseVelocities(ref Vector3[,,] x, Vector3[,,] x0, float diff, float dt)
    {
        int gridSizeCubed = gridSize * gridSize * gridSize;
        float a = dt * diff * (float)gridSizeCubed;
        float c = (1f + (6f * a));
        Vector3[,,] xCopy = (Vector3[,,])x.Clone();

        for (int q = 0; q < solverIterations; q++)
        {

            for (int i = 1; i <= gridSize; i++)
            {
                for (int j = 1; j <= gridSize; j++)
                {
                    for (int k = 1; k <= gridSize; k++)
                    {
                        Vector3 thisPrev = x0[i, j, k];
                        Vector3 prev0 = xCopy[i - 1, j, k];
                        Vector3 prev1 = xCopy[i + 1, j, k];
                        Vector3 prev2 = xCopy[i, j - 1, k];
                        Vector3 prev3 = xCopy[i, j + 1, k];
                        Vector3 prev4 = xCopy[i, j, k - 1];
                        Vector3 prev5 = xCopy[i, j, k + 1];

                        x[i, j, k] = (thisPrev + (a * (prev0 + prev1 + prev2 + prev3 + prev4 + prev5))) / c;
                    }
                }
            }

            SetVelocityBoundaries(ref x);
        }
    }
}
