using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Threading; 

public class ClassicThreadedMeshApi : MeshAPITestBase {

    public class TClass {
        private Thread thread;
        private ManualResetEventSlim mre_startpos;
        public ManualResetEventSlim mre_donepos;
        private ManualResetEventSlim mre_startnorm;
        public ManualResetEventSlim mre_donenorm;

        public bool alive;
        int arrIdxFrom;
        int arrIdxTo;

        Vector3[] positions;
        Vector3[] normals;
        Color[] colors;
        Vector2Int[] indices;
        float timer;
        float mult;
        int resolution;
 
 

        public TClass(int idx, int tcount, Vector2Int[] indices, Vector3[] positions, Vector3[] normals, Color[] colors, int resolution) {
            this.indices = indices;
            this.positions = positions;
            this.normals = normals;
            this.colors = colors;
            this.resolution = resolution;
            alive = true;
            mre_startpos = new ManualResetEventSlim(false);
            mre_donepos = new ManualResetEventSlim(false);
            mre_startnorm = new ManualResetEventSlim(false);
            mre_donenorm = new ManualResetEventSlim(false);

            int countPerThread = indices.Length / tcount;
            arrIdxFrom = idx * countPerThread;
            arrIdxTo = arrIdxFrom + countPerThread;
            if (idx == tcount - 1) {
                arrIdxTo = indices.Length;
            }

            thread = new Thread(PositionJob);
            thread.Priority = System.Threading.ThreadPriority.Highest;
            thread.Start();
        }

        public void StartLoop(float timer) {
            this.timer = timer;
        }

        public void StartPositionJob(float timer, float mult) {
            this.timer = timer;
            this.mult = mult;
            mre_donepos.Reset();
            mre_startpos.Set();
        }

        void PositionJob() {
            while (alive) {

                mre_startpos.Wait();
                for (int i = arrIdxFrom; i < arrIdxTo; i++) {
                    int x = indices[i].x;
                    int y = indices[i].y;
                    float nx = x * mult;
                    float ny = y * mult;
                    float z = Mathf.PerlinNoise(nx + timer, ny);
                    Color col = Color.Lerp(Color.red, Color.blue, z);
                    Vector3 pos = new Vector3(nx, ny, z);
                    positions[i] = pos;
                    colors[i] = col;
                }
                mre_donepos.Set();

                mre_startnorm.Wait();
                for (int i = arrIdxFrom; i < arrIdxTo; i++) {
                    int x = indices[i].x;
                    int y = indices[i].y;
                    int vi = i;
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
                mre_donenorm.Set();

                mre_startpos.Reset();
                mre_startnorm.Reset();
            }
        }

        public void StartNormalsJob() {
            mre_donenorm.Reset();
            mre_startnorm.Set();
        }

        int GetVertIdx(int x, int y) {
            x = Mathf.Clamp(x, 0, resolution);
            y = Mathf.Clamp(y, 0, resolution);
            return x + y * (resolution + 1);
        }

        public void OnDisable() {
            alive = false;
            mre_startpos.Set();
            mre_startnorm.Set();
        }
    }

    public int threadsCount = 4;
    Vector3[] positions;
    Vector3[] normals;
    Color[] colors;
    Vector2Int[] indices;
    float timer;
    TClass[] threads;


    public override void onEnable() {
        threadsCount = System.Environment.ProcessorCount;
        //Mathf.Max(threadsCount, 1);
        int vertsCount = (resolution + 1) * (resolution + 1);
        indices = new Vector2Int[vertsCount];
        int counter = 0;
        for (int y = 0; y <= resolution; y++) {
            for (int x = 0; x <= resolution; x++) {
                indices[counter] = new Vector2Int(x, y);
                counter++;
            }
        }

        positions = new Vector3[vertsCount];
        normals = new Vector3[vertsCount];
        colors = new Color[vertsCount];

        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        int _vertsCount = GetVertsCount();
        positions = new Vector3[_vertsCount];
        normals = new Vector3[_vertsCount];
        colors = new Color[_vertsCount];
        mesh.vertices = positions;
        mesh.triangles = GetTriangles();

        threads = new TClass[threadsCount];
        for (int t = 0; t < threads.Length; t++) {
            threads[t] = new TClass(t, threadsCount, indices, positions, normals, colors, resolution);
        }
    }

    public override void PositionJob() {
        float mult = 1f / resolution * size;

        for (int t = 0; t < threads.Length; t++) {
            threads[t].StartPositionJob(timer, 1f / resolution * size);
        }

        for (int t = 0; t < threads.Length; t++) {
            threads[t].mre_donepos.Wait();
        }
        timer += Time.deltaTime;
    }

    public override void NormalsJob() {

        for (int t = 0; t < threads.Length; t++) {
            threads[t].StartNormalsJob();
        }

        for (int t = 0; t < threads.Length; t++) {
            threads[t].mre_donenorm.Wait();
        }
    }

    public override void FillMesh() {
        mesh.SetVertices(positions);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
    }


    private void OnDisable() {
        for (int t = 0; t < threads.Length; t++) {
            threads[t].OnDisable();
        }
    }

    public override string scriptname {
        get {
            return "Classic Multithreaded";
        }
    }

}
