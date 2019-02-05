using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    //TODO: Rework boundaries to make them sparse
    // use a list of  Vector3Int, Vector3Int tuples
    void SetBoundaryOffsets()
    {

        for (int i = 0; i < gridSizeX + 2; i++)
        {
            for (int j = 0; j < gridSizeY + 2; j++)
            {
                for (int k = 0; k < gridSizeZ + 2; k++)
                {
                    collisionGrid[i, j, k] = Physics.CheckBox(Vector3.Scale(new Vector3(0.5f + i, 0.5f + j, 0.5f + k), cellSize), cellSize / 2f);
                }
            }
        }
        for (int i = 0; i < gridSizeX + 2; i++)
        {
            for (int j = 0; j < gridSizeY + 2; j++)
            {
                for (int k = 0; k < gridSizeZ + 2; k++)
                {
                    if (!(i > 0 && i < gridSizeX + 1 && j > 0 && j < gridSizeY + 1 && k > 0 && k < gridSizeZ + 1))
                    {
                        boundaryOffsets[i, j, k] = GetSampledNormal(i, j, k, collisionGrid);
                    }
                    else
                    {
                        if (collisionGrid[i, j, k])
                        {
                            boundaryOffsets[i, j, k] = GetSampledNormal(i, j, k, collisionGrid);
                        }
                    }
                }
            }
        }
    }

    Vector3Int GetSampledNormal(int x, int y, int z, bool[,,] grid)
    {
        Vector3 foundNormal = new Vector3();
        int newX, newY, newZ;
        int foundCellsCount = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    if (!(i == 0 && j == 0 && k == 0))
                    {
                        newX = x + i;
                        newY = y + j;
                        newZ = z + k;

                        if (!(newX >= 0 && newX < gridSizeX + 2 && newY >= 0 && newY < gridSizeY + 2 && newZ >= 0 && newZ < gridSizeZ + 2))
                        {
                            foundNormal -= new Vector3(i, j, k);
                            foundCellsCount++;
                            continue;
                        }
                        if (grid[newX, newY, newZ])
                        {
                            foundNormal -= new Vector3(i, j, k);
                            foundCellsCount++;
                        }

                    }
                }
            }
        }
        Vector3Int foundVector = Vector3Int.RoundToInt(foundNormal);
        foundVector.Clamp(Vector3Int.one * -1, Vector3Int.one);
        return foundVector;
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
