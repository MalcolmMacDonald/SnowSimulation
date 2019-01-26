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
                if (newPos.x < 0 || newPos.x >= gridSize + 1 || newPos.y < 0 || newPos.y >= gridSize + 1 || newPos.z < 0 || newPos.z >= gridSize + 1)
                {
                    v[i] = Vector3.zero;
                    float particleCellSize = gridSize / (float)particleGridSize;
                    int yPos = Mathf.FloorToInt((float)i / (float)particleGridSize);
                    int xPos = (i % particleGridSize);
                    newPos = new Vector3(xPos + 1.5f, yPos + 1.5f) * particleCellSize + Vector3.forward;
                }
            }
            else
            {
                if (newPos.x < 1 || newPos.x >= gridSize + 1 || newPos.y < 1 || newPos.y >= gridSize + 1 || newPos.z < 1 || newPos.z >= gridSize + 1)
                {
                    newPos.x = Mathf.Clamp(newPos.x, 1, gridSize + 1);
                    newPos.y = Mathf.Clamp(newPos.y, 1, gridSize + 1);
                    newPos.z = Mathf.Clamp(newPos.z, 1, gridSize + 1);
                    v[i] = newPos - p[i];
                }


            }

            particles[i] = newPos;
        }
    }
}
