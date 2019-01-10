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
                    newPos = new Vector3(UnityEngine.Random.Range(1f, gridSize), UnityEngine.Random.Range(1f, gridSize), 1);
                }
                if (addObstacle)
                {
                    if ((particles[i].x >= (gridSize / 3) - 1 && particles[i].x <= 2 * gridSize / 3 + 1) && (particles[i].y >= gridSize / 3 - 1 && particles[i].y <= 2 * gridSize / 3 + 1))
                    {
                        newPos = new Vector3(UnityEngine.Random.Range(1f, gridSize), UnityEngine.Random.Range(1f, gridSize), 1);
                        v[i] = Vector3.zero;
                    }
                }
            }
            else
            {
                newPos.x = Mathf.Clamp(newPos.x, 1, gridSize);
                newPos.y = Mathf.Clamp(newPos.y, 1, gridSize);

            }

            particles[i] = newPos;

        }
    }

}
