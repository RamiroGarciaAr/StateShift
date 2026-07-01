using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponInertiaConfigSO))]
public class WeaponInertiaConfigSOEditor : Editor
{
    private const float PREVIEW_HEIGHT = 100f;
    private const int SAMPLE_COUNT = 200;

    private static readonly Color k_BgColor = new Color(0.11f, 0.11f, 0.11f);
    private static readonly Color k_CurveColor = new Color(0.28f, 0.88f, 0.42f);
    private static readonly Color k_TargetColor = new Color(1f, 1f, 1f, 0.15f);
    private static readonly Color k_ZeroColor = new Color(1f, 1f, 1f, 0.05f);

    private static readonly float[] s_Samples = new float[SAMPLE_COUNT];
    private static readonly Vector3[] s_Points = new Vector3[SAMPLE_COUNT];

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        WeaponInertiaConfigSO config = (WeaponInertiaConfigSO)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Live Dynamics Preview (Step Response)", EditorStyles.boldLabel);

        DrawGraph("Roll Dynamics", config.RollF, config.RollZ, config.RollR);

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawGraph(string label, float f, float z, float r)
    {
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        Rect rect = GUILayoutUtility.GetRect(10, PREVIEW_HEIGHT);
        
        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect, k_BgColor);

        // Simulation
        float duration = 4.0f / Mathf.Max(f, 0.1f);
        float dt = duration / SAMPLE_COUNT;

        SecondOrderDynamics sim = new SecondOrderDynamics(f, z, r, Vector3.zero);
        Vector3 targetPos = Vector3.one;

        float minVal = 0f;
        float maxVal = 1f;

        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            float val = sim.Update(dt, targetPos).x;
            s_Samples[i] = val;
            
            if (val < minVal) minVal = val;
            if (val > maxVal) maxVal = val;
        }

        float range = maxVal - minVal;
        float padding = range * 0.2f;
        float viewMin = minVal - padding;
        float viewMax = maxVal + padding;
        float viewRange = Mathf.Max(viewMax - viewMin, 0.001f);

        float Normalize(float v) => 1f - (v - viewMin) / viewRange;

        Handles.color = k_ZeroColor;
        float zeroY = rect.y + rect.height * Normalize(0f);
        Handles.DrawLine(new Vector3(rect.x, zeroY, 0), new Vector3(rect.xMax, zeroY, 0));

        Handles.color = k_TargetColor;
        float targetY = rect.y + rect.height * Normalize(1f);
        Handles.DrawLine(new Vector3(rect.x, targetY, 0), new Vector3(rect.xMax, targetY, 0));

        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            float t = i / (float)(SAMPLE_COUNT - 1);
            float normY = Normalize(s_Samples[i]);
            s_Points[i] = new Vector3(
                rect.x + t * rect.width,
                rect.y + rect.height * normY,
                0
            );
        }

        Handles.color = k_CurveColor;
        Handles.DrawPolyLine(s_Points);
        
        string info = $"f: {f:F1} | z: {z:F2} | r: {r:F2} (Duration: {duration:F2}s)";
        GUI.Label(new Rect(rect.x + 5, rect.y + 2, rect.width, 18), info, EditorStyles.miniLabel);
    }
}
