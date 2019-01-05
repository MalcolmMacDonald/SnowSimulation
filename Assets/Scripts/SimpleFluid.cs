using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFluid : MonoBehaviour
{
    const float gridScale = 1f;
    float cellSize;

    public int gridSize;
    public float timeStep = 0.1f;

    Vector2[,] velocities;
    Vector2[,] prevVelocities;
    Vector2[,] gradientField;
    Vector2[,] incompressibleField;
    float[,] densities;
    float[,] prevDensities;
    public bool drawGrid;
    public bool drawVelocities;
    public bool drawDensities;
    public bool drawGradient;
    public bool drawIncompressibleField;

    public int solverIterations;
    public float densitySourceRate;
    public float velocitySourceRate;
    public float diffusionRate;
    public float viscocity;
    private float halfrdx;


    public Color gridColor;
    public Color velocitiesColor;
    public Color densitiesColor;
    public Color gradientColor;
    public Color incompressibleColor;

    // Use this for initialization
    void Start()
    {
        cellSize = gridScale / (gridSize + 2);
        halfrdx = 0.5f / gridSize;
        velocities = new Vector2[gridSize + 2, gridSize + 2];
        prevVelocities = new Vector2[gridSize + 2, gridSize + 2];
        gradientField = new Vector2[gridSize, gridSize];
        incompressibleField = new Vector2[gridSize, gridSize];
        densities = new float[gridSize + 2, gridSize + 2];
        prevDensities = new float[gridSize + 2, gridSize + 2];


        for (int i = 0; i < gridSize + 2; i++)
        {


            for (int j = 0; j < gridSize + 2; j++)
            {


                // velocities[i, j] = Quaternion.AngleAxis(360 * (float)(i + j) / (gridSize + 2) + 90, Vector3.forward) * Vector2.up * velocitySourceRate * cellSize;
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
        if (drawGradient)
        {
            DrawGradient();
        }
        if (drawIncompressibleField)
        {

            DrawIncompressibleField();
        }
        SwapDensities();

    }

    void DensityStep()
    {
        AddDensitySources();

        DiffuseDensity();
        SetDensityBoundaries();
        SwapDensities();

        AdvectDensity();
        SetDensityBoundaries();
        SwapDensities();
    }

    void VelocityStep()
    {
        SwapVelocities();

        AddVelocitySources();
        DiffuseVelocities();
        SetVelocityBoundaries();


        AdvectVelocities();





        SubtractPressureGradient(computePressure(computeDivergence()));




    }

    float[,] computeDivergence()
    {
        float[,] divergence = new float[gridSize, gridSize];
        halfrdx = 0.5f / gridSize;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {

                float xGradient = ((GetPrevVelocity(i + 1, j).x - GetPrevVelocity(i - 1, j).x));
                float yGradient = ((GetPrevVelocity(i, j + 1).y - GetPrevVelocity(i, j - 1).y));
                divergence[i, j] = (xGradient + yGradient) * halfrdx;
                //DrawRect(new Vector2(i + 1, j + 1) * cellSize, Vector2.one * divergence[i, j], Color.green);
            }
        }

        return divergence;
    }

    float[,] computePressure(float[,] div)
    {
        float[,] prevPressure = new float[gridSize + 2, gridSize + 2];
        float a = -(cellSize * cellSize);
        float b = 1 / 4f;
        for (int k = 0; k < solverIterations; k++)
        {
            prevPressure = JacobiFloat(prevPressure, div, a, b);
        }


        return prevPressure;
    }
    float[,] JacobiFloat(float[,] x, float[,] b, float alpha, float rBeta)
    {
        float[,] output = new float[gridSize + 2, gridSize + 2];

        for (int i = 1; i < gridSize + 1; i++)
        {
            for (int j = 1; j < gridSize + 1; j++)
            {

                float thisPrev = b[i - 1, j - 1];
                float prev0 = x[i - 1, j];
                float prev1 = x[i + 1, j];
                float prev2 = x[i, j - 1];
                float prev3 = x[i, j + 1];
                output[i, j] = (thisPrev + prev0 + prev1 + prev2 + prev3) * rBeta;
                /*if (k == (solverIterations - 1))
                {
                    //    DrawRect(new Vector2(i, j) * cellSize, Vector2.one * pressure[i, j], Color.cyan);
                }*/
            }
        }

        return output;
    }
    Vector2[,] JacobiVector(Vector2[,] x, Vector2[,] b, Vector2 alpha, Vector2 rBeta)
    {
        Vector2[,] output = new Vector2[gridSize, gridSize];

        for (int i = 1; i < gridSize + 1; i++)
        {
            for (int j = 1; j < gridSize + 1; j++)
            {

                Vector2 thisPrev = b[i - 1, j - 1];
                Vector2 prev0 = x[i - 1, j];
                Vector2 prev1 = x[i + 1, j];
                Vector2 prev2 = x[i, j - 1];
                Vector2 prev3 = x[i, j + 1];
                output[i, j] = (thisPrev + prev0 + prev1 + prev2 + prev3) * rBeta;
                /*if (k == (solverIterations - 1))
                {
                    //    DrawRect(new Vector2(i, j) * cellSize, Vector2.one * pressure[i, j], Color.cyan);
                }*/
            }
        }

        return output;
    }

    void SubtractPressureGradient(float[,] pressure)
    {
        gradientField = new Vector2[gridSize, gridSize];
        for (int i = 1; i < gridSize + 1; i++)
        {
            for (int j = 1; j < gridSize + 1; j++)
            {

                float xGradient = pressure[i + 1, j] - pressure[i - 1, j];
                float yGradient = pressure[i, j + 1] - pressure[i, j - 1];
                gradientField[i - 1, j - 1] = new Vector2(xGradient, yGradient) * halfrdx;
            }
        }
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                SetVelocity(i, j, GetPrevVelocity(i, j) - gradientField[i, j]);
            }
        }
    }



    void SwapVelocities()
    {
        Vector2[,] temp = prevVelocities;
        prevVelocities = velocities;
        velocities = prevVelocities;
    }
    float timer;

    void AddVelocitySources()
    {
        Vector2 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePos *= gridSize;
        mousePos.x = Mathf.Clamp(mousePos.x, 0, gridSize);
        mousePos.y = Mathf.Clamp(mousePos.y, 0, gridSize);
        //  mousePos -= Vector2.up * 0.5f;


        Vector2 inputVelocity = Vector2.up * velocitySourceRate * cellSize;
        inputVelocity = Quaternion.AngleAxis(timer, Vector3.forward) * inputVelocity;
        if (Input.GetMouseButton(0))
        {

            SetVelocity((int)mousePos.x, (int)mousePos.y, GetVelocity((int)mousePos.x, (int)mousePos.y) + inputVelocity);
        }
        if (Input.GetMouseButton(1))
        {
            timer += Time.deltaTime * 50f;

        }



    }
    void DiffuseVelocities()
    {

    }



    void AdvectVelocities()
    {

        float dt0 = timeStep * cellSize;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector2 nextPos = new Vector2(i + 1, j + 1) - (dt0 * GetPrevVelocity(i, j));

                int leftX = Mathf.FloorToInt(nextPos.x);
                leftX = Mathf.Clamp(leftX, 0, gridSize);
                int rightX = leftX + 1;
                int bottomY = Mathf.FloorToInt(nextPos.y);
                bottomY = Mathf.Clamp(bottomY, 0, gridSize);
                int topY = bottomY + 1;


                Vector2 bottomLeft = new Vector2(leftX, bottomY) * cellSize + (Vector2.one * 0.5f * cellSize);
                Vector2 bottomRight = new Vector2(rightX, bottomY) * cellSize + (Vector2.one * 0.5f * cellSize);
                Vector2 topLeft = new Vector2(leftX, topY) * cellSize + (Vector2.one * 0.5f * cellSize);
                Vector2 topRight = new Vector2(rightX, topY) * cellSize + (Vector2.one * 0.5f * cellSize);


                float leftDistance = (nextPos.x - (float)leftX);
                float bottomDistance = (nextPos.y - (float)bottomY);

                Vector2 topLerp = topLerp = Vector2.Lerp(prevVelocities[leftX, topY], prevVelocities[rightX, topY], leftDistance);

                Vector2 bottomLerp = Vector2.Lerp(prevVelocities[leftX, bottomY], prevVelocities[rightX, bottomY], leftDistance);

                Vector2 finalVelocity = Vector2.Lerp(bottomLerp, topLerp, bottomDistance);

                SetVelocity(i, j, finalVelocity);
            }
        }
    }
    void SetVelocityBoundaries()
    {
        float boundaryStrength = velocitySourceRate * timeStep * 0.2f;
        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                if (i < 1)
                {
                    velocities[i, j] = Vector2.zero;
                }
                if (i > gridSize)
                {
                    velocities[i, j] = Vector2.zero;
                }
                if (j < 1)
                {
                    velocities[i, j] = Vector2.zero;
                }
                if (j > gridSize)
                {
                    velocities[i, j] = Vector2.zero;
                }

            }
        }
    }

    void SwapDensities()
    {
        float[,] temp = prevDensities;
        prevDensities = densities;
        densities = temp;
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
            SetDensity((int)mousePos.x, (int)mousePos.y, GetDensity((int)mousePos.x, (int)mousePos.y) + (densitySourceRate * Time.deltaTime));
        }
    }
    void DiffuseDensity()
    {

        float a = timeStep * diffusionRate * gridSize * gridSize;
        for (int k = 0; k < solverIterations; k++)
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {

                    float thisPrev = GetPrevDensity(i, j);
                    float prev0 = GetDensity(i - 1, j);
                    float prev1 = GetDensity(i + 1, j);
                    float prev2 = GetDensity(i, j - 1);
                    float prev3 = GetDensity(i, j + 1);

                    SetDensity(i, j, (thisPrev + a * (prev0 + prev1 + prev2 + prev3)) / (1 + 4 * a));
                }
            }
        }

    }
    void AdvectDensity()
    {

        float dt0 = timeStep * gridSize;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector2 nextPos = new Vector2(i + 1, j + 1) - (dt0 * GetVelocity(i, j));
                nextPos.x = Mathf.Clamp(nextPos.x, 0, gridSize - 1.5f);
                nextPos.y = Mathf.Clamp(nextPos.y, 0, gridSize - 1.5f);

                int leftX = (int)nextPos.x;
                leftX = Mathf.Clamp(leftX, 0, gridSize + 2);
                int rightX = leftX + 1;
                int bottomY = (int)nextPos.y;
                bottomY = Mathf.Clamp(bottomY, 0, gridSize + 2);
                int topY = bottomY + 1;

                float leftDistance = (nextPos.x - (float)leftX);
                float bottomDistance = (nextPos.y - (float)bottomY);

                float topLerp = Mathf.Lerp(prevDensities[leftX, topY], prevDensities[rightX, topY], leftDistance);
                float bottomLerp = Mathf.Lerp(prevDensities[leftX, bottomY], prevDensities[rightX, bottomY], leftDistance);

                SetDensity(i, j, Mathf.Lerp(bottomLerp, topLerp, bottomDistance));

            }
        }

    }
    void SetDensityBoundaries()
    {
        for (int i = 0; i < gridSize + 2; i++)
        {
            for (int j = 0; j < gridSize + 2; j++)
            {
                if ((i < 1 || i > gridSize) && (j < 1 || j > gridSize))
                {
                    densities[i, j] = 0;
                }
            }
        }
        for (int i = gridSize / 3; i < (gridSize * 2 / 3); i++)
        {
            for (int j = 0; j < gridSize / 3; j++)
            {


                //   SetDensity(i, j + gridSize / 3, 0);
            }
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
                Debug.DrawRay(new Vector2(0.5f + i, 0.5f + j) * cellSize, velocities[i, j], velocitiesColor);
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
    void DrawGradient()
    {

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Debug.DrawRay(new Vector2(1.5f + i, 1.5f + j) * cellSize, gradientField[i, j], gradientColor);
            }
        }
    }
    void DrawIncompressibleField()
    {

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Debug.DrawRay(new Vector2(1.5f + i, 1.5f + j) * cellSize, incompressibleField[i, j], incompressibleColor);
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

    Vector2 GetVelocity(int x, int y)
    {
        return velocities[x + 1, y + 1];
    }
    void SetVelocity(int x, int y, Vector2 newVelocity)
    {
        velocities[x + 1, y + 1] = newVelocity;
    }
    Vector2 GetPrevVelocity(int x, int y)
    {
        return prevVelocities[x + 1, y + 1];
    }
    void SetPrevVelocity(int x, int y, Vector2 newVelocity)
    {
        prevVelocities[x + 1, y + 1] = newVelocity;
    }
    float GetDensity(int x, int y)
    {
        return densities[x + 1, y + 1];
    }
    void SetDensity(int x, int y, float newDensity)
    {
        densities[x + 1, y + 1] = newDensity;
    }
    float GetPrevDensity(int x, int y)
    {
        return prevDensities[x + 1, y + 1];
    }
    void SetPrevDensity(int x, int y, float newDensity)
    {
        prevDensities[x + 1, y + 1] = newDensity;
    }

}
