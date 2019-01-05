using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Jobs;
using System.Runtime.InteropServices;
using Unity.Collections;
using System.Linq;
using Unity.Mathematics;

public class SnowflakeManager : MonoBehaviour
{
    public static Vector3 offset;

    public int snowflakeCount = 1023;
    public int batchCount = 60;


    public Material snowflakeMaterial;

    public bool displayRect;
    public bool drawGrid;
    public bool showGridDebugLines;




    private NativeArray<float3> velocities;
    private NativeArray<float3> positions;
    NativeArray<float3> vectorGrid;
    const int xSize = 25;
    const int ySize = 14;
    const int zSize = 15;
    public static float scale;
    static float deltaTime;

    Bounds worldBounds;
    JobHandle respawnJob;

    NativeArray<Matrix4x4> renderMatrices;


    public static Vector3 snowflakeScale = Vector3.one / 30;

    public static Quaternion snowflakeRotation = Quaternion.identity;

    MeshRenderer[] buildingMeshes;

    Texture2D[] buildingTextures;
    public Material snowBuildupMaterial;

    Mesh quadMesh;
    Matrix4x4[] drawnMatricesArray;

    JobHandle matrixJob;
    public static Vector3Int[] gridNeighbors;
    public static int inverseCellSize = 2;

    public Transform debugObject;
    //  JobHandle matrixJob;
    IEnumerator Start()
    {
        quadMesh = new Mesh();
        quadMesh.vertices = new Vector3[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
        quadMesh.triangles = new int[] { 0, 1, 2, 2, 3, 0, 3, 2, 0, 0, 2, 1 };
        quadMesh.uv = new Vector2[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };

        inverseCellSize = 1;
        yield return new WaitForSeconds(0.5f);

        positions = new NativeArray<float3>(snowflakeCount * batchCount, Allocator.Persistent);
        velocities = new NativeArray<float3>(snowflakeCount * batchCount, Allocator.Persistent);
        renderMatrices = new NativeArray<Matrix4x4>(snowflakeCount * batchCount, Allocator.Persistent);


        vectorGrid = new NativeArray<float3>(xSize * ySize * zSize * inverseCellSize * inverseCellSize * inverseCellSize, Allocator.Persistent);
        scale = 1;
        offset = transform.position;
        Collider[] hitColliders = new Collider[4];
        for (int i = 0; i < vectorGrid.Length; i++)
        {
            Vector3 createdVector = Vector3.zero;
            Vector3 centerPos = ((Vector3)ArrayIndex(i) * scale) + transform.position;
            int hitAmount = Physics.OverlapBoxNonAlloc(centerPos, Vector3.one * scale, hitColliders);
            createdVector = Vector3.down;
            for (int a = 0; a < hitAmount; a++)
            {
                createdVector -= (hitColliders[a].ClosestPointOnBounds(centerPos) - centerPos);
            }
            if (hitAmount < 1)
            {
                createdVector += -Vector3.right;
            }
            // createdVector = Vector3.ClampMagnitude(createdVector, 0.25f);
            vectorGrid[i] = createdVector;
        }
        Vector3 size = new Vector3(xSize, ySize, zSize) * scale;
        worldBounds = new Bounds(transform.position + size / 2, size);

        drawnMatricesArray = new Matrix4x4[snowflakeCount];

        gridNeighbors = new Vector3Int[6];
        bool inX;
        bool inY;
        bool inZ;
        int lerpPos;
        for (int i = 0; i < 6; i++)
        {
            inX = i < 2;
            inY = i > 1 && i < 4;
            inZ = i > 3;
            lerpPos = (int)(((float)(i % 2) - 0.5f) * 2f);
            gridNeighbors[i] = new Vector3Int(inX ? lerpPos : 0, inY ? lerpPos : 0, inZ ? lerpPos : 0);
        }
        //-1, 0, 0
        // 1, 0, 0
        // 0,-1, 0
        // 0, 1, 0
        // 0, 0,-1
        // 0, 0, 1


        for (int i = 0; i < snowflakeCount * batchCount; i++)
        {
            Respawn(i);
        }
    }


    private void Respawn(int i)
    {
        Vector3 randomOnPlane = new Vector3();
        // randomOnPlane.x = transform.position.x + ( Random.Range(0, 1f) * (xSize - 1)) * scale;
        // randomOnPlane.z = transform.position.z + (Random.Range(0, 1f) * (zSize - 1)) * scale;
        randomOnPlane.y = transform.position.y + 14;
        positions[i] = randomOnPlane;
        velocities[i] = Vector3.down;
        renderMatrices[i] = Matrix4x4.identity;
    }

    private void Update()
    {
        if (!positions.IsCreated)
        {
            return;
        }

        deltaTime = Time.deltaTime;

        var addWindForcesJob = new AddWindForcesJob()
        {
            velocities = velocities,
            positions = positions,
            forceField = vectorGrid
        };

        var windForceJob = addWindForcesJob.Schedule(snowflakeCount * batchCount, batchCount);



        var raycastCommands = new NativeArray<RaycastCommand>(snowflakeCount * batchCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var raycastHits = new NativeArray<RaycastHit>(snowflakeCount * batchCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var checkCollisionsJob = new PrepareRaycastCommands()
        {
            Velocities = velocities,
            Positions = positions,
            Raycasts = raycastCommands
        };

        var collisionJob = checkCollisionsJob.Schedule(snowflakeCount * batchCount, batchCount, windForceJob);

        collisionJob.Complete();

        var raycastJob = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, batchCount, collisionJob);

        raycastJob.Complete();





        var addVelocitiesJob = new AddVelocitiesJob()
        {
            velocities = velocities,
            positions = positions
        };

        var velocitiesJob = addVelocitiesJob.Schedule(snowflakeCount * batchCount, batchCount, raycastJob);

        var checkRespawnsJob = new CheckRespawnJob()
        {
            position = transform.position,
            velocities = velocities,
            positions = positions,
            renderMatrices = renderMatrices,
            worldBounds = worldBounds,
            raycastHits = raycastHits
        };

        respawnJob = checkRespawnsJob.Schedule(snowflakeCount * batchCount, batchCount, velocitiesJob);

        //  respawnJob.Complete();


        RenderSnowflakes();




        Profiler.BeginSample("Check for respawns");


        /*for (int i = 0; i < snowflakeCount * batchCount; i++)
        {
            if (!worldBounds.Contains(positions[i]) || raycastHits[i].normal != Vector3.zero)
            {
                Respawn(i);
            }
        }*/

        Profiler.EndSample();

        raycastHits.Dispose();
        raycastCommands.Dispose();


    }

    void RenderSnowflakes()
    {
        if (!renderMatrices.IsCreated)
        {
            return;
        }
        var renderMatrixJob = new CalculateRenderMatricesJob()
        {
            positions = positions,
            velocities = velocities,
            renderMatrices = renderMatrices
        };

        matrixJob = renderMatrixJob.Schedule(snowflakeCount * batchCount, batchCount, respawnJob);


        matrixJob.Complete();


        DrawSnowflakes();
    }


    void DrawSnowflakes()
    {
        Profiler.BeginSample("Draw matrices");
        Matrix4x4[] matricesArray = renderMatrices.ToArray();
        for (int i = 0; i < batchCount; i++)
        {
            System.Array.Copy(matricesArray, i * snowflakeCount, drawnMatricesArray, 0, snowflakeCount);
            Graphics.DrawMeshInstanced(quadMesh, 0, snowflakeMaterial, drawnMatricesArray);
        }
        Profiler.EndSample();
    }


    private void OnApplicationQuit()
    {
        matrixJob.Complete();
        positions.Dispose();
        velocities.Dispose();
        vectorGrid.Dispose();
        renderMatrices.Dispose();
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && displayRect)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
        }

        if (vectorGrid.Length < 1)
        {
            return;
        }

        if (showGridDebugLines)
        {
            Gizmos.color = Color.red;
            Vector3 roundedWorldPos = (debugObject.position) / scale; //WorldToGridPos(debugObject.position);
            Vector3Int offsetPos;
            float distanceRight = InverseLerp(Vector3Int.RoundToInt(roundedWorldPos), Vector3Int.RoundToInt(roundedWorldPos) + gridNeighbors[0], roundedWorldPos);
            Vector3 lerpedVector = Vector3.Lerp(GetVector(Vector3Int.RoundToInt(roundedWorldPos), vectorGrid), GetVector(Vector3Int.RoundToInt(roundedWorldPos) + gridNeighbors[0], vectorGrid), distanceRight);
            Gizmos.DrawRay(Vector3Int.RoundToInt(roundedWorldPos), lerpedVector);

            for (int i = 0; i < 6; i++)
            {

                offsetPos = Vector3Int.RoundToInt(roundedWorldPos) + gridNeighbors[i]; // Vector3Int.RoundToInt(roundedWorldPos + new Vector3(0, 0.5f, 0)) + gridNeighbors[i] - Vector3.up / 2;
                Gizmos.DrawWireCube(offsetPos, Vector3.one / 25f);                                                        // Gizmos.color = Color.HSVToRGB((ArrayIndex(offsetPos - Vector3Int.RoundToInt(offset)) % 3) / 3f, 1, 1);
                Gizmos.DrawRay(offsetPos, GetVector(offsetPos, vectorGrid));                                    //   Gizmos.DrawSphere(offsetPos, 0.03f);
            }
        }

        if (drawGrid)
        {
            Gizmos.color = Color.white;
            Vector3Int rayPos = new Vector3Int();
            for (int x = 0; x < xSize * inverseCellSize; x++)
            {
                rayPos.x = x;
                for (int y = 0; y < ySize * inverseCellSize; y++)
                {
                    rayPos.y = y;
                    for (int z = 0; z < zSize * inverseCellSize; z++)
                    {
                        rayPos.z = z;
                        Gizmos.DrawWireCube((Vector3)rayPos / inverseCellSize * scale, Vector3.one / (inverseCellSize));
                        if (math.all(vectorGrid[ArrayIndex(rayPos)] != new float3(0, 0, 0)))
                        {
                            Gizmos.DrawWireCube((Vector3)rayPos * scale + (Vector3)vectorGrid[ArrayIndex(rayPos)], Vector3.one / 30f);
                            Gizmos.DrawRay((Vector3)rayPos * scale, vectorGrid[ArrayIndex(rayPos)]);
                        }
                    }
                }
            }
        }

    }

    public struct CalculateRenderMatricesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> positions;
        [ReadOnly]
        public NativeArray<float3> velocities;

        public NativeArray<Matrix4x4> renderMatrices;

        public void Execute(int i)
        {
            renderMatrices[i] = Matrix4x4.TRS(positions[i] + velocities[i] * deltaTime, Quaternion.LookRotation(Vector3.forward, velocities[i]), SnowflakeManager.snowflakeScale);
        }
    }


    public struct AddWindForcesJob : IJobParallelFor
    {

        public NativeArray<float3> velocities;
        [ReadOnly]
        public NativeArray<float3> positions;
        [ReadOnly]
        public NativeArray<float3> forceField;


        public void Execute(int i)
        {
            velocities[i] += GetVector(positions[i], forceField) * deltaTime;
        }




    }

    public struct AddVelocitiesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> velocities;
        public NativeArray<float3> positions;
        public void Execute(int i)
        {
            positions[i] += velocities[i] * deltaTime;

        }
    }
    public struct PrepareRaycastCommands : IJobParallelFor
    {

        public NativeArray<RaycastCommand> Raycasts;
        [ReadOnly]
        public NativeArray<float3> Velocities;
        [ReadOnly]
        public NativeArray<float3> Positions;


        public void Execute(int i)
        {
            // figure out how far the object we are testing collision for
            // wants to move in total. Our collision raycast only needs to be
            // that far.
            float distance = math.length(Velocities[i]) * deltaTime;
            Raycasts[i] = new RaycastCommand(Positions[i], Velocities[i], distance);
        }
    }
    // [ComputeJobOptimization]
    public struct CheckRespawnJob : IJobParallelFor
    {
        [ReadOnly]
        public float3 position;

        public NativeArray<float3> positions;
        public NativeArray<float3> velocities;
        public NativeArray<Matrix4x4> renderMatrices;

        [ReadOnly]
        public Bounds worldBounds;
        [ReadOnly]
        public NativeArray<RaycastHit> raycastHits;



        public void Execute(int i)
        {

            if (!worldBounds.Contains(positions[i]) || raycastHits[i].normal != Vector3.zero)
            {
                // float a = Random.value;
                positions[i] = new float3(position.x + ((positions[i].z * 50f) % (xSize - 1)) * scale, position.y + 14, position.z + ((math.abs(positions[i].x) * 50f) % (zSize - 1)) * scale);//(math..Range(0, 1f) * (xSize - 1)) * scale, position.z + (Random.Range(0, 1f) * (zSize - 1)) * scale, position.y + 14);
                velocities[i] = Vector3.down;
                renderMatrices[i] = Matrix4x4.identity;
            }
        }

    }
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
    public static float3 GetVector(Vector3 worldPos, NativeArray<float3> forceField)
    {
        Vector3Int roundedWorldPos = Vector3Int.RoundToInt((worldPos - offset) / scale);
        return GetGridVector(roundedWorldPos, forceField);
    }
    public static float3 GetGridVector(Vector3Int pos, NativeArray<float3> field)
    {
        int index = ArrayIndex(pos);
        if (index < 0 || index >= field.Length)
        {
            return Vector3.zero;
        }

        return field[index];
    }
    public static int ArrayIndex(Vector3Int input)
    {
        return (input.x * inverseCellSize) + (input.y * xSize * inverseCellSize) + (input.z * xSize * ySize * inverseCellSize);
    }
    public static Vector3Int ArrayIndex(int input)
    {
        int xPos = input % (xSize * inverseCellSize);
        int yPos = Mathf.FloorToInt((float)input / (xSize * inverseCellSize)) % (ySize * inverseCellSize);
        int zPos = Mathf.FloorToInt((float)input / (xSize * ySize * inverseCellSize));
        return new Vector3Int(xPos, yPos, zPos);
    }


}
