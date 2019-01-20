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
                if (newPos.x < 1 || newPos.x >= gridSize || newPos.y < 1 || newPos.y >= gridSize || newPos.z < 1 || newPos.z >= gridSize)
                {
                    v[i] = Vector3.zero;
                    newPos = (Vector3)(new Vector2(Mathf.FloorToInt(i / particleGridSize) + 1, i % particleGridSize + 1) * gridSize / (float)particleGridSize) + Vector3.forward;
                }
            }
            else
            {
                if (newPos.x < 1 || newPos.x >= gridSize || newPos.y < 1 || newPos.y >= gridSize || newPos.z < 1 || newPos.z >= gridSize)
                {
                    newPos.x = Mathf.Clamp(newPos.x, 1, gridSize);
                    newPos.y = Mathf.Clamp(newPos.y, 1, gridSize);
                    newPos.z = Mathf.Clamp(newPos.z, 1, gridSize);
                    v[i] = newPos - p[i];
                }


            }

            particles[i] = newPos;
        }
    }
}
