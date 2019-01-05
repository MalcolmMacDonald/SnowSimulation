using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NSSnow : MonoBehaviour
{

    public int size = 50;
    public float displayScale = 0.125f;
    Vector2[] velocity;
    Vector2[] velocityPrev;
    float[] density;
    float[] densityPrev;

    float[] densitySources;

    bool isSelected;

    public float feedRate = 1;

    public float diff = 1.4f;

    public int calculationsPerFrame = 1;
    // Use this for initialization
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        SimulateGrid();
    }
    void Initialize()
    {
        velocity = new Vector2[(size + 2) * (size + 2)];
        velocityPrev = new Vector2[(size + 2) * (size + 2)];
      //  velocity[ArrayIndex(size/2,size/2)] = -Vector2.one/5f;
        for (int i = 1; i <= size; i++)
        {
            for (int j = 1; j <= size; j++)
            {
               velocity[ArrayIndex(i, j)] = Random.insideUnitCircle;
            }
        }
        
        density = new float[(size + 2) * (size + 2)];
        densityPrev = new float[(size + 2) * (size + 2)];
        densitySources = new float[(size + 2) * (size + 2)];
    }
    void SimulateGrid()
    {
        densityStep();
        velocityStep();
    }
    void densityStep()
    {
        densitySources = new float[(size + 2) * (size + 2)];
        densitySources[ArrayIndex(Mathf.RoundToInt(Mathf.Sin(Time.time) * size / 5) + size / 2, Mathf.RoundToInt(Mathf.Cos(Time.time) * size / 5) + size / 2)] = feedRate;

        addDensitySources();
        diffuseDensity();
        advectDensity(density, densityPrev, velocity);
        densityPrev = density;
    }
    void velocityStep()
    {
        addVelocitySources();
        diffuseVelocity();
        advectVelocity();
        projectVelocity();
        velocityPrev = velocity;
    }

    void addDensitySources()
    {
        for (int i = 0; i < density.Length; i++)
        {
            density[i] += Time.deltaTime * densitySources[i];
        }

    }
    void addVelocitySources()
    {
        for (int i = 0; i < velocity.Length; i++)
        {
            velocity[i] += Time.deltaTime * velocityPrev[i];
        }
    }
    void diffuseDensity()
    {
        float a = Time.deltaTime * diff * size * size;
        for (int k = 0; k < calculationsPerFrame; k++)
        {
            for (int i = 1; i <= size; i++)
            {
                for (int j = 1; j <= size; j++)
                {
                    float neighborSums = density[ArrayIndex(i, j + 1)] + density[ArrayIndex(i + 1, j)] + density[ArrayIndex(i, j - 1)] + density[ArrayIndex(i - 1, j)];

                    density[ArrayIndex(i, j)] = (densityPrev[ArrayIndex(i, j)] + a * (neighborSums)) / (i + 4 * a);
                   
                }
            }
        }
    }
    void diffuseVelocity()
    {
        float a = Time.deltaTime * diff * size * size;
        for (int k = 0; k < calculationsPerFrame; k++)
        {
            for (int i = 1; i <= size; i++)
            {
                for (int j = 1; j <= size; j++)
                {
                    Vector2 neighborSums = velocity[ArrayIndex(i, j + 1)] + velocity[ArrayIndex(i + 1, j)] + velocity[ArrayIndex(i, j - 1)] + velocity[ArrayIndex(i - 1, j)];

                    velocity[ArrayIndex(i, j)] = (velocityPrev[ArrayIndex(i, j)] + a * (neighborSums)) / (i + 4 * a);
                }
            }
        }
    }


    void advectDensity(float[] d, float[] d0, Vector2[] vel)
    {
        float dt0 = Time.deltaTime * size;
        float x, y;
        float s0, t0, s1, t1, prevLerp;
        int i0, j0, i1, j1;

        for (int i = 1; i <= size; i++)
        {
            for (int j = 1; j <= size; j++)
            {
                x = i - dt0 * vel[ArrayIndex(i, j)].x;
                y = j - dt0 * vel[ArrayIndex(i, j)].y;
                if (x < 0.5f)
                {
                    x = 0.5f;
                }
                if (x > size + 0.5f)
                {
                    x = size + 0.5f;
                }
                i0 = (int)x;
                i1 = i0 + 1;

                if (y < 0.5f)
                {
                    y = 0.5f;
                }
                if (y > size + 0.5f)
                {
                    y = size + 0.5f;
                }
                j0 = (int)y;
                j1 = (int)y + 1;

                s1 = x - i0;
                s0 = 1 - s1;
                t1 = y - j0;
                t0 = 1 - t1;
                prevLerp = s0 * (t0 * d0[ArrayIndex(i0, j0)] + t1 * d0[ArrayIndex(i0, j1)]) + s1 * (t0 * d0[ArrayIndex(i1, j0)] + t1 * d0[ArrayIndex(i1, j1)]);

                d[ArrayIndex(i, j)] = prevLerp;

            }
        }
    }
    void advectVelocity()
    {
        float dt0 = Time.deltaTime * size;
        float x, y;
        float s0, t0, s1, t1;
        Vector2 prevLerp;
        int i0, j0, i1, j1;

        for (int i = 1; i <= size; i++)
        {
            for (int j = 1; j <= size; j++)
            {
                x = i - dt0 * velocity[ArrayIndex(i, j)].x;
                y = j - dt0 * velocity[ArrayIndex(i, j)].y;
                if (x < 0.5f)
                {
                    x = 0.5f;
                }
                if (x > size + 0.5f)
                {
                    x = size + 0.5f;
                }
                i0 = (int)x;
                i1 = i0 + 1;

                if (y < 0.5f)
                {
                    y = 0.5f;
                }
                if (y > size + 0.5f)
                {
                    y = size + 0.5f;
                }
                j0 = (int)y;
                j1 = (int)y + 1;

                s1 = x - i0;
                s0 = 1 - s1;
                t1 = y - j0;
                t0 = 1 - t1;
                prevLerp = s0 * (t0 * velocityPrev[ArrayIndex(i0, j0)] + t1 * velocityPrev[ArrayIndex(i0, j1)]) + s1 * (t0 * velocityPrev[ArrayIndex(i1, j0)] + t1 * velocityPrev[ArrayIndex(i1, j1)]);

                velocity[ArrayIndex(i, j)] = prevLerp;
            }
        }
    }
    void projectVelocity()
    {
        float h;
        h = 1;

        for (int i = 1; i <= size; i++)
        {
            for (int j = 1; j <= size; j++)
            {
                velocityPrev[ArrayIndex(i, j)].y = -0.5f * h * (velocity[ArrayIndex(i + 1, j)].x - velocity[ArrayIndex(i - 1, j)].x + velocity[ArrayIndex(i, j + 1)].y - velocity[ArrayIndex(i, j - 1)].y);
                velocityPrev[ArrayIndex(i, j)].x = 0;
            }
        }
    //    handleVelocityBoundaries(velocityPrev);
        for (int k = 0; k < calculationsPerFrame; k++)
        {
            for (int i = 1; i <= size; i++)
            {
                for (int j = 1; j <= size; j++)
                {
                    float lerpValue = (velocityPrev[ArrayIndex(i, j)].y + velocityPrev[ArrayIndex(i - 1, j)].x + velocityPrev[ArrayIndex(i + 1, j)].x + velocityPrev[ArrayIndex(i, j - 1)].x + velocityPrev[ArrayIndex(i, j + 1)].x) / 4f;
                    velocityPrev[ArrayIndex(i, j)].x = lerpValue;
                }
            }
       //     handleVelocityBoundaries(velocityPrev);
        }

        for (int i = 1; i <= size; i++)
        {
            for(int j = 1; j <= size; j++)
            {
                velocity[ArrayIndex(i, j)].x -= 0.5f * (velocityPrev[ArrayIndex(i + 1, j)].x - velocityPrev[ArrayIndex(i - 1, j)].x) / h;
                velocity[ArrayIndex(i, j)].y -= 0.5f * (velocityPrev[ArrayIndex(i, j+1)].y - velocityPrev[ArrayIndex(i, j-1)].y) / h;
             //   velocity[ArrayIndex(i, j)] = Vector2.ClampMagnitude(velocity[ArrayIndex(i, j)], 1);
            }
        }
       // handleVelocityBoundaries(velocity);
    }

    void handleVelocityBoundaries(Vector2[] vel)
    {
        for(int i = 0; i <= size+1; i++)
        {
            for (int j = 0; j <= size + 1; j++)
            {
                if (i == 0 || i == size + 1)
                {
                    vel[ArrayIndex(i, j)].x = 0;
                }
                if (j == 0 || j == size + 1)
                {
                    vel[ArrayIndex(i, j)].y = 0;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Vector3 startPos;
            Vector3 endPos;
            for (int i = 1; i <= size; i++)
            {
                for (int j = 1; j <= size; j++)
                {
                    Gizmos.color = Color.red;
                    startPos = transform.position + (Vector3)(transform.localToWorldMatrix * new Vector3(i, j) * displayScale);
                    endPos = transform.localToWorldMatrix * velocity[ArrayIndex(i, j)] * displayScale;
                    Gizmos.DrawRay(startPos, endPos);
                    Gizmos.DrawWireCube(startPos + endPos, Vector3.one * transform.localScale.x * displayScale / 15f);

                    Gizmos.color = new Color(1, 1, 1);
                    Gizmos.DrawCube(startPos, Vector3.one * Mathf.Sqrt(density[ArrayIndex(i, j)]));
                }
            }

        }
    }
    int ArrayIndex(int i, int j)
    {
        return (i + (size + 2) * j);
    }
    float sigmoid(float x)
    {
        return 1f / (1 + Mathf.Exp(-x));
    }

}
