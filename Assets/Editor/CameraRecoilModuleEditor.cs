using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraRecoilModule))]
public class CameraRecoilModuleEditor : Editor
{
    private const int TextureWidth = 256;
    private const int TextureHeight = 80;
    private Texture2D _verticalTex;
    private Texture2D _horizontalTex;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();

        float maxVMult = serializedObject.FindProperty("maxVerticalRecoilMult").floatValue;
        float vGrowth = serializedObject.FindProperty("verticalGrowthRate").floatValue;
        float verticalKick = serializedObject.FindProperty("verticalKick").floatValue;
        float maxHWindow = serializedObject.FindProperty("maxHorizontalRecoilWindow").floatValue;
        float hGrowth = serializedObject.FindProperty("horizontalGrowthRate").floatValue;
        int maxShots = serializedObject.FindProperty("shotsToMaxRecoil").intValue;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Vertical Recoil Curve", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            $"Shot 1: {verticalKick:F1}°  →  Shot {maxShots}: {verticalKick * maxVMult:F1}°",
            EditorStyles.miniLabel
        );
        _verticalTex = DrawCurve(
            _verticalTex,
            maxShots,
            t => 1f + (maxVMult - 1f) * (1f - Mathf.Exp(-vGrowth * t)),
            new Color(0.3f, 0.6f, 1f), // blue
            0f,
            maxVMult
        );
        GUILayout.Label(
            _verticalTex,
            GUILayout.Width(TextureWidth),
            GUILayout.Height(TextureHeight)
        );

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Horizontal Window Curve", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            $"Shot 1: ±{maxHWindow * (1f - Mathf.Exp(-hGrowth * (1f / maxShots))):F2}°  →  Shot {maxShots}: ±{maxHWindow:F2}°",
            EditorStyles.miniLabel
        );
        _horizontalTex = DrawCurve(
            _horizontalTex,
            maxShots,
            t => maxHWindow * (1f - Mathf.Exp(-hGrowth * t)),
            new Color(0.3f, 1f, 0.5f), // green
            0f,
            maxHWindow
        );
        GUILayout.Label(
            _horizontalTex,
            GUILayout.Width(TextureWidth),
            GUILayout.Height(TextureHeight)
        );

        if (GUI.changed)
            Repaint();
    }

    private Texture2D DrawCurve(
        Texture2D tex,
        int maxShots,
        System.Func<float, float> curve,
        Color lineColor,
        float yMin,
        float yMax
    )
    {
        if (tex == null || tex.width != TextureWidth || tex.height != TextureHeight)
        {
            tex = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
        }

        Color bg = new Color(0.15f, 0.15f, 0.15f, 1f);
        Color grid = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Fill background
        Color[] pixels = new Color[TextureWidth * TextureHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = bg;

        // Grid lines at 25% 50% 75%
        foreach (float gridT in new[] { 0.25f, 0.5f, 0.75f })
        {
            int gx = Mathf.RoundToInt(gridT * (TextureWidth - 1));
            int gy = Mathf.RoundToInt(gridT * (TextureHeight - 1));
            for (int y = 0; y < TextureHeight; y++)
                pixels[y * TextureWidth + gx] = grid;
            for (int x = 0; x < TextureWidth; x++)
                pixels[gy * TextureWidth + x] = grid;
        }

        // Draw curve
        float range = Mathf.Max(yMax - yMin, 0.001f);
        for (int x = 0; x < TextureWidth; x++)
        {
            float t = (float)x / (TextureWidth - 1);
            float val = curve(t);
            int py = Mathf.Clamp(
                Mathf.RoundToInt(((val - yMin) / range) * (TextureHeight - 1)),
                0,
                TextureHeight - 1
            );

            // Draw 2px thick line
            for (int dy = -1; dy <= 1; dy++)
            {
                int drawY = Mathf.Clamp(py + dy, 0, TextureHeight - 1);
                pixels[drawY * TextureWidth + x] = lineColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
