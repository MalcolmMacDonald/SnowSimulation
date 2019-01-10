using System.Collections;
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

        int particleCellSize = Mathf.FloorToInt(gridSize / (float)particleGridSize);

        for (int i = 0; i < particleGridSize; i++)
        {
            for (int j = 0; j < particleGridSize; j++)
            {
                particles[j + (i * particleGridSize)] = ((Vector3)(Vector2.one / 2f) + new Vector3(i, j)) * particleCellSize;
                particles[j + (i * particleGridSize)].z = 1;

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
        if (drawVelocities)
        {
            DrawVelocities();
        }

        if (drawParticles)
        {
            DrawParticles();

        }

    }


    void VelocityStep()
    {

        AddVelocitySources();


        SwapGrids(ref velocities, ref prevVelocities);





        DiffuseVelocities(1, ref velocities, prevVelocities, viscocity, timeStep);






        ProjectVelocities(ref velocities);

        SwapGrids(ref velocities, ref prevVelocities);




        AdvectVelocities(1, ref velocities, prevVelocities, timeStep);

        SwapGrids(ref velocities, ref prevVelocities);

        AddVorticity(vorticityMutiplier, ref velocities, prevVelocities);
        ProjectVelocities(ref velocities);


    }

    void ParticlesStep()
    {
        AdvectParticleVelocities(ref velocities);
        MoveParticles(ref particles, ref particleVelocities);
    }




    void SwapGrids(ref Vector3[,,] a, ref Vector3[,,] b)
    {
        Vector3[,,] temp = a;
        a = b;
        b = temp;
    }









    void DrawGrid()
    {
        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                Debug.DrawRay(new Vector3(i, j) * cellSize, Vector3.right * cellSize, gridColor);
                Debug.DrawRay(new Vector3(i, j) * cellSize, Vector3.up * cellSize, gridColor);

            }
        }
        Debug.DrawRay(new Vector3(gridSize + 2, 0) * cellSize, Vector3.up * (gridSize + 2) * cellSize, gridColor);
        Debug.DrawRay(new Vector3(0, gridSize + 2) * cellSize, Vector3.right * (gridSize + 2) * cellSize, gridColor);

    }
    void DrawVelocities()
    {
        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                for (int k = 0; k < gridSize + 2; k++)
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

            //  if (!Physics.Linecast(Camera.main.transform.position, particlePos))
            {
                DrawCube(particlePos, Vector3.one * cellSize * 0.1f, particlesColor);
            }
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

        x = Mathf.Clamp(x, 0.5f, gridSize - 0.5f);
        y = Mathf.Clamp(y, 0.5f, gridSize - 0.5f);
        z = Mathf.Clamp(z, 0.5f, gridSize - 0.5f);

        int leftX = Mathf.RoundToInt(x);
        int rightX = leftX + 1;

        int bottomY = Mathf.RoundToInt(y);
        int topY = bottomY + 1;

        int backZ = Mathf.RoundToInt(z);
        int fromtZ = backZ + 1;


        float leftDistance = x - leftX;
        float bottomDistance = y - bottomY;
        float backDistance = z - backZ;

        float3 bottomLeftVel = velocities[leftX, bottomY, backZ];
        float3 bottomRightVel = velocities[rightX, bottomY, backZ];
        float3 topLeftVel = velocities[leftX, topY, backZ];
        float3 topRightVel = velocities[leftX, topY, backZ];

        float3 bottomLeftFrontVel = velocities[leftX, bottomY, fromtZ];
        float3 bottomRightFrontVel = velocities[rightX, bottomY, fromtZ];
        float3 topLeftFrontVel = velocities[leftX, topY, fromtZ];
        float3 topRightFrontVel = velocities[leftX, topY, fromtZ];



        float3 leftVelLerp = math.lerp(bottomLeftVel, topLeftVel, bottomDistance);
        float3 rightVelLerp = math.lerp(bottomRightVel, topRightVel, bottomDistance);

        float3 leftFrontVelLerp = math.lerp(bottomLeftFrontVel, topLeftFrontVel, bottomDistance);
        float3 rightFrontVelLerp = math.lerp(bottomRightFrontVel, topRightFrontVel, bottomDistance);

        float3 backLerp = math.lerp(leftVelLerp, rightVelLerp, leftDistance);
        float3 frontLerp = math.lerp(leftFrontVelLerp, rightFrontVelLerp, leftDistance);

        return math.lerp(backLerp, frontLerp, backDistance);
    }
}
