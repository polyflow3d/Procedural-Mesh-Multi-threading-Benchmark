 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;


public class BurstMeshApi : MeshAPITestBase {

    [BurstCompile(CompileSynchronously = true)]
    struct PositionJob : IJobParallelFor {
        public float mult;
        public float timer;
        NativeArray<Vector2Int> indices;
        NativeArray<Vector3> positions;
        NativeArray<Color> colors;

        public PositionJob(NativeArray<Vector2Int> indices, NativeArray<Vector3> positions, NativeArray<Color> colors) {
            this.indices = indices;
            this.positions = positions;
            this.colors = colors;
            this.mult = 1;
            this.timer = 0;
        }

        public void Execute(int n) {
            int x = indices[n].x;
            int y = indices[n].y;
            float nx = x * mult;
            float ny = y * mult;
            float z = Mathf.PerlinNoise(nx + timer, ny);
            Color col = Color.Lerp(Color.red, Color.blue, z);
            Vector3 pos = new Vector3(nx, ny, z);
            positions[n] = pos;
            colors[n] = col;
        }
    }
 
    [BurstCompile(CompileSynchronously = true)]
    struct NormalJob : IJobParallelFor {
        NativeArray<Vector2Int> indices;

        [NativeDisableParallelForRestriction]
        NativeArray<Vector3> positions;
        NativeArray<Vector3> normals;
        int resolution;

        public NormalJob(NativeArray<Vector2Int> indices, NativeArray<Vector3> positions, NativeArray<Vector3> normals, int resolution) {
            this.indices = indices;
            this.positions = positions;
            this.normals = normals;
            this.resolution = resolution;
        }

        int GetVertIdx(int x, int y) {
            x = Mathf.Clamp(x, 0, resolution);
            y = Mathf.Clamp(y, 0, resolution);
            return x + y * (resolution + 1);
        }

        public void Execute(int n) {
            int x = indices[n].x;
            int y = indices[n].y;
            int vi = n;
            int vitop = GetVertIdx(x, y + 1);
            int vibottom = GetVertIdx(x, y - 1);
            int vileft = GetVertIdx(x - 1, y);
            int viright = GetVertIdx(x + 1, y);
            Vector3 _vi = positions[vi];
            Vector3 _vitop = positions[vitop];
            Vector3 _vibottom = positions[vibottom];
            Vector3 _vileft = positions[vileft];
            Vector3 _viright = positions[viright];
            Vector3 c0 = Vector3.Cross(_vitop - _vi, _viright - _vi);
            Vector3 c1 = Vector3.Cross(_vibottom - _vi, _vileft - _vi);
            Vector3 norm = Vector3.LerpUnclamped(c0, c1, 0.5f);
            normals[vi] = norm;
        }
    }

    PositionJob positionJob;
    NormalJob normalJob;
    NativeArray<Vector2Int> indices;
    NativeArray<Vector3> positions;
    NativeArray<Vector3> normals;
    NativeArray<Color> colors;

    public void CreateArrays(int resolution) {
        int vertsCount = (resolution + 1) * (resolution + 1);
        indices = new NativeArray<Vector2Int>(vertsCount, Allocator.Persistent);
        int counter = 0;
        for (int y = 0; y <= resolution; y++) {
            for (int x = 0; x <= resolution; x++) {
                indices[counter] = new Vector2Int(x, y);
                counter++;
            }
        }

        positions = new NativeArray<Vector3>(vertsCount, Allocator.Persistent);
        normals = new NativeArray<Vector3>(vertsCount, Allocator.Persistent);
        colors = new NativeArray<Color>(vertsCount, Allocator.Persistent);
    }
 
    Mesh mesh;

    private void OnEnable() {
        CreateArrays(resolution);
        positionJob = new PositionJob(indices, positions, colors);
        normalJob = new NormalJob(indices, positions, normals, resolution);
        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = new Vector3[GetVertsCount()];
        mesh.triangles = GetTriangles();
 
        OnEnableBase();
    }

    private void Update() {
        UpdateBegin();
        UnityEngine.Profiling.Profiler.BeginSample("Parallel threaded");
        positionJob.mult = 1f / resolution * size;
        JobHandle jh = positionJob.Schedule(positions.Length, 10);
        jh.Complete();
        JobHandle jhn = normalJob.Schedule(normals.Length, 10);
        jhn.Complete();
        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
        mesh.bounds = GetBounds();
        UnityEngine.Profiling.Profiler.EndSample();
        if (mat) {
            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, mat, 0);
        }
        positionJob.timer += Time.deltaTime;
        UpdateEnd();
    }

    public override string scriptname {
        get {
            return "Burst Jobs";
        }
    }

    private void OnDisable() {
        indices.Dispose();
        positions.Dispose();
        normals.Dispose();
        colors.Dispose();
    }
 

}
