using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void SetVelocityBoundaries(int b, ref Vector3[,,] x)
    {
        Vector3 centerPoint = Vector3.one * gridSize / 2f;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                for (int k = 0; k < gridSize; k++)
                {

                    if (i == 0 || i >= gridSize || j == 0 || j >= gridSize || k == 0 || k >= gridSize)
                    {
                        x[i, j, k] = Vector3.zero;
                    }
                }
            }
        }

    }

    void SetFloatBoundaries(int b, ref float[,,] x)
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                for (int k = 0; k < gridSize; k++)
                {

                    if (i == 0 || i >= gridSize || j == 0 || j >= gridSize || k == 0 || k >= gridSize)
                    {
                        x[i, j, k] = 0;
                    }

                }
            }
        }

    }
}
