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

            if (resetParticlePositionAtBoundary)
            {
                if (newPos.x < 0 || newPos.x >= gridSizeX + 1 || newPos.y < 0 || newPos.y >= gridSizeY + 1 || newPos.z < 0 || newPos.z >= gridSizeZ + 1)
                {
                    v[i] = Vector3.zero;
                    Vector3 particleCellSize = new Vector3(gridSizeX / (float)particleGridSize, gridSizeY / (float)particleGridSize, gridSizeZ / (float)particleGridSize);
                    newPos = new Vector3(UnityEngine.Random.Range(1, gridSizeX + 1), UnityEngine.Random.Range(1, gridSizeY + 1), UnityEngine.Random.Range(1, gridSizeZ + 1));
                }
            }
            else
            {
                if (newPos.x < 1 || newPos.x >= gridSizeX + 1 || newPos.y < 1 || newPos.y >= gridSizeY + 1 || newPos.z < 1 || newPos.z >= gridSizeZ + 1)
                {
                    newPos.x = Mathf.Clamp(newPos.x, 1, gridSizeX + 1);
                    newPos.y = Mathf.Clamp(newPos.y, 1, gridSizeY + 1);
                    newPos.z = Mathf.Clamp(newPos.z, 1, gridSizeZ + 1);
                    v[i] = newPos - p[i];
                }


            }

            particles[i] = newPos;
        }
    }
}
