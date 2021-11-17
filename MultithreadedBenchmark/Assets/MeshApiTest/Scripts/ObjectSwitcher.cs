using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSwitcher : MonoBehaviour
{
    public MeshAPITestBase[] objs;
    GUIStyle style;
    string headerInfo;
    public float switchRate = 3;

    private void Start() {
        style = new GUIStyle();
        style.fontSize = (int)(Screen.dpi/5);
        style.normal.textColor = Color.white;
        style.padding = new RectOffset(8, 8, 8, 8);
        headerInfo = string.Format("Vertices: {0} | Platform: {1} | CPU cores: {2} | CPU Frequency: {3}", objs[0].GetVertsCount(),  Application.platform, SystemInfo.processorCount, SystemInfo.processorFrequency );
    }

    // Update is called once per frame
    void Update()
    {
        float v = (Time.time % (switchRate * 2))/ (switchRate * 2);
        for (int i = 0; i<objs.Length; i++) {
            float m = 1.0f / (float)objs.Length;
            float f = i * m;
            float t = f + m;
            if (v >= f && v < t) {
                objs[i].gameObject.SetActive(true);
            } else {
                objs[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnGUI() {
        GUILayout.Label(headerInfo, style);
        for (int i = 0; i < objs.Length; i++) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(objs[i].scriptname, style, GUILayout.Width(260) );
            GUILayout.Label(objs[i].info, style);
            GUILayout.EndHorizontal();
        }
    }
}
