using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SimpleFluid : MonoBehaviour
{
    const float gridScale = 1f;
    float cellSize;

    public int gridSize;
    public float timeStep = 0.1f;


    float[,] densities;
    float[,] prevDensities;

    float[,] u;
    float[,] v;
    float[,] prevU;
    float[,] prevV;

    public bool drawGrid;
    public bool drawVelocities;
    public bool drawDensities;
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
    public Color densitiesColor;
    public Color particlesColor;

    float timer;
    Vector2 previousMousePos;


    Vector2[] particles;
    Vector2[] particleVelocities;
    public int particleGridSize;
    int particleCount;


    // Use this for initialization
    void Start()
    {
        cellSize = gridScale / (gridSize + 2);
        particleCount = particleGridSize * particleGridSize;
        particles = new Vector2[particleCount];
        particleVelocities = new Vector2[particleCount];
        densities = new float[gridSize + 2, gridSize + 2];
        prevDensities = new float[gridSize + 2, gridSize + 2];
        u = new float[gridSize + 2, gridSize + 2];
        v = new float[gridSize + 2, gridSize + 2];
        prevU = new float[gridSize + 2, gridSize + 2];
        prevV = new float[gridSize + 2, gridSize + 2];


        Vector2 previousMousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition) * gridSize;
        previousMousePos.x = Mathf.Clamp(previousMousePos.x, 0, gridSize);
        previousMousePos.y = Mathf.Clamp(previousMousePos.y, 0, gridSize);

        float particleCellSize = gridSize / particleGridSize;
        for (int i = 0; i < particleGridSize; i++)
        {
            for (int j = 0; j < particleGridSize; j++)
            {
                particles[j + (i * particleGridSize)] = ((Vector2.one / 2f) + new Vector2(i, j)) * particleCellSize;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {


        VelocityStep();

        //  DensityStep();

        ParticlesStep();


        if (drawGrid)
        {
            DrawGrid();
        }
        if (drawVelocities)
        {
            DrawVelocities();
        }
        if (drawDensities)
        {
            DrawDensities();
        }
        if (drawParticles)
        {
            DrawParticles();

        }

    }

    void DensityStep()
    {
        AddDensitySources();


        SwapGrids(ref densities, ref prevDensities);
        Diffuse(0, ref densities, prevDensities, diffusionRate, timeStep);
        SwapGrids(ref densities, ref prevDensities);
        Advect(0, ref densities, prevDensities, u, v, timeStep);

    }
    void VelocityStep()
    {

        AddVelocitySources();


        SwapGrids(ref u, ref prevU);
        SwapGrids(ref v, ref prevV);


        Diffuse(1, ref u, prevU, viscocity, timeStep);
        Diffuse(2, ref v, prevV, viscocity, timeStep);






        Project(ref u, ref v, ref prevU, ref prevV);

        SwapGrids(ref u, ref prevU);
        SwapGrids(ref v, ref prevV);




        Advect(1, ref u, prevU, prevU, prevV, timeStep);
        Advect(2, ref v, prevV, prevU, prevV, timeStep);

        SwapGrids(ref u, ref prevU);
        SwapGrids(ref v, ref prevV);
        AddVorticity(vorticityMutiplier, ref u, ref v, prevU, prevV);
        Project(ref u, ref v, ref prevU, ref prevV);


        //       



    }

    void ParticlesStep()
    {
        for (int i = 0; i < particleCount; i++)
        {
            float x = particles[i].x;
            float y = particles[i].y;
            if (x < 0.5f)
            {
                x = 0.5f;
            }
            if (x > gridSize + 0.5f)
            {
                x = gridSize + 0.5f;
            }
            int i0 = Mathf.RoundToInt(x);
            int i1 = i0 + 1;
            if (y < 0.5f)
            {
                y = 0.5f;
            }
            if (y > gridSize + 0.5f)
            {
                y = gridSize + 0.5f;
            }
            int j0 = Mathf.RoundToInt(y);
            int j1 = j0 + 1;

            float leftDistance = x - i0;
            float rightDistance = 1 - leftDistance;
            float bottomDistance = y - j0;
            float topDistance = 1 - bottomDistance;

            Vector2 bottomLeftVel = new Vector2(u[i0, j0], v[i0, j0]);
            Vector2 bottomRightVel = new Vector2(u[i1, j0], v[i1, j0]);
            Vector2 topLeftVel = new Vector2(u[i0, j1], v[i0, j1]);
            Vector2 topRightVel = new Vector2(u[i0, j1], v[i0, j1]);

            Vector2 leftVelLerp = rightDistance * (topDistance * bottomLeftVel + bottomDistance * topLeftVel);
            Vector2 rightVelLerp = leftDistance * (topDistance * bottomRightVel + bottomDistance * topRightVel);

            float bottomLeft = densities[i0, j0];
            float bottomRight = densities[i1, j0];
            float topLeft = densities[i0, j1];
            float topRight = densities[i0, j1];

            float leftLerp = rightDistance * (topDistance * bottomLeft + bottomDistance * topLeft);
            float rightLerp = leftDistance * (topDistance * bottomRight + bottomDistance * topRight);


            Vector2 newVelocity = leftVelLerp + rightVelLerp;

            /*  if ((particles[i].x > gridSize / 3 && particles[i].x < 2 * gridSize / 3) && (particles[i].y > gridSize / 3 && particles[i].y < 2 * gridSize / 3))
             {
                 newVelocity = Vector2.zero;
             }*/


            //            newVelocity = newVelocity.normalized * (newVelocity.magnitude + (leftLerp + rightLerp));
            //   newVelocity = newVelocity.normalized * Mathf.Clamp(newVelocity.magnitude - (leftLerp + rightLerp), 0, newVelocity.magnitude);

            particleVelocities[i] = newVelocity;
        }
        for (int i = 0; i < particleCount; i++)
        {
            Vector2 newPos = particles[i] + particleVelocities[i] * timeStep * particleVelocity;



            if (resetParticlePositionAtBoundary)
            {
                if (newPos.x < 1 || newPos.x > gridSize || newPos.y < 1 || newPos.y > gridSize)
                {
                    particleVelocities[i] = Vector2.zero;
                    newPos = new Vector2(Random.Range(1, gridSize), 1);
                }
                if (addObstacle)
                {
                    if ((particles[i].x >= (gridSize / 3) - 1 && particles[i].x <= 2 * gridSize / 3 + 1) && (particles[i].y >= gridSize / 3 - 1 && particles[i].y <= 2 * gridSize / 3 + 1))
                    {
                        newPos = new Vector2(Random.Range(1, gridSize), 1);
                        particleVelocities[i] = Vector2.zero;
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




    void SwapGrids(ref float[,] a, ref float[,] b)
    {
        float[,] temp = a;
        a = b;
        b = temp;
    }

    void AddDensitySources()
    {

        Vector2 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePos *= gridSize;
        mousePos.x = Mathf.Clamp(mousePos.x, 0, gridSize);
        mousePos.y = Mathf.Clamp(mousePos.y, 0, gridSize);
        mousePos += Vector2.one * 0.5f;
        if (Input.GetMouseButton(0))
        {
            densities[(int)mousePos.x, (int)mousePos.y] += (densitySourceRate * timeStep);
        }


        /*   for (int i = gridSize / 3; i < (2 * gridSize / 3); i++)
          {
              densities[i, 1] = densitySourceRate * timeStep;
          }*/

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
            u[(int)mousePos.x, (int)mousePos.y] = currentDir.x;
            v[(int)mousePos.x, (int)mousePos.y] = currentDir.y;
        }

        previousMousePos = mousePos;

        /*     for (int i = 0; i < gridSize; i++)
            {
                v[i, 1] = Random.Range(0, velocitySourceRate);
            }
    */
    }



    void Diffuse(int b, ref float[,] x, float[,] x0, float diff, float dt)
    {

        float a = dt * diff * gridSize * gridSize;
        float[,] xCopy = x;
        for (int k = 0; k < solverIterations; k++)
        {
            for (int i = 1; i <= gridSize; i++)
            {
                for (int j = 1; j <= gridSize; j++)
                {

                    float thisPrev = x0[i, j];
                    float prev0 = xCopy[i - 1, j];
                    float prev1 = xCopy[i + 1, j];
                    float prev2 = xCopy[i, j - 1];
                    float prev3 = xCopy[i, j + 1];


                    x[i, j] = (thisPrev + a * (prev0 + prev1 + prev2 + prev3)) / (1 + 4 * a);
                }
            }
            SetBoundaries(b, ref x);
        }

    }
    void Advect(int b, ref float[,] d, float[,] d0, float[,] u, float[,] v, float dt)
    {

        int i, j, i0, j0, i1, j1;
        float x, y, rightDistance, topDistance, leftDistance, bottomDistance, dt0;
        dt0 = dt * gridSize;

        for (i = 1; i <= gridSize; i++)
        {
            for (j = 1; j <= gridSize; j++)
            {
                x = i - (dt0 * u[i, j]);
                y = j - (dt0 * v[i, j]);
                if (x < 1.5f)
                {
                    x = 1.5f;
                }
                if (x > gridSize - 0.5f)
                {
                    x = gridSize - 0.5f;
                }
                i0 = Mathf.RoundToInt(x);
                i1 = i0 + 1;
                if (y < 1.5f)
                {
                    y = 1.5f;
                }
                if (y > gridSize - 0.5f)
                {
                    y = gridSize - 0.5f;
                }
                j0 = Mathf.RoundToInt(y);
                j1 = j0 + 1;

                leftDistance = x - i0;
                rightDistance = 1 - leftDistance;
                bottomDistance = y - j0;
                topDistance = 1 - bottomDistance;

                float bottomLeft = d0[i0, j0];
                float bottomRight = d0[i1, j0];
                float topLeft = d0[i0, j1];
                float topRight = d0[i1, j1];

                float leftLerp = rightDistance * (topDistance * bottomLeft + bottomDistance * topLeft);
                float rightLerp = leftDistance * (topDistance * bottomRight + bottomDistance * topRight);

                d[i, j] = leftLerp + rightLerp;
            }
        }
        SetBoundaries(b, ref d);


    }
    void Project(ref float[,] u, ref float[,] v, ref float[,] p, ref float[,] div)
    {
        int i, j, k;
        float h;
        h = 1f / gridSize;
        for (i = 1; i <= gridSize; i++)
        {
            for (j = 1; j <= gridSize; j++)
            {
                div[i, j] = -0.5f * h * (u[i + 1, j] - u[i - 1, j] + v[i, j + 1] - v[i, j - 1]);
                p[i, j] = 0;
            }
        }

        SetBoundaries(0, ref div);
        SetBoundaries(0, ref p);

        float[,] pCopy = p;
        for (k = 0; k < solverIterations; k++)
        {
            for (i = 1; i <= gridSize; i++)
            {
                for (j = 1; j <= gridSize; j++)
                {
                    p[i, j] = (div[i, j] + pCopy[i - 1, j] + pCopy[i + 1, j] + pCopy[i, j - 1] + pCopy[i, j + 1]) * 0.25f;
                }
            }
            SetBoundaries(0, ref p);
        }

        for (i = 1; i <= gridSize; i++)
        {
            for (j = 1; j <= gridSize; j++)
            {
                u[i, j] -= 0.5f * gridSize * (p[i + 1, j] - p[i - 1, j]);
                v[i, j] -= 0.5f * gridSize * (p[i, j + 1] - p[i, j - 1]);
            }
        }
        SetBoundaries(1, ref u);
        SetBoundaries(2, ref v);

    }
    void AddVorticity(float multiplier, ref float[,] u, ref float[,] v, float[,] u0, float[,] v0)
    {

        Vector2[,] curl = new Vector2[gridSize + 2, gridSize + 2];
        float h = 1f / gridSize;


        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                curl[i, j] = 0.5f * new Vector2((u0[i + 1, j] - u0[i - 1, j]), v0[i, j + 1] - v0[i, j - 1]);
            }
        }


        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {

                float dwdx = (curl[i + 1, j].x - curl[i - 1, j].x) * 0.5f;
                float dwdy = (curl[i, j + 1].y - curl[i, j - 1].y) * 0.5f;

                Vector2 normCurl = new Vector2(dwdx, dwdy).normalized * multiplier;
                //  Vector2 gradient = 0.5f * new Vector2(curl[i + 1, j].magnitude - curl[i - 1, j].magnitude, +curl[i, j + 1].magnitude - curl[i, j - 1].magnitude).normalized;

                //   Vector2 multipliedVorticity = Vector3.Cross(curl[i, j], gradient) * h * vorticityMutiplier;
                if (drawVorticity)
                {
                    //      Debug.DrawRay(new Vector2(0.5f + i, 0.5f + j) * cellSize, Vector2.ClampMagnitude(multipliedVorticity, cellSize), Color.yellow);
                }
                u[i, j] += normCurl.x;
                u[i, j] += normCurl.y;
                //    v[i, j] += multipliedVorticity.y;
            }
        }
    }


    void SetBoundaries(int b, ref float[,] x)
    {
        for (int i = 0; i <= gridSize; i++)
        {

            /*  x[0, i] = b == 1 ? -x[1, i] : x[1, i];
             x[gridSize + 1, i] = b == 1 ? -x[gridSize, i] : x[gridSize, i];
             x[i, 0] = b == 2 ? -x[i, 1] : x[i, 1];
             x[i, gridSize + 1] = b == 2 ? -x[i, gridSize] : x[i, gridSize];
              */
            x[0, i] = 0;
            x[gridSize + 1, i] = 0;
            x[i, 0] = 0;
            x[i, gridSize + 1] = 0;

        }

        if (addObstacle)
        {
            for (int i = gridSize / 3; i < (2 * gridSize / 3); i++)
            {
                for (int j = gridSize / 3; j < (2 * gridSize / 3); j++)
                {
                    x[i, j] = 0;
                }
            }
        }
        x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
        x[0, gridSize + 1] = 0.5f * (x[1, gridSize + 1] + x[0, gridSize]);
        x[gridSize + 1, 0] = 0.5f * (x[gridSize, 0] + x[gridSize + 1, 1]);
        x[gridSize + 1, gridSize + 1] = 0.5f * (x[gridSize, gridSize + 1] + x[gridSize + 1, gridSize]);

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
                Debug.DrawRay(new Vector2(0.5f + i, 0.5f + j) * cellSize, Vector2.ClampMagnitude(new Vector2(u[i, j], v[i, j]), cellSize), velocitiesColor);
            }
        }
    }
    void DrawDensities()
    {

        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                DrawRect(new Vector2(0.5f + i, 0.5f + j) * cellSize, (Vector2.up + Vector2.right) * Mathf.Clamp(densities[i, j], 0, cellSize), densitiesColor);
            }
        }
    }
    void DrawParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector2 particlePos = new Vector2(0.5f + particles[i].x, 0.5f + particles[i].y) * cellSize;

            if (!Physics.Linecast(Camera.main.transform.position, particlePos))
            {
                DrawRect(particlePos, (Vector2.up + Vector2.right) * cellSize * 0.1f, particlesColor);
            }
        }
    }


    void DrawRect(Vector2 position, Vector2 scale, Color color)
    {
        Vector2 topLeft = position + Vector2.Scale(scale, new Vector2(-1, 1));
        Vector2 topRight = position + Vector2.Scale(scale, Vector2.one);
        Vector2 bottomLeft = position + Vector2.Scale(scale, -Vector2.one);
        Vector2 bottomRight = position + Vector2.Scale(scale, new Vector2(1, -1));

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);

    }


}
