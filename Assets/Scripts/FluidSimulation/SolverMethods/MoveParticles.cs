using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void MoveParticles(ref Vector3[] p, ref Vector3[] v)
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 newPos = p[i] + v[i] * timeStep * particleVelocity;
            newPos.x = Mathf.Clamp(newPos.x, 0, gridSizeX + 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, gridSizeY + 1);
            newPos.z = Mathf.Clamp(newPos.z, 0, gridSizeZ + 1);

            Vector3Int clampedPos = Vector3Int.FloorToInt(newPos);
            Vector3Int boundaryInt = boundaryOffsets[ArrayIndex(clampedPos.x, clampedPos.y, clampedPos.z)];


            if (boundaryInt != Vector3Int.zero || collisionGrid[ArrayIndex(clampedPos.x, clampedPos.y, clampedPos.z)])
            {
                if (resetParticlePositionAtBoundary)
                {
                    v[i] = Vector3.zero;
                    Vector3 particleCellSize = new Vector3(gridSizeX / (float)particleGridSize, gridSizeY / (float)particleGridSize, gridSizeZ / (float)particleGridSize);
                    newPos = new Vector3(UnityEngine.Random.Range(1, gridSizeX + 1f), UnityEngine.Random.Range(1, gridSizeY + 1f), UnityEngine.Random.Range(1, gridSizeZ + 1f));
                }
                else
                {
                    v[i] = newPos - p[i];
                    newPos = p[i] + v[i] * timeStep * particleVelocity;
                }
            }

            particles[i] = newPos;
        }
    }
}
