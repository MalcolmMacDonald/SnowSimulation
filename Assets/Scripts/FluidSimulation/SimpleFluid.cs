using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;

public partial class SimpleFluid : MonoBehaviour
{
    Vector3 gridScale;
    [HideInInspector]
    public Vector3 cellSize;

    public int gridSizeX;
    public int gridSizeY;
    public int gridSizeZ;



    public float timeStep = 0.1f;


    Vector3[,,] velocities;
    Vector3[,,] prevVelocities;

    public bool drawGrid;
    public bool drawBoundingBox;
    public bool drawVelocities;
    public bool drawParticles;
    public bool drawParticleVelocities;


    public bool addObstacle;
    public bool resetParticlePositionAtBoundary;

    public int solverIterations;
    public float velocitySourceRate;
    public float viscocity;
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



        float maxValue = Mathf.Max(gridSizeX, gridSizeY, gridSizeZ);
        gridScale = new Vector3(gridSizeX / maxValue, gridSizeY / maxValue, gridSizeZ / maxValue);

        cellSize = new Vector3(gridScale.x / (gridSizeX + 2), gridScale.y / (gridSizeY + 2), gridScale.z / (gridSizeZ + 2));
        particleCount = particleGridSize * particleGridSize;
        particles = new Vector3[particleCount];
        particleVelocities = new Vector3[particleCount];
        velocities = new Vector3[gridSizeX + 2, gridSizeY + 2, gridSizeZ + 2];
        prevVelocities = new Vector3[gridSizeX + 2, gridSizeY + 2, gridSizeZ + 2];

        Vector2 previousMousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        previousMousePos.x = Mathf.Clamp(previousMousePos.x, 0, 1f) * gridSizeX;
        previousMousePos.y = Mathf.Clamp(previousMousePos.y, 0, 1f) * gridSizeY;

        Vector3 particleCellSize = new Vector3(gridSizeX / (float)particleGridSize, gridSizeY / (float)particleGridSize, gridSizeZ / (float)particleGridSize);

        for (int i = 0; i < particleGridSize; i++)
        {
            for (int j = 0; j < particleGridSize; j++)
            {
                particles[i + (j * particleGridSize)] = new Vector3(UnityEngine.Random.Range(1, gridSizeX + 1), UnityEngine.Random.Range(1, gridSizeY + 1), UnityEngine.Random.Range(1, gridSizeZ + 1));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {



        VelocityStep();

        ParticlesStep();

        if (drawGrid)
        {
            DrawGrid();
        }
        if (drawBoundingBox)
        {
            DrawBoundingBox();
        }

        if (drawVelocities)
        {
            DrawVelocities();
        }

        if (drawParticles)
        {
            DrawParticles();
        }
        if (drawParticleVelocities)
        {
            DrawParticleVelocities();
        }
    }


    void VelocityStep()
    {

        prevVelocities = (Vector3[,,])velocities.Clone();

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
        for (int i = 0; i < gridSizeX + 2; i++)
        {
            for (int j = 0; j < gridSizeY + 2; j++)
            {
                for (int k = 0; k < gridSizeZ + 2; k++)
                {
                    DrawCube(Vector3.Scale(new Vector3(i, j, k) + Vector3.one / 2f, cellSize), cellSize / 2f, gridColor);
                }
            }
        }
    }
    void DrawBoundingBox()
    {
        Vector3 boundsCenter = Vector3.Scale(cellSize, new Vector3(gridSizeX + 2, gridSizeY + 2, gridSizeZ + 2) / 2f);
        DrawCube(boundsCenter, boundsCenter, gridColor);
    }
    void DrawVelocities()
    {
        for (int i = 1; i <= gridSizeX; i++)
        {
            for (int j = 1; j <= gridSizeY; j++)
            {
                for (int k = 1; k <= gridSizeZ; k++)
                {
                    Vector3 clampedVelocity = velocities[i, j, k];
                    clampedVelocity.x = Mathf.Clamp(clampedVelocity.x, -cellSize.x, cellSize.x);
                    clampedVelocity.y = Mathf.Clamp(clampedVelocity.y, -cellSize.y, cellSize.y);
                    clampedVelocity.z = Mathf.Clamp(clampedVelocity.z, -cellSize.z, cellSize.z);

                    Debug.DrawRay(Vector3.Scale(new Vector3(0.5f + i, 0.5f + j, 0.5f + k), cellSize), clampedVelocity, velocitiesColor);
                }
            }
        }
    }

    void DrawParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 particlePos = Vector3.Scale(new Vector3(0.5f + particles[i].x, 0.5f + particles[i].y, 0.5f + particles[i].z), cellSize);

            DrawCube(particlePos, cellSize * 0.1f, particlesColor);
        }
    }
    void DrawParticleVelocities()
    {

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 remappedVelocity = Vector3.ClampMagnitude(particleVelocities[i] * timeStep, cellSize.magnitude);
            Debug.DrawRay(Vector3.Scale(particles[i], cellSize), remappedVelocity, particlesColor);
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

        x = Mathf.Clamp(x, 1, gridSizeX);
        y = Mathf.Clamp(y, 1, gridSizeY);
        z = Mathf.Clamp(z, 1, gridSizeZ);

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
