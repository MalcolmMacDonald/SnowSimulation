using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void SetVelocityBoundaries(ref Vector3[,,] x)
    {
        Vector3 centerPoint = Vector3.one * gridSize / 2f;
        int edgeValue = 0;
        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                edgeValue = 0;
                x[i, j, edgeValue].z = -x[i, j, edgeValue + 1].z;
                edgeValue = gridSize + 1;
                x[i, j, edgeValue].z = -x[i, j, edgeValue - 1].z;

                edgeValue = 0;
                x[edgeValue, i, j].x = -x[edgeValue + 1, i, j].x;
                edgeValue = gridSize + 1;
                x[edgeValue, i, j].x = -x[edgeValue - 1, i, j].x;

                edgeValue = 0;
                x[i, edgeValue, j].y = -x[i, edgeValue + 1, j].y;
                edgeValue = gridSize + 1;
                x[i, edgeValue, j].y = -x[i, edgeValue - 1, j].y;
            }
        }
    }


    void SetFloatBoundaries(ref float[,,] x)
    {
        int edgeValue;
        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                edgeValue = 0;
                x[i, j, edgeValue] = -x[i, j, edgeValue + 1];
                edgeValue = gridSize + 1;
                x[i, j, edgeValue] = -x[i, j, edgeValue - 1];

                edgeValue = 0;
                x[edgeValue, i, j] = -x[edgeValue + 1, i, j];
                edgeValue = gridSize + 1;
                x[edgeValue, i, j] = -x[edgeValue - 1, i, j];

                edgeValue = 0;
                x[i, edgeValue, j] = -x[i, edgeValue + 1, j];
                edgeValue = gridSize + 1;
                x[i, edgeValue, j] = -x[i, edgeValue - 1, j];
            }
        }

    }
}
