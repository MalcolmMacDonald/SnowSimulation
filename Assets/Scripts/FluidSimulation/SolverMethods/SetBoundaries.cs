using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void SetVelocityBoundaries(ref Vector3[,,] x)
    {
        int edgeValue = 0;
        for (int i = 1; i < gridSizeY + 1; i++)
        {
            for (int j = 1; j < gridSizeZ + 1; j++)
            {
                edgeValue = 0;
                x[edgeValue, i, j].x = -x[edgeValue + 1, i, j].x;
                edgeValue = gridSizeX + 1;
                x[edgeValue, i, j].x = -x[edgeValue - 1, i, j].x;

            }
        }
        for (int i = 1; i < gridSizeX + 1; i++)
        {
            for (int j = 1; j < gridSizeZ + 1; j++)
            {
                edgeValue = 0;
                x[i, edgeValue, j].y = -x[i, edgeValue + 1, j].y;
                edgeValue = gridSizeY + 1;
                x[i, edgeValue, j].y = -x[i, edgeValue - 1, j].y;
            }
        }
        for (int i = 1; i < gridSizeX + 1; i++)
        {
            for (int j = 1; j < gridSizeY + 1; j++)
            {
                edgeValue = 0;
                x[i, j, edgeValue].z = -x[i, j, edgeValue + 1].z;
                edgeValue = gridSizeZ + 1;
                x[i, j, edgeValue].z = -x[i, j, edgeValue - 1].z;
            }
        }


    }


    void SetFloatBoundaries(ref float[,,] x)
    {
        int edgeValue;
        for (int i = 1; i < gridSizeY + 1; i++)
        {
            for (int j = 1; j < gridSizeZ + 1; j++)
            {
                edgeValue = 0;
                x[edgeValue, i, j] = -x[edgeValue + 1, i, j];
                edgeValue = gridSizeX + 1;
                x[edgeValue, i, j] = -x[edgeValue - 1, i, j];

            }
        }
        for (int i = 1; i < gridSizeX + 1; i++)
        {
            for (int j = 1; j < gridSizeZ + 1; j++)
            {

                edgeValue = 0;
                x[i, edgeValue, j] = -x[i, edgeValue + 1, j];
                edgeValue = gridSizeY + 1;
                x[i, edgeValue, j] = -x[i, edgeValue - 1, j];
            }
        }
        for (int i = 1; i < gridSizeX + 1; i++)
        {
            for (int j = 1; j < gridSizeY + 1; j++)
            {
                edgeValue = 0;
                x[i, j, edgeValue] = -x[i, j, edgeValue + 1];
                edgeValue = gridSizeZ + 1;
                x[i, j, edgeValue] = -x[i, j, edgeValue - 1];

            }
        }

    }
}
