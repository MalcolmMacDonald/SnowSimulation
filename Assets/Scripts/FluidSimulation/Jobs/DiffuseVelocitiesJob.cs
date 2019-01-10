using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile(CompileSynchronously = true)]
public struct DiffuseVelocitiesJob : IJob
{
    [ReadOnly]
    float dt;
    [ReadOnly]
    float diff;
    [ReadOnly]
    uint gridSize;
    [ReadOnly]
    uint solverIterations;
    public NativeArray<float3> x;
    [ReadOnly]
    NativeArray<float3> x0;



    public void Execute()
    {
        uint i, j, k, q;
        float a = dt * diff * gridSize * gridSize;
        NativeArray<float3> xCopy = x;
        for (q = 0; q < solverIterations; q++)
        {

            for (i = 1; i <= gridSize; i++)
            {
                for (j = 1; j <= gridSize; j++)
                {
                    for (k = 1; k <= gridSize; k++)
                    {


                        x[ArrayIndex(i, j, k, gridSize)] =
                        (x0[ArrayIndex(i, j, k, gridSize)]
                        + a * (xCopy[ArrayIndex(i - 1, j, k, gridSize)]
                        + xCopy[ArrayIndex(i + 1, j, k, gridSize)]
                        + xCopy[ArrayIndex(i, j - 1, k, gridSize)]
                        + xCopy[ArrayIndex(i, j + 1, k, gridSize)]
                        + xCopy[ArrayIndex(i, j, k - 1, gridSize)]
                        + xCopy[ArrayIndex(i, j, k + 1, gridSize)])) / (1 + 6 * a);
                    }
                }
            }
            // SetVectorBoundaries(b, ref x);
        }
    }
    static int ArrayIndex(uint x, uint y, uint z, uint N)
    {
        return (int)(x + (y * N) + (z * N * N));
    }

}
