using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SpringVector3))]
public sealed class SpringVector3Drawer : PropertyDrawer
{
    private const float PREVIEW_HEIGHT = 64f;
    private const float FIELD_SPACING = 2f;
    private const int SAMPLE_COUNT = 200;
    private const float SIM_DURATION = 3.5f;

    private static readonly Color k_BgColor = new Color(0.11f, 0.11f, 0.11f);
    private static readonly Color k_CurveColor = new Color(0.28f, 0.88f, 0.42f);
    private static readonly Color k_ZeroColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
    private static readonly Color k_Underdamped = new Color(0.35f, 0.65f, 1f);
    private static readonly Color k_Overdamped = new Color(1f, 0.55f, 0.2f);
    private static readonly Color k_Critical = new Color(0.35f, 0.88f, 0.42f);

    // Static buffers are safe here — PropertyDrawers draw sequentially, never concurrently.
    private static readonly float[] s_Samples = new float[SAMPLE_COUNT];
    private static readonly Vector3[] s_Points = new Vector3[SAMPLE_COUNT];

    private GUIStyle _infoStyle;

    private GUIStyle InfoStyle
    {
        get
        {
            if (_infoStyle != null)
                return _infoStyle;
            _infoStyle = new GUIStyle(EditorStyles.miniLabel);
            _infoStyle.alignment = TextAnchor.UpperRight;
            return _infoStyle;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float lineH = EditorGUIUtility.singleLineHeight + FIELD_SPACING;
        return EditorGUIUtility.singleLineHeight + lineH * 2f + PREVIEW_HEIGHT + FIELD_SPACING * 3f;
    }

    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(pos, label, property);

        float lineH = EditorGUIUtility.singleLineHeight;

        Rect foldRect = new Rect(pos.x, pos.y, pos.width, lineH);
        property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // FindPropertyRelative in OnGUI is unavoidable for PropertyDrawers (no OnEnable).
            SerializedProperty stiffnessProp = property.FindPropertyRelative("stiffness");
            SerializedProperty dampingProp = property.FindPropertyRelative("damping");

            float y = pos.y + lineH + FIELD_SPACING;
            EditorGUI.PropertyField(new Rect(pos.x, y, pos.width, lineH), stiffnessProp);
            y += lineH + FIELD_SPACING;
            EditorGUI.PropertyField(new Rect(pos.x, y, pos.width, lineH), dampingProp);
            y += lineH + FIELD_SPACING;
            DrawResponsePreview(
                new Rect(pos.x, y, pos.width, PREVIEW_HEIGHT),
                stiffnessProp.floatValue,
                dampingProp.floatValue
            );

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawResponsePreview(Rect rect, float stiffness, float damping)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect, k_BgColor);

        // Simulate from an impulse sized relative to stiffness so the curve
        // always fills the preview regardless of parameter range.
        float dt = SIM_DURATION / SAMPLE_COUNT;
        float val = 0f;
        float vel = Mathf.Sqrt(Mathf.Max(stiffness, 0.01f)) * 0.3f;
        float lo = 0f;
        float hi = vel * dt;

        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            float accel = -stiffness * val - damping * vel;
            vel += accel * dt;
            val += vel * dt;
            s_Samples[i] = val;
            if (val < lo)
                lo = val;
            if (val > hi)
                hi = val;
        }

        if (lo > 0f)
            lo = 0f;
        if (hi < 0.001f)
            hi = 0.001f;

        float span = hi - lo;
        float pad = span * 0.14f;
        float dMin = lo - pad;
        float dRange = hi + pad - dMin;

        float zeroNorm = -dMin / dRange;
        float zeroY = rect.y + rect.height * (1f - zeroNorm);

        Handles.color = k_ZeroColor;
        Handles.DrawLine(new Vector3(rect.x, zeroY, 0f), new Vector3(rect.xMax, zeroY, 0f));

        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            float t = i / (float)(SAMPLE_COUNT - 1);
            float norm = (s_Samples[i] - dMin) / dRange;
            s_Points[i] = new Vector3(
                rect.x + t * rect.width,
                rect.y + rect.height * (1f - norm),
                0f
            );
        }

        Handles.color = k_CurveColor;
        for (int i = 0; i < SAMPLE_COUNT - 1; i++)
            Handles.DrawLine(s_Points[i], s_Points[i + 1]);

        // Critical damping: ζ = damping / (2√stiffness)  →  critical when ζ = 1
        float critDamping = 2f * Mathf.Sqrt(Mathf.Max(stiffness, 0.01f));
        float ratio = damping / critDamping;

        string dampingTag;
        Color tagColor;
        if (ratio < 0.95f)
        {
            dampingTag = "underdamped";
            tagColor = k_Underdamped;
        }
        else if (ratio > 1.05f)
        {
            dampingTag = "overdamped";
            tagColor = k_Overdamped;
        }
        else
        {
            dampingTag = "~critical";
            tagColor = k_Critical;
        }

        InfoStyle.normal.textColor = tagColor;
        GUI.Label(new Rect(rect.x, rect.y + 2f, rect.width - 4f, 14f), dampingTag, InfoStyle);
    }
}
