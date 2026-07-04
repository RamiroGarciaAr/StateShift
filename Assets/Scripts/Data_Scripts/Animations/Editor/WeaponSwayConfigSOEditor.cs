using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponSwayConfigSO))]
public class WeaponSwayConfigSOEditor : Editor
{
    private const float PREVIEW_HEIGHT = 100f;
    private const float TOP_PADDING = 20f;
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

        WeaponSwayConfigSO config = (WeaponSwayConfigSO)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Live Dynamics Preview (Step Response)", EditorStyles.boldLabel);

        DrawGraph("Sway Dynamics", config.SwayF, config.SwayZ, config.SwayR);
        EditorGUILayout.Space(15);
        DrawGraph("Breathing Dynamics", config.BreathF, config.BreathZ, config.BreathR);

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
        // Duration is inversely proportional to frequency to keep the curve visible
        float duration = 4.0f / Mathf.Max(f, 0.1f);
        float dt = duration / SAMPLE_COUNT;

        SecondOrderDynamics sim = new SecondOrderDynamics(f, z, r, Vector3.zero);
        Vector3 targetPos = Vector3.one;

        float minVal = 0f;
        float maxVal = 1f;

        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            // Use x component for the 1D graph
            float val = sim.Update(dt, targetPos).x;
            s_Samples[i] = val;
            
            if (val < minVal) minVal = val;
            if (val > maxVal) maxVal = val;
        }

        // Calculate vertical scale with 20% headroom
        float range = maxVal - minVal;
        float padding = range * 0.2f;
        float viewMin = minVal - padding;
        float viewMax = maxVal + padding;
        float viewRange = Mathf.Max(viewMax - viewMin, 0.001f);

        // Helper to normalize values to graph space
        float Normalize(float v) => 1f - (v - viewMin) / viewRange;

        // Draw Reference Lines
        Handles.color = k_ZeroColor;
        float zeroY = rect.y + rect.height * Normalize(0f);
        Handles.DrawLine(new Vector3(rect.x, zeroY, 0), new Vector3(rect.xMax, zeroY, 0));

        Handles.color = k_TargetColor;
        float targetY = rect.y + rect.height * Normalize(1f);
        Handles.DrawLine(new Vector3(rect.x, targetY, 0), new Vector3(rect.xMax, targetY, 0));

        // Generate points for the curve
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

        // Draw Curve
        Handles.color = k_CurveColor;
        Handles.DrawPolyLine(s_Points);
        
        // Info label
        string info = $"f: {f:F1} | z: {z:F2} | r: {r:F2} (Duration: {duration:F2}s)";
        GUI.Label(new Rect(rect.x + 5, rect.y + 2, rect.width, 18), info, EditorStyles.miniLabel);
    }
}
