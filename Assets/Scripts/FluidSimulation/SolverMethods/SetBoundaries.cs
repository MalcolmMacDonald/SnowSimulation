using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void SetBoundaryOffsets()
    {
        int edgeValue = 0;
        for (int i = 1; i < gridSizeY + 1; i++)
        {
            for (int j = 1; j < gridSizeZ + 1; j++)
            {
                edgeValue = 0;
                boundaryOffsets[edgeValue, i, j].x = 1;
                edgeValue = gridSizeX + 1;
                boundaryOffsets[edgeValue, i, j].x = -1;

            }
        }
        for (int i = 1; i < gridSizeX + 1; i++)
        {
            for (int j = 1; j < gridSizeZ + 1; j++)
            {
                edgeValue = 0;
                boundaryOffsets[i, edgeValue, j].y = 1;
                edgeValue = gridSizeY + 1;
                boundaryOffsets[i, edgeValue, j].y = -1;
            }
        }
        for (int i = 1; i < gridSizeX + 1; i++)
        {
            for (int j = 1; j < gridSizeY + 1; j++)
            {
                edgeValue = 0;
                boundaryOffsets[i, j, edgeValue].z = 1;
                edgeValue = gridSizeZ + 1;
                boundaryOffsets[i, j, edgeValue].z = -1;
            }
        }
    }


    void SetVelocityBoundaries(ref Vector3[,,] x)
    {
        for (int i = 0; i < gridSizeX + 2; i++)
        {
            for (int j = 0; j < gridSizeY + 2; j++)
            {
                for (int k = 0; k < gridSizeZ + 2; k++)
                {
                    if (boundaryOffsets[i, j, k] != Vector3.zero)
                    {
                        x[i, j, k] = -x[i + boundaryOffsets[i, j, k].x, j + boundaryOffsets[i, j, k].y, k + boundaryOffsets[i, j, k].z];
                    }
                }
            }
        }
    }


    void SetFloatBoundaries(ref float[,,] x)
    {
        for (int i = 0; i < gridSizeX + 2; i++)
        {
            for (int j = 0; j < gridSizeY + 2; j++)
            {
                for (int k = 0; k < gridSizeZ + 2; k++)
                {
                    if (boundaryOffsets[i, j, k] != Vector3.zero)
                    {
                        x[i, j, k] = -x[i + boundaryOffsets[i, j, k].x, j + boundaryOffsets[i, j, k].y, k + boundaryOffsets[i, j, k].z];
                    }
                }
            }
        }
    }
}
