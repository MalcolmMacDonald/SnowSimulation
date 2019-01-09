using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;

public class SimpleFluid : MonoBehaviour
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
    public float densitySourceRate;
    public float velocitySourceRate;
    public float diffusionRate;
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

        float particleCellSize = gridSize / particleGridSize;
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


        Diffuse(1, ref velocities, prevVelocities, viscocity, timeStep);






        Project(ref velocities);

        SwapGrids(ref velocities, ref prevVelocities);




        Advect(1, ref velocities, prevVelocities, timeStep);

        SwapGrids(ref velocities, ref prevVelocities);

        AddVorticity(vorticityMutiplier, ref velocities, prevVelocities);
        Project(ref velocities);


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


    void AddVelocitySources()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane simPlane = new Plane(Vector3.forward, 0);
        float rayDist = 0;
        simPlane.Raycast(mouseRay, out rayDist);
        Vector2 mousePos = mouseRay.origin + mouseRay.direction * rayDist;

        mousePos *= gridSize;

        mousePos.x = Mathf.Clamp(mousePos.x, 0, gridSize);
        mousePos.y = Mathf.Clamp(mousePos.y, 0, gridSize);
        mousePos += Vector2.one * 0.5f;
        Vector2 currentDir = (mousePos - previousMousePos) * velocitySourceRate;
        if (Input.GetMouseButton(0))
        {
            velocities[(int)mousePos.x, (int)mousePos.y, 1] = currentDir;
        }

        previousMousePos = mousePos;

        /*     for (int i = 0; i < gridSize; i++)
            {
                v[i, 1] = Random.Range(0, velocitySourceRate);
            }
    */
    }



    void Diffuse(int b, ref Vector3[,,] x, Vector3[,,] x0, float diff, float dt)
    {

        float a = dt * diff * gridSize * gridSize;
        Vector3[,,] xCopy = x;
        for (int q = 0; q < solverIterations; q++)
        {

            for (int i = 1; i <= gridSize; i++)
            {
                for (int j = 1; j <= gridSize; j++)
                {

                    Vector2 thisPrev = x0[i, j, 1];
                    Vector2 prev0 = xCopy[i - 1, j, 1];
                    Vector2 prev1 = xCopy[i + 1, j, 1];
                    Vector2 prev2 = xCopy[i, j - 1, 1];
                    Vector2 prev3 = xCopy[i, j + 1, 1];


                    x[i, j, 1] = (thisPrev + a * (prev0 + prev1 + prev2 + prev3)) / (1 + 4 * a);
                }

            }
            SetBoundaries(b, ref x);
        }

    }
    void Advect(int b, ref Vector3[,,] d, Vector3[,,] d0, float dt)
    {

        float dt0 = dt * gridSize;

        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                for (int k = 1; k <= gridSize; k++)
                {

                    Vector3 newPos = dt0 * d[i, j, 1];
                    d[i, j, 1] = TrilinearInterpolation(newPos, d0);
                }
            }
        }
        SetBoundaries(b, ref d);


    }
    void Project(ref Vector3[,,] u)
    {
        int i, j, k;
        float h;
        h = 1f / gridSize;
        float[,] pressure = new float[gridSize + 2, gridSize + 2];
        float[,] divergence = new float[gridSize + 2, gridSize + 2];
        for (i = 1; i <= gridSize; i++)
        {
            for (j = 1; j <= gridSize; j++)
            {
                divergence[i, j] = -0.5f * h * (u[i + 1, j, 1].x - u[i - 1, j, 1].x + u[i, j + 1, 1].y - u[i, j - 1, 1].y);
            }
        }

        // SetBoundaries(0, ref divergence);
        //  SetBoundaries(0, ref pressure); //write new float boundaries function because these have to stay as floats

        float[,] pCopy = pressure;
        for (k = 0; k < solverIterations; k++)
        {
            for (i = 1; i <= gridSize; i++)
            {
                for (j = 1; j <= gridSize; j++)
                {
                    pressure[i, j] = (divergence[i, j] + pCopy[i - 1, j] + pCopy[i + 1, j] + pCopy[i, j - 1] + pCopy[i, j + 1]) * 0.25f;
                }
            }
            // SetBoundaries(0, ref pressure); //this is correct, just need to rewerite SetBoundareies
        }

        for (i = 1; i <= gridSize; i++)
        {
            for (j = 1; j <= gridSize; j++)
            {
                u[i, j, 1].x -= 0.5f * gridSize * (pressure[i + 1, j] - pressure[i - 1, j]);
                u[i, j, 1].y -= 0.5f * gridSize * (pressure[i, j + 1] - pressure[i, j - 1]);
            }
        }
        SetBoundaries(1, ref u);
    }
    void AddVorticity(float multiplier, ref Vector3[,,] u, Vector3[,,] prevU)
    {

        Vector3[,,] curl = new Vector3[gridSize + 2, gridSize + 2, gridSize + 2];
        float h = 1f / gridSize;


        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                curl[i, j, 0] = 0.5f * new Vector3((prevU[i + 1, j, 1].x - prevU[i - 1, j, 1].x), prevU[i, j + 1, 1].y - prevU[i, j - 1, 1].y);
            }
        }


        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {

                float dwdx = (curl[i + 1, j, 1].x - curl[i - 1, j, 1].x) * 0.5f;
                float dwdy = (curl[i, j + 1, 1].y - curl[i, j - 1, 1].y) * 0.5f;

                Vector3 normCurl = new Vector3(dwdx, dwdy).normalized * multiplier;
                //  Vector2 gradient = 0.5f * new Vector2(curl[i + 1, j].magnitude - curl[i - 1, j].magnitude, +curl[i, j + 1].magnitude - curl[i, j - 1].magnitude).normalized;

                //   Vector2 multipliedVorticity = Vector3.Cross(curl[i, j], gradient) * h * vorticityMutiplier;
                if (drawVorticity)
                {
                    //      Debug.DrawRay(new Vector2(0.5f + i, 0.5f + j) * cellSize, Vector2.ClampMagnitude(multipliedVorticity, cellSize), Color.yellow);
                }
                u[i, j, 1] += normCurl;
                //    v[i, j] += multipliedVorticity.y;
            }
        }
    }


    void SetBoundaries(int b, ref Vector3[,,] x)
    {
        for (int i = 0; i <= gridSize; i++)
        {

            /*  x[0, i] = b == 1 ? -x[1, i] : x[1, i];
             x[gridSize + 1, i] = b == 1 ? -x[gridSize, i] : x[gridSize, i];
             x[i, 0] = b == 2 ? -x[i, 1] : x[i, 1];
             x[i, gridSize + 1] = b == 2 ? -x[i, gridSize] : x[i, gridSize];
 */


            x[0, i, 1] = Vector2.zero;
            x[gridSize + 1, i, 1] = Vector2.zero;
            x[i, 0, 1] = Vector2.zero;
            x[i, gridSize + 1, 1] = Vector2.zero;

        }

        if (addObstacle)
        {
            for (int i = gridSize / 3; i < (2 * gridSize / 3); i++)
            {
                for (int j = gridSize / 3; j < (2 * gridSize / 3); j++)
                {
                    x[i, j, 1] = Vector2.zero;
                }
            }
        }
        /*     x[0, 0, 0] = 0.5f * (x[1, 0, 0] + x[0, 1, 0]);
            x[0, gridSize + 1, 0] = 0.5f * (x[1, gridSize + 1, 0] + x[0, gridSize, 0]);
            x[gridSize + 1, 0, 0] = 0.5f * (x[gridSize, 0, 0] + x[gridSize + 1, 1, 0]);
            x[gridSize + 1, gridSize + 1, 0] = 0.5f * (x[gridSize, gridSize + 1, 0] + x[gridSize + 1, gridSize, 0]);
    */
    }

    void AdvectParticleVelocities(ref Vector3[,,] v)
    {
        for (int i = 0; i < particleCount; i++)
        {


            particleVelocities[i] = TrilinearInterpolation(particles[i], v);
        }

    }
    void MoveParticles(ref Vector3[] p, ref Vector3[] v)
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 newPos = p[i] + v[i] * timeStep * particleVelocity;



            if (resetParticlePositionAtBoundary)
            {
                if (newPos.x < 1 || newPos.x > gridSize || newPos.y < 1 || newPos.y > gridSize)
                {
                    v[i] = Vector3.zero;
                    newPos = new Vector3(UnityEngine.Random.Range(1, gridSize), 1, 1);
                }
                if (addObstacle)
                {
                    if ((particles[i].x >= (gridSize / 3) - 1 && particles[i].x <= 2 * gridSize / 3 + 1) && (particles[i].y >= gridSize / 3 - 1 && particles[i].y <= 2 * gridSize / 3 + 1))
                    {
                        newPos = new Vector3(UnityEngine.Random.Range(1, gridSize), 1, 1);
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
                    Debug.DrawRay(new Vector3(0.5f + i, 0.5f + j, 0.5f + k) * cellSize, Vector2.ClampMagnitude(velocities[i, j, k], cellSize), velocitiesColor);
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
