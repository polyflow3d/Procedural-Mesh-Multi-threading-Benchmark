using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class MeshAPITestBase : MonoBehaviour
{
    public float size = 2;
    Stopwatch sw;
    public Material mat;
    protected float minMs;
    protected float maxMs;
    protected float averageMs;

    protected int resolution = 220;

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

    float ms;
    public string info;

    public virtual string scriptname  {
        get {
            return "not implemented";
        }
    }

    protected void OnEnableBase() {
        sw = Stopwatch.StartNew();
        minMs = float.MaxValue;
        maxMs = float.MinValue;
        averageMs = -1;
    }

    protected void UpdateBegin() {
        sw.Start();
    }

    protected void UpdateEnd() {
        ms = sw.ElapsedTicks / (float)System.TimeSpan.TicksPerMillisecond;
        sw.Stop();
        sw.Reset();
        minMs = Mathf.Min(minMs, ms);
        maxMs = Mathf.Max(minMs, ms);
        if (averageMs < 0) {
            averageMs = ms;
        } else {
            averageMs = Mathf.Lerp(averageMs, ms, 0.5f);
        }
        info = string.Format("current:{0}   min:{1}   max:{2}   average:{3}",  ms.ToString("F2"), minMs.ToString("F2"), maxMs.ToString("F2"), averageMs.ToString("F2")) ;
    }

    public Bounds GetBounds() {
        Bounds result = new Bounds();
        result.extents = new Vector3(size, size,size);
        return result;  
    } 
}
