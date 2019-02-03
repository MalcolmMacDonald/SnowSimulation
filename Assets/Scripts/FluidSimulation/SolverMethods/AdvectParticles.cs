using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SimpleFluid
{
    void AdvectParticleVelocities(ref Vector3[,,] v)
    {
        for (int i = 0; i < particleCount; i++)
        {
            particleVelocities[i] = (Vector3)TrilinearInterpolation(particles[i], v);
        }
    }
}
