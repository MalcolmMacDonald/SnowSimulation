using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int solverIterations;
    public float densitySourceRate;
    public float velocitySourceRate;
    public float diffusionRate;
    public float viscocity;


    public Color gridColor;
    public Color velocitiesColor;
    public Color densitiesColor;

    // Use this for initialization
    void Start()
    {
        cellSize = gridScale / (gridSize + 2);

        densities = new float[gridSize + 2, gridSize + 2];
        prevDensities = new float[gridSize + 2, gridSize + 2];
        u = new float[gridSize + 2, gridSize + 2];
        v = new float[gridSize + 2, gridSize + 2];
        prevU = new float[gridSize + 2, gridSize + 2];
        prevV = new float[gridSize + 2, gridSize + 2];

        for (int i = 1; i <= gridSize; i++)
        {
            for (int j = 1; j <= gridSize; j++)
            {
                u[i, j] = 1;
                v[i, j] = 1;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        VelocityStep();
        DensityStep();

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

    }

    void DensityStep()
    {
        SwapGrids(ref densities, ref prevDensities);


        AddDensitySources();
        SwapGrids(ref densities, ref prevDensities);
        Diffuse(0, ref densities, ref prevDensities, diffusionRate, timeStep);
        SwapGrids(ref densities, ref prevDensities);
        Advect(0, ref densities, ref prevDensities, u, v, timeStep);

    }
    void VelocityStep()
    {

    }



    void SwapGrids(ref float[,] a, ref float[,] b)
    {
        float[,] temp = b;
        b = a;
        a = temp;
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
            //  SetDensity(, GetDensity((int)mousePos.x, (int)mousePos.y) + );
        }
    }



    void Diffuse(int b, ref float[,] x, ref float[,] x0, float diff, float dt)
    {

        float a = dt * diff * gridSize * gridSize;
        for (int k = 0; k < solverIterations; k++)
        {
            for (int i = 1; i <= gridSize; i++)
            {
                for (int j = 1; j <= gridSize; j++)
                {

                    float thisPrev = x0[i, j];
                    float prev0 = x[i - 1, j];
                    float prev1 = x[i + 1, j];
                    float prev2 = x[i, j - 1];
                    float prev3 = x[i, j + 1];


                    x[i, j] = (thisPrev + a * (prev0 + prev1 + prev2 + prev3)) / (1 + 4 * a);
                }
            }
            SetBoundaries(b, x);
        }

    }
    void Advect(int b, ref float[,] d, ref float[,] d0, float[,] u, float[,] vs, float dt)
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
                if (x < 0.5f)
                {
                    x = 0.5f;
                }
                if (x > gridSize + 0.5f)
                {
                    x = gridSize + 0.5f;
                }
                i0 = (int)x;
                i1 = i0 + 1;
                if (y < 0.5f)
                {
                    y = 0.5f;
                }
                if (y > gridSize + 0.5f)
                {
                    y = gridSize + 0.5f;
                }
                j0 = (int)y;
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
        SetBoundaries(b, d);


    }


    void SetBoundaries(int b, float[,] x)
    {
        for (int i = 0; i <= gridSize; i++)
        {

            x[0, i] = b == 1 ? -x[1, i] : x[1, i];
            x[gridSize + 1, i] = b == 1 ? -x[gridSize, i] : x[gridSize, i];
            x[i, 0] = b == 2 ? -x[i, 1] : x[i, 1];
            x[i, gridSize + 1] = b == 2 ? -x[i, gridSize] : x[i, gridSize];
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
                Debug.DrawRay(new Vector2(0.5f + i, 0.5f + j) * cellSize, new Vector2(u[i, j], v[i, j]), velocitiesColor);
            }
        }
    }
    void DrawDensities()
    {
        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                DrawRect(new Vector2(0.5f + i, 0.5f + j) * cellSize, (Vector2.up + Vector2.right) * densities[i, j] * cellSize, densitiesColor);
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
