using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSwitcher : MonoBehaviour
{
    public MeshAPITestBase[] objs;
    GUIStyle stylePadLeft;
    GUIStyle styleCenter;
    string headerInfo;
    public float switchRate = 4;

    [Range(0.5f, 2f)]
    public float uiScale = 1f;

    private void Start() {
        stylePadLeft = new GUIStyle();
        stylePadLeft.fontSize = 10;
        stylePadLeft.normal.textColor = Color.white;
        stylePadLeft.padding = new RectOffset(8, 8, 8, 8);
        styleCenter = new GUIStyle(stylePadLeft);
        styleCenter.alignment = TextAnchor.MiddleCenter;
        headerInfo = string.Format("Vertices: {0}, Platform: {1}, CPU cores: {2}, CPU Frequency: {3}", objs[0].GetVertsCount(),  Application.platform, SystemInfo.processorCount, SystemInfo.processorFrequency );
    }

    // Update is called once per frame
    void Update()
    {
        int fontSize = (int)(Screen.width / 45 * uiScale);
        stylePadLeft.fontSize = fontSize;
        styleCenter.fontSize = fontSize;
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
        GUILayout.Label(headerInfo, stylePadLeft);
        DrawRow(" ", "Aver",  "Min",  "Max", "Pos", "Norm", "Fill");
        for (int i = 0; i < objs.Length; i++) {
            if (objs[i].warmedUp) {
                DrawRow(objs[i].scriptname, objs[i].averageMs.ToString("F2"), objs[i].minMs.ToString("F2"), objs[i].maxMs.ToString("F2"), objs[i].positionMs.ToString("F2"), objs[i].normalMs.ToString("F2"), objs[i].fillMeshMs.ToString("F2"));
            } else {
                DrawRow(objs[i].scriptname, "-", "-", "-", "-", "-", "-");
            }
        }
    }

    void DrawRow(string name, string aver, string min, string max, string pos, string norm, string fillMesh) {
        GUILayout.BeginHorizontal();

        float separatorw = Screen.width * 0.02f * uiScale;
        float columnw = Screen.width * 0.08f * uiScale;

        GUILayout.Label(name, stylePadLeft, GUILayout.Width(Screen.width * 0.27f * uiScale));

        GUILayout.Label("|", styleCenter, GUILayout.Width(separatorw));
        GUILayout.Label(aver, styleCenter, GUILayout.Width(columnw));

        GUILayout.Label("|", styleCenter, GUILayout.Width(separatorw));
        GUILayout.Label(min, styleCenter, GUILayout.Width(columnw));

        GUILayout.Label("|", styleCenter, GUILayout.Width(separatorw));
        GUILayout.Label(max, styleCenter, GUILayout.Width(columnw));

        GUILayout.Label("|", styleCenter, GUILayout.Width(separatorw));
        GUILayout.Label(pos, styleCenter, GUILayout.Width(columnw));

        GUILayout.Label("|", styleCenter, GUILayout.Width(separatorw));
        GUILayout.Label(norm, styleCenter, GUILayout.Width(columnw));

        GUILayout.Label("|", styleCenter, GUILayout.Width(separatorw));
        GUILayout.Label(fillMesh, styleCenter, GUILayout.Width(columnw));
        GUILayout.EndHorizontal();
    }
}
