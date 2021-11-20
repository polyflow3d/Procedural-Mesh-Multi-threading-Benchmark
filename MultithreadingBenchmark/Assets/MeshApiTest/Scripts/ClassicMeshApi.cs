using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;

[DefaultExecutionOrder(1)]
public class ClassicMeshApi : MeshAPITestBase {

    Vector3[] positions;
    Vector3[] normals;
    Color[] colors;
    float timer;

    public override void onEnable() {
        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        int _vertsCount = GetVertsCount();
        positions = new Vector3[_vertsCount];
        normals = new Vector3[_vertsCount];
        colors = new Color[_vertsCount];
        mesh.vertices = positions;
        mesh.triangles = GetTriangles();
    }

    public override void PositionJob() {
        float mult = 1f / resolution * size;
        //calc positions and color
        for (int y = 0; y <= resolution; y++) {
            for (int x = 0; x <= resolution; x++) {
                float nx = x * mult;
                float ny = y * mult;
                float z = Mathf.PerlinNoise(nx + timer, ny);
                int vidx = GetVertIdx(x, y);
                Color col = Color.Lerp(Color.red, Color.blue, z);
                Vector3 pos = new Vector3(nx, ny, z);
                positions[vidx] = pos;
                colors[vidx] = col;
            }
        }
        timer += Time.deltaTime;
    }

    public override void NormalsJob() {
        //calc normals 
        for (int y = 0; y <= resolution; y++) {
            for (int x = 0; x <= resolution; x++) {
                int vi = GetVertIdx(x, y);
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
    }
 
    public override void FillMesh() {
        mesh.vertices = positions;
        mesh.normals = normals;
        mesh.colors = colors;
    }

    public override string scriptname {
        get {
            return "Classic single thread ";
        }
    }

}
