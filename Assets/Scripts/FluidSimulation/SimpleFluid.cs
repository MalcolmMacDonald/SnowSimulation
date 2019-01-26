﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;

public partial class SimpleFluid : MonoBehaviour
{
    const float gridScale = 1f;
    float cellSize;

    public int gridSize;
    public float timeStep = 0.1f;



    Vector3[,,] velocities;
    Vector3[,,] prevVelocities;

    public bool drawGrid;
    public bool drawVelocities;
    public bool drawVorticity;
    public bool drawParticles;
    public bool drawParticleMesh;


    public bool addObstacle;
    public bool resetParticlePositionAtBoundary;

    public int solverIterations;
    public float velocitySourceRate;
    public float viscocity;
    public float vorticityMutiplier;
    public float particleVelocity;


    public Color gridColor;
    public Color velocitiesColor;
    public Color particlesColor;

    float timer;
    Vector2 previousMousePos;


    Vector3[] particles;
    Vector3[] particleVelocities;
    public int particleGridSize;
    int particleCount;
    public float velocityInputRadius;



    public bool diffuse;
    public bool advect;
    public bool project;
    public bool vorticity;


    // Use this for initialization
    void Start()
    {
        cellSize = gridScale / (gridSize + 2);
        particleCount = particleGridSize * particleGridSize;
        particles = new Vector3[particleCount];
        particleVelocities = new Vector3[particleCount];
        velocities = new Vector3[gridSize + 2, gridSize + 2, gridSize + 2];
        prevVelocities = new Vector3[gridSize + 2, gridSize + 2, gridSize + 2];

        Vector2 previousMousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition) * gridSize;
        previousMousePos.x = Mathf.Clamp(previousMousePos.x, 0, gridSize);
        previousMousePos.y = Mathf.Clamp(previousMousePos.y, 0, gridSize);

        float particleCellSize = (gridSize) / (float)particleGridSize;

        for (int i = 0; i < particleGridSize; i++)
        {
            for (int j = 0; j < particleGridSize; j++)
            {


                particles[i + (j * particleGridSize)] = new Vector3(i + 1, j + 1) * particleCellSize;
                particles[i + (j * particleGridSize)].z = 1;

            }
        }
    }

    // Update is called once per frame
    void Update()
    {



        VelocityStep();

        ParticlesStep();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < particleGridSize; i++)
            {
                for (int j = 0; j < particleGridSize; j++)
                {
                    particleVelocities[i] = Vector3.zero;

                    float particleCellSize = gridSize / (float)particleGridSize;

                    particles[i + (j * particleGridSize)] = new Vector3(i + 1, j + 1) * particleCellSize;
                    particles[i + (j * particleGridSize)].z = 1;

                }
            }
        }

        if (drawGrid)
        {
            DrawGrid();
        }

        if (drawVelocities)
        {
            DrawVelocities();
        }

        if (drawParticles)
        {
            DrawParticles();
        }
        if (drawParticleMesh)
        {
            DrawParticleMesh();
        }
    }


    void VelocityStep()
    {

        prevVelocities = (Vector3[,,])velocities.Clone();
        //velocities = new Vector3[gridSize + 2, gridSize + 2, gridSize + 2];

        AddVelocitySources();



        if (diffuse)
        {

            DiffuseVelocities(ref velocities, prevVelocities, viscocity, timeStep);

        }
        if (project)
        {
            ProjectVelocities(ref velocities);
        }

        if (advect)
        {
            SwapGrids(ref velocities, ref prevVelocities);

            AdvectVelocities(ref velocities, prevVelocities, timeStep);

        }

        if (vorticity)
        {
            //            SwapGrids(ref velocities, ref prevVelocities);

            AddVorticity(vorticityMutiplier, ref velocities, prevVelocities);

        }

        if (project)
        {

            ProjectVelocities(ref velocities);

        }

    }

    void ParticlesStep()
    {
        AdvectParticleVelocities(ref velocities);

        MoveParticles(ref particles, ref particleVelocities);
    }




    void SwapGrids(ref Vector3[,,] a, ref Vector3[,,] b)
    {
        Vector3[,,] temp = (Vector3[,,])a.Clone();
        a = (Vector3[,,])b.Clone();
        b = (Vector3[,,])temp.Clone();
    }





    void DrawGrid()
    {
        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                for (int k = 0; k < gridSize + 2; k++)
                {
                    DrawCube((new Vector3(i, j, k) + Vector3.one / 2f) * cellSize, Vector3.one * cellSize / 2f, gridColor);
                }
            }
        }
    }
    void DrawVelocities()
    {
        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                for (int k = 1; k <= gridSize; k++)
                {
                    Debug.DrawRay(new Vector3(0.5f + i, 0.5f + j, 0.5f + k) * cellSize, Vector3.ClampMagnitude(velocities[i, j, k], cellSize), velocitiesColor);
                }
            }
        }
    }

    void DrawParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 particlePos = new Vector3(0.5f + particles[i].x, 0.5f + particles[i].y, 0.5f + particles[i].z) * cellSize;

            DrawCube(particlePos, Vector3.one * cellSize * 0.1f, particlesColor);
        }
    }
    void DrawParticleMesh()
    {
        int index = 0;
        Vector3 particlePos = Vector3.zero;
        Vector3 nextParticlePos = Vector3.zero;
        for (int i = 0; i < particleGridSize - 1; i++)
        {
            for (int j = 0; j < particleGridSize - 1; j++)
            {
                index = (i * particleGridSize) + j;
                particlePos = new Vector3(0.5f + particles[index].x, 0.5f + particles[index].y, 0.5f + particles[index].z) * cellSize;
                index = (i * particleGridSize) + j + 1;
                nextParticlePos = new Vector3(0.5f + particles[index].x, 0.5f + particles[index].y, 0.5f + particles[index].z) * cellSize;
                Debug.DrawLine(particlePos, nextParticlePos, particlesColor);


                index = (i * particleGridSize) + j;
                particlePos = new Vector3(0.5f + particles[index].x, 0.5f + particles[index].y, 0.5f + particles[index].z) * cellSize;
                index = ((i + 1) * particleGridSize) + j;
                nextParticlePos = new Vector3(0.5f + particles[index].x, 0.5f + particles[index].y, 0.5f + particles[index].z) * cellSize;

                Debug.DrawLine(particlePos, nextParticlePos, particlesColor);
            }
        }

        for (int i = 0; i < particleGridSize - 1; i++)
        {
            index = (particleGridSize * (particleGridSize - 1)) + i;
            particlePos = new Vector3(0.5f + particles[index].x, 0.5f + particles[index].y, 0.5f + particles[index].z) * cellSize;
            index = (particleGridSize * (particleGridSize - 1)) + i + 1;
            nextParticlePos = new Vector3(0.5f + particles[index].x, 0.5f + particles[index].y, 0.5f + particles[index].z) * cellSize;
            Debug.DrawLine(particlePos, nextParticlePos, particlesColor);



            index = (i * particleGridSize) + particleGridSize - 1;
            particlePos = new Vector3(0.5f + particles[index].x, 0.5f + particles[index].y, 0.5f + particles[index].z) * cellSize;
            index = ((i + 1) * particleGridSize) + particleGridSize - 1;
            nextParticlePos = new Vector3(0.5f + particles[index].x, 0.5f + particles[index].y, 0.5f + particles[index].z) * cellSize;

            Debug.DrawLine(particlePos, nextParticlePos, particlesColor);
        }



    }


    void DrawCube(Vector3 position, Vector3 scale, Color color)
    {
        DrawCubePoints(CubePoints(position, scale, Quaternion.identity), color);
    }

    Vector3[] CubePoints(Vector3 center, Vector3 extents, Quaternion rotation)
    {
        Vector3[] points = new Vector3[8];
        points[0] = rotation * Vector3.Scale(extents, new Vector3(1, 1, 1)) + center;
        points[1] = rotation * Vector3.Scale(extents, new Vector3(1, 1, -1)) + center;
        points[2] = rotation * Vector3.Scale(extents, new Vector3(1, -1, 1)) + center;
        points[3] = rotation * Vector3.Scale(extents, new Vector3(1, -1, -1)) + center;
        points[4] = rotation * Vector3.Scale(extents, new Vector3(-1, 1, 1)) + center;
        points[5] = rotation * Vector3.Scale(extents, new Vector3(-1, 1, -1)) + center;
        points[6] = rotation * Vector3.Scale(extents, new Vector3(-1, -1, 1)) + center;
        points[7] = rotation * Vector3.Scale(extents, new Vector3(-1, -1, -1)) + center;

        return points;
    }

    void DrawCubePoints(Vector3[] points, Color cubeColor)
    {
        Debug.DrawLine(points[0], points[1], cubeColor);
        Debug.DrawLine(points[0], points[2], cubeColor);
        Debug.DrawLine(points[0], points[4], cubeColor);
        Debug.DrawLine(points[7], points[6], cubeColor);
        Debug.DrawLine(points[7], points[5], cubeColor);
        Debug.DrawLine(points[7], points[3], cubeColor);

        Debug.DrawLine(points[1], points[3], cubeColor);
        Debug.DrawLine(points[1], points[5], cubeColor);
        Debug.DrawLine(points[2], points[3], cubeColor);
        Debug.DrawLine(points[2], points[6], cubeColor);

        Debug.DrawLine(points[4], points[5], cubeColor);
        Debug.DrawLine(points[4], points[6], cubeColor);
    }
    float3 TrilinearInterpolation(float3 position, Vector3[,,] array)
    {
        float x = position.x;
        float y = position.y;
        float z = position.z;

        x = Mathf.Clamp(x, 1, gridSize);
        y = Mathf.Clamp(y, 1, gridSize);
        z = Mathf.Clamp(z, 1, gridSize);

        int leftX = Mathf.FloorToInt(x);
        int rightX = leftX + 1;

        int bottomY = Mathf.FloorToInt(y);
        int topY = bottomY + 1;

        int backZ = Mathf.FloorToInt(z);
        int frontZ = backZ + 1;


        float leftDistance = x - leftX;
        float bottomDistance = y - bottomY;
        float backDistance = z - backZ;

        float3 bottomLeftVel = array[leftX, bottomY, backZ];
        float3 bottomRightVel = array[rightX, bottomY, backZ];
        float3 topLeftVel = array[leftX, topY, backZ];
        float3 topRightVel = array[rightX, topY, backZ];

        float3 bottomLeftFrontVel = array[leftX, bottomY, frontZ];
        float3 bottomRightFrontVel = array[rightX, bottomY, frontZ];
        float3 topLeftFrontVel = array[leftX, topY, frontZ];
        float3 topRightFrontVel = array[rightX, topY, frontZ];


        float3 leftVelLerp = math.lerp(bottomLeftVel, topLeftVel, bottomDistance);
        float3 rightVelLerp = math.lerp(bottomRightVel, topRightVel, bottomDistance);

        float3 leftFrontVelLerp = math.lerp(bottomLeftFrontVel, topLeftFrontVel, bottomDistance);
        float3 rightFrontVelLerp = math.lerp(bottomRightFrontVel, topRightFrontVel, bottomDistance);

        float3 backLerp = math.lerp(leftVelLerp, rightVelLerp, leftDistance);
        float3 frontLerp = math.lerp(leftFrontVelLerp, rightFrontVelLerp, leftDistance);

        return math.lerp(backLerp, frontLerp, backDistance);
    }
}
