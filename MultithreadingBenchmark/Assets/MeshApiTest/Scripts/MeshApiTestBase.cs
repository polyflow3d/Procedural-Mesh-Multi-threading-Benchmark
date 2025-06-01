using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class MeshAPITestBase : MonoBehaviour
{
    public float size = 2;
    Stopwatch updateSW;
    protected Stopwatch positionsw;
    protected Stopwatch normalssw;
    protected Stopwatch fillMeshSW;
    
    public Material mat;
    public float updateMs = 0;
    public float minMs = 0;
    public float maxMs = 1000;
    public float averageMs = 0;
    public float positionMs = 0;
    public float normalMs = 0;
    public float fillMeshMs = 0;
    protected int resolution = 220;
    protected Mesh mesh;

    int framesCounter = 0;

    protected int[] GetTriangles() {
        int quadsCount = resolution * resolution;
        int[] triangles = new int[quadsCount * 6];
        int tc = 0;

        for (int y = 0; y < resolution; y++) {
            for (int x = 0; x < resolution; x++) {
                int q0 = GetVertIdx(x, y);
                int q1 = GetVertIdx(x, y + 1);
                int q2 = GetVertIdx(x + 1, y + 1);
                int q3 = GetVertIdx(x + 1, y);
                triangles[tc + 0] = q0;
                triangles[tc + 1] = q1;
                triangles[tc + 2] = q2;
                triangles[tc + 3] = q0;
                triangles[tc + 4] = q2;
                triangles[tc + 5] = q3;
                tc += 6;
            }
        }
        return triangles;
    }

    protected int GetVertIdx(int x, int y) {
        x = Mathf.Clamp(x, 0, resolution);
        y = Mathf.Clamp(y, 0, resolution);
        return x + y * (resolution + 1);
    }

    public int GetVertsCount() {
        return (resolution + 1) * (resolution + 1);
    }

    private void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 p0 = new Vector3();
        Vector3 p1 = new Vector3(0, size,0);
        Vector3 p2 = new Vector3(size, size, 0);
        Vector3 p3 = new Vector3( size, 0, 0);

        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p0);
    }

    public virtual string scriptname  {
        get {
            return "not implemented";
        }
    }

    public virtual void onEnable() { 
    
    }

    public virtual void PositionJob() { 

    }

    public virtual void NormalsJob() {

    }

    public virtual void FillMesh() { 
    
    }

    private void OnEnable() {
        onEnable();
        updateSW = Stopwatch.StartNew();
        positionsw = Stopwatch.StartNew();
        normalssw = Stopwatch.StartNew();
        fillMeshSW = Stopwatch.StartNew();
        minMs = 1000;
        maxMs = 0;
        averageMs = -1;
        framesCounter = 0;
    }

    public bool warmedUp {
        get {
            return framesCounter > 3;
        }
    }

    private void Update() {
        updateSW.Start();

        positionsw.Start();
        PositionJob();
        positionsw.Stop();
        positionMs = positionsw.ElapsedTicks / (float)System.TimeSpan.TicksPerMillisecond;
        positionsw.Reset();

        normalssw.Start();
        NormalsJob();
        normalssw.Stop();
        normalMs = normalssw.ElapsedTicks / (float)System.TimeSpan.TicksPerMillisecond;
        normalssw.Reset();

        updateMs = updateSW.ElapsedTicks / (float)System.TimeSpan.TicksPerMillisecond;
        updateSW.Stop();
        updateSW.Reset();
        if (warmedUp) {
            minMs = Mathf.Min(minMs, updateMs);
            maxMs = Mathf.Max(maxMs, updateMs);
            if (averageMs < 0) {
                averageMs = updateMs;
            } else {
                averageMs = Mathf.Lerp(averageMs, updateMs, 0.5f);
            }
        }
        framesCounter++;
        fillMeshSW.Start();
        FillMesh();
        fillMeshSW.Stop();
        fillMeshMs = fillMeshSW.ElapsedTicks / (float)System.TimeSpan.TicksPerMillisecond;
        fillMeshSW.Reset();

        if (mesh != null) {
            mesh.bounds = GetBounds();
            if (mat != null) {
                Graphics.DrawMesh(mesh, transform.localToWorldMatrix, mat, 0);
            }
        }
    }

    public Bounds GetBounds() {
        Bounds result = new Bounds();
        result.extents = new Vector3(size, size,size);
        return result;  
    } 


   
}
