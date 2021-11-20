using static Unity.Mathematics.math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Burst.CompilerServices;

public class BurstMathematicApi : MeshAPITestBase {

    [BurstCompile(CompileSynchronously = true)]
    struct P_ositionJob : IJobParallelFor {
        public float mult;
        public float timer;
        NativeArray<int2> indices;
        NativeArray<Vertex> vertices;
 
        public P_ositionJob(NativeArray<int2> indices, NativeArray<Vertex> vertices ) {
            this.indices = indices;
            this.vertices = vertices;
            this.mult = 1;
            this.timer = 0;
        }

        public void Execute(int n) {
            int x = indices[n].x;
            int y = indices[n].y;
            float nx = x * mult;
            float ny = y * mult;
            Vertex v = vertices[n];
            float z = Mathf.PerlinNoise(nx + timer, ny);
            v.color = lerp(float4(1,0,0,1), float4(0, 0, 1, 1), z);
            v.position= new float3(nx, ny, z);
            vertices[n] = v; 
        }
    }
 
    [BurstCompile(CompileSynchronously = true)]
    struct N_ormalJob : IJobParallelFor {
        NativeArray<int2> indices;

        [NativeDisableParallelForRestriction]
        NativeArray<Vertex> vertices;
 
        readonly int resolution;

        public N_ormalJob(NativeArray<int2> indices, NativeArray<Vertex> vertices,  int resolution) {
            this.indices = indices;
            this.vertices = vertices;
            this.resolution = resolution;
        }

        int GetVertIdx(int x, int y) {
            x = clamp(x, 0, resolution);
            y = clamp(y, 0, resolution);
            return x + y * (resolution + 1);
        }

        public void Execute(int n) {  
            int x = indices[n].x;
            int y = indices[n].y;
            int vi = n;
            Vertex vert = vertices[vi];
            int vitop = GetVertIdx(x, y + 1);
            int vibottom = GetVertIdx(x, y - 1);
            int vileft = GetVertIdx(x - 1, y);
            int viright = GetVertIdx(x + 1, y);
            float3 _vi = vertices[vi].position;
            float3 _vitop = vertices[vitop].position;
            float3 _vibottom = vertices[vibottom].position;
            float3 _vileft = vertices[vileft].position;
            float3 _viright = vertices[viright].position;
            float3 c0 = cross(_vitop - _vi, _viright - _vi);
            float3 c1 = cross(_vibottom - _vi, _vileft - _vi);
            float3 norm = lerp(c0, c1, 0.5f);
            vert.normal = norm;
            vertices[vi] = vert;
        }
    }

    P_ositionJob positionJob;
    N_ormalJob normalJob;

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct Vertex {
        public float3 position;
        public float3 normal;
        public float4 color;
    }

    NativeArray<Vertex> vertices;
    [ReadOnly]
    NativeArray<int2> indices;

    public void CreateArrays(int resolution) {
        int vertsCount = (resolution + 1) * (resolution + 1);
        vertices = new NativeArray<Vertex>(vertsCount, Allocator.Persistent);
        indices = new NativeArray<int2>(vertsCount, Allocator.Persistent);
 
        int counter = 0;
        for (int y = 0; y <= resolution; y++) {
            for (int x = 0; x <= resolution; x++) {
                indices[counter] = int2(x, y);
                counter++;
            }
        }
    }
 

    public override  void onEnable() {
        CreateArrays(resolution);
        int verticesCount = GetVertsCount();
        mesh = new Mesh();
  
        mesh.vertices = new Vector3[verticesCount];
        mesh.SetIndexBufferParams(verticesCount, IndexFormat.UInt32);
        mesh.triangles = GetTriangles(); 
        VertexAttributeDescriptor[] vad = new VertexAttributeDescriptor[3];
        vad[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, stream:0);
        vad[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, stream: 0);
        vad[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, stream: 0);
        mesh.SetVertexBufferParams(verticesCount, vad );
        positionJob = new P_ositionJob(indices, vertices);
        normalJob = new N_ormalJob(indices,   vertices, resolution);
 
    }

    public override void PositionJob() {
        positionJob.mult = 1f / resolution * size;
        JobHandle jh = positionJob.Schedule(vertices.Length, 100);
        jh.Complete();
        positionJob.timer += Time.deltaTime;
    }

    public override void NormalsJob() {
        JobHandle jhn = normalJob.Schedule(vertices.Length, 100);
        jhn.Complete();
    }

    public override void FillMesh() {
        mesh.SetVertexBufferData<Vertex>(vertices, 0, 0, vertices.Length, stream: 0, UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds);
    }

    private void OnDisable() {
        indices.Dispose();
        vertices.Dispose();
    }

    public override string scriptname {
        get {
            return "Burst Jobs + Mathematics";
        }
    }

}
