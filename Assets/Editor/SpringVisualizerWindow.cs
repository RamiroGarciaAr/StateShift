using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class SpringVisualizerWindow : EditorWindow
{
    #region Constants

    private const string k_PrefStiffness = "StateShift.SpringViz.Stiffness";
    private const string k_PrefDamping = "StateShift.SpringViz.Damping";
    private const string k_PrefImpulse = "StateShift.SpringViz.Impulse";

    private const float k_SimStep = 1f / 120f;
    private const int k_HistorySize = 512;
    private const float k_LeftPanelWidth = 215f;
    private const float k_GraphPadLeft = 52f;
    private const float k_GraphPadOther = 12f;
    private const float k_GraphPadBottom = 20f;
    private const float k_MinStiffness = 1f;
    private const float k_MaxStiffness = 500f;
    private const float k_MinDamping = 0f;
    private const float k_MaxDamping = 80f;

    private static readonly Color k_ColorX = new Color(0.92f, 0.32f, 0.32f);
    private static readonly Color k_ColorY = new Color(0.32f, 0.88f, 0.32f);
    private static readonly Color k_ColorZ = new Color(0.35f, 0.55f, 1f);
    private static readonly Color k_ColorBg = new Color(0.11f, 0.11f, 0.11f);
    private static readonly Color k_ColorGrid = new Color(0.28f, 0.28f, 0.28f, 0.5f);
    private static readonly Color k_ColorZeroLine = new Color(0.55f, 0.55f, 0.55f, 0.7f);
    private static readonly Color k_ColorPanel = new Color(0.165f, 0.165f, 0.165f);
    private static readonly Color k_ColorBorder = new Color(0.22f, 0.22f, 0.22f);

    private static readonly (string Label, float Stiffness, float Damping)[] k_Presets =
    {
        ("Snappy", 300f, 25f),
        ("Smooth", 80f, 18f),
        ("Bouncy", 150f, 8f),
        ("Sluggish", 30f, 14f),
    };

    #endregion

    #region Spring State

    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _target;
    private float _stiffness;
    private float _damping;
    private float _impulseMag;

    #endregion

    #region History Buffers

    // Circular buffers — pre-allocated, no runtime alloc.
    private readonly float[] _hX = new float[k_HistorySize];
    private readonly float[] _hY = new float[k_HistorySize];
    private readonly float[] _hZ = new float[k_HistorySize];
    private int _head;
    private int _count;

    // Per-frame line-point scratch — avoids alloc inside DrawSeries.
    private readonly Vector3[] _ptX = new Vector3[k_HistorySize];
    private readonly Vector3[] _ptY = new Vector3[k_HistorySize];
    private readonly Vector3[] _ptZ = new Vector3[k_HistorySize];

    #endregion

    #region Editor Update

    private double _lastTick;
    private float _accumulator;
    private bool _paused;

    #endregion

    #region UI References

    private IMGUIContainer _graphArea;
    private Slider _stiffnessSlider;
    private Slider _dampingSlider;

    // Cached IMGUI styles — initialized once in EnsureStyles().
    private GUIStyle _styleAxisLabel;
    private GUIStyle _styleValX;
    private GUIStyle _styleValY;
    private GUIStyle _styleValZ;
    private GUIStyle _styleEmpty;

    #endregion

    [MenuItem("State Shift/Spring Visualizer")]
    public static void Open()
    {
        var w = GetWindow<SpringVisualizerWindow>();
        w.titleContent = new GUIContent("Spring Visualizer");
        w.minSize = new Vector2(680f, 320f);
        w.Show();
    }

    private void OnEnable()
    {
        _stiffness = EditorPrefs.GetFloat(k_PrefStiffness, 100f);
        _damping = EditorPrefs.GetFloat(k_PrefDamping, 10f);
        _impulseMag = EditorPrefs.GetFloat(k_PrefImpulse, 5f);
        _lastTick = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
        BuildUI();
    }

    private void OnDisable()
    {
        EditorPrefs.SetFloat(k_PrefStiffness, _stiffness);
        EditorPrefs.SetFloat(k_PrefDamping, _damping);
        EditorPrefs.SetFloat(k_PrefImpulse, _impulseMag);
        EditorApplication.update -= Tick;
    }

    #region Simulation

    private void Tick()
    {
        double now = EditorApplication.timeSinceStartup;
        float dt = (float)(now - _lastTick);
        _lastTick = now;

        // Guard against tab-switch spikes or debugger pauses.
        if (_paused || dt <= 0f || dt > 0.25f)
            return;

        _accumulator += dt;
        while (_accumulator >= k_SimStep)
        {
            Step(k_SimStep);
            PushSample();
            _accumulator -= k_SimStep;
        }

        _graphArea?.MarkDirtyRepaint();
    }

    private void Step(float dt)
    {
        Vector3 accel = -_stiffness * (_position - _target) - _damping * _velocity;
        _velocity += accel * dt;
        _position += _velocity * dt;
    }

    private void PushSample()
    {
        _hX[_head] = _position.x;
        _hY[_head] = _position.y;
        _hZ[_head] = _position.z;
        _head = (_head + 1) % k_HistorySize;
        if (_count < k_HistorySize)
            _count++;
    }

    private void ResetSpring()
    {
        _position = Vector3.zero;
        _velocity = Vector3.zero;
        _target = Vector3.zero;
        _head = 0;
        _count = 0;
        Array.Clear(_hX, 0, k_HistorySize);
        Array.Clear(_hY, 0, k_HistorySize);
        Array.Clear(_hZ, 0, k_HistorySize);
    }

    private void ApplyImpulse(Vector3 axis) => _velocity += axis * _impulseMag;

    private void SetTarget(Vector3 t) => _target = t;

    #endregion

    #region UI Construction

    private void BuildUI()
    {
        VisualElement root = rootVisualElement;
        root.Clear();
        root.style.flexDirection = FlexDirection.Row;
        root.style.flexGrow = 1f;

        root.Add(BuildLeftPanel());
        root.Add(BuildGraphPanel());
    }

    private VisualElement BuildLeftPanel()
    {
        var panel = new VisualElement();
        panel.style.width = k_LeftPanelWidth;
        panel.style.minWidth = k_LeftPanelWidth;
        panel.style.flexShrink = 0f;
        panel.style.backgroundColor = new StyleColor(k_ColorPanel);
        panel.style.borderRightWidth = 1f;
        panel.style.borderRightColor = new StyleColor(k_ColorBorder);
        panel.style.paddingTop = 10f;
        panel.style.paddingLeft = 10f;
        panel.style.paddingRight = 10f;
        panel.style.paddingBottom = 10f;

        SectionHeader(panel, "PARAMETERS");

        _stiffnessSlider = LabeledSlider(
            panel,
            "Stiffness",
            _stiffness,
            k_MinStiffness,
            k_MaxStiffness,
            v => _stiffness = v
        );
        _dampingSlider = LabeledSlider(
            panel,
            "Damping",
            _damping,
            k_MinDamping,
            k_MaxDamping,
            v => _damping = v
        );

        SectionHeader(panel, "PRESETS");
        var presetGrid = new VisualElement();
        presetGrid.style.flexDirection = FlexDirection.Row;
        presetGrid.style.flexWrap = Wrap.Wrap;
        presetGrid.style.marginBottom = 4f;
        panel.Add(presetGrid);

        foreach ((string label, float s, float d) in k_Presets)
        {
            float cs = s,
                cd = d; // capture copies for lambda
            var btn = new Button(() =>
            {
                _stiffness = cs;
                _damping = cd;
                _stiffnessSlider.SetValueWithoutNotify(cs);
                _dampingSlider.SetValueWithoutNotify(cd);
            })
            {
                text = label,
            };
            btn.style.flexGrow = 1f;
            btn.style.marginRight = 2f;
            btn.style.marginBottom = 2f;
            presetGrid.Add(btn);
        }

        SectionHeader(panel, "IMPULSE");

        var magRow = new VisualElement();
        magRow.style.flexDirection = FlexDirection.Row;
        magRow.style.alignItems = Align.Center;
        magRow.style.marginBottom = 4f;
        panel.Add(magRow);

        magRow.Add(new Label("Magnitude") { style = { width = 72f, flexShrink = 0f } });
        var magField = new FloatField { value = _impulseMag };
        magField.style.flexGrow = 1f;
        magField.RegisterValueChangedCallback(e => _impulseMag = Mathf.Max(0f, e.newValue));
        magRow.Add(magField);

        var axisRow = new VisualElement();
        axisRow.style.flexDirection = FlexDirection.Row;
        axisRow.style.marginBottom = 6f;
        panel.Add(axisRow);
        ImpulseButton(axisRow, "+X", Vector3.right);
        ImpulseButton(axisRow, "+Y", Vector3.up);
        ImpulseButton(axisRow, "+Z", Vector3.forward);

        SectionHeader(panel, "TARGET");
        ActionButton(panel, "Set (0, 1, 0)", () => SetTarget(Vector3.up));
        ActionButton(panel, "Set (1, 1, 1)", () => SetTarget(Vector3.one));
        ActionButton(panel, "Clear target", () => SetTarget(Vector3.zero));

        SectionHeader(panel, "PLAYBACK");

        var pauseBtn = new Button { text = "Pause" };
        pauseBtn.clicked += () =>
        {
            _paused = !_paused;
            pauseBtn.text = _paused ? "Resume" : "Pause";
        };
        pauseBtn.style.marginBottom = 3f;
        panel.Add(pauseBtn);

        ActionButton(panel, "Reset", ResetSpring);

        return panel;
    }

    private VisualElement BuildGraphPanel()
    {
        var panel = new VisualElement();
        panel.style.flexGrow = 1f;
        panel.style.backgroundColor = new StyleColor(k_ColorBg);

        _graphArea = new IMGUIContainer(DrawGraph);
        _graphArea.style.flexGrow = 1f;
        panel.Add(_graphArea);

        return panel;
    }

    #endregion

    #region Graph Drawing (IMGUI)

    private void EnsureStyles()
    {
        if (_styleAxisLabel != null)
            return;

        _styleAxisLabel = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight,
            fontSize = 9,
        };
        _styleAxisLabel.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

        _styleValX = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperRight };
        _styleValX.normal.textColor = k_ColorX;

        _styleValY = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperRight };
        _styleValY.normal.textColor = k_ColorY;

        _styleValZ = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperRight };
        _styleValZ.normal.textColor = k_ColorZ;

        _styleEmpty = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { wordWrap = true };
    }

    private void DrawGraph()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        EnsureStyles();

        Rect r = _graphArea.contentRect;
        if (r.width < 4f || r.height < 4f)
            return;

        EditorGUI.DrawRect(r, k_ColorBg);

        if (_count < 2)
        {
            GUI.Label(r, "Fire an impulse or set a target to begin.", _styleEmpty);
            return;
        }

        var g = new Rect(
            r.x + k_GraphPadLeft,
            r.y + k_GraphPadOther,
            r.width - k_GraphPadLeft - k_GraphPadOther,
            r.height - k_GraphPadOther - k_GraphPadBottom
        );

        ComputeRange(out float lo, out float hi);
        PadRange(ref lo, ref hi);
        float span = hi - lo;

        DrawGrid(g, lo, span);
        DrawSeries(g, _hX, k_ColorX, lo, span, _ptX);
        DrawSeries(g, _hY, k_ColorY, lo, span, _ptY);
        DrawSeries(g, _hZ, k_ColorZ, lo, span, _ptZ);
        DrawAxisLabels(g, r, lo, hi, span);
        DrawCurrentValues(r);
        DrawLegend(r);
    }

    private void ComputeRange(out float lo, out float hi)
    {
        lo = 0f;
        hi = 0f;
        for (int i = 0; i < _count; i++)
        {
            int idx = (_head - _count + i + k_HistorySize) % k_HistorySize;
            float x = _hX[idx],
                y = _hY[idx],
                z = _hZ[idx];
            if (x < lo)
                lo = x;
            if (x > hi)
                hi = x;
            if (y < lo)
                lo = y;
            if (y > hi)
                hi = y;
            if (z < lo)
                lo = z;
            if (z > hi)
                hi = z;
        }
    }

    private static void PadRange(ref float lo, ref float hi)
    {
        if (lo > -0.001f)
            lo = -0.001f;
        if (hi < 0.001f)
            hi = 0.001f;
        float pad = (hi - lo) * 0.12f;
        lo -= pad;
        hi += pad;
    }

    private void DrawGrid(Rect g, float lo, float span)
    {
        const int divisions = 4;

        Handles.color = k_ColorGrid;
        for (int i = 1; i < divisions; i++)
        {
            float y = g.y + g.height * (i / (float)divisions);
            Handles.DrawLine(new Vector3(g.x, y, 0f), new Vector3(g.xMax, y, 0f));
        }

        float zeroY = g.y + g.height * (1f - (-lo / span));
        if (zeroY >= g.y && zeroY <= g.yMax)
        {
            Handles.color = k_ColorZeroLine;
            Handles.DrawLine(new Vector3(g.x, zeroY, 0f), new Vector3(g.xMax, zeroY, 0f));
        }
    }

    private void DrawSeries(Rect g, float[] hist, Color color, float lo, float span, Vector3[] pts)
    {
        Handles.color = color;
        int n = 0;

        for (int i = 0; i < _count; i++)
        {
            int idx = (_head - _count + i + k_HistorySize) % k_HistorySize;
            float t = i / (float)(_count - 1);
            float norm = (hist[idx] - lo) / span;
            pts[n++] = new Vector3(g.x + t * g.width, g.y + g.height * (1f - norm), 0f);
        }

        for (int i = 0; i < n - 1; i++)
            Handles.DrawLine(pts[i], pts[i + 1]);
    }

    private void DrawAxisLabels(Rect g, Rect r, float lo, float hi, float span)
    {
        float lx = r.x + 2f;
        float lw = k_GraphPadLeft - 4f;
        float lh = 14f;

        GUI.Label(new Rect(lx, g.y - 7f, lw, lh), hi.ToString("F2"), _styleAxisLabel);
        GUI.Label(new Rect(lx, g.yMax - 7f, lw, lh), lo.ToString("F2"), _styleAxisLabel);

        float zeroY = g.y + g.height * (1f - (-lo / span));
        if (zeroY >= g.y + lh && zeroY <= g.yMax - lh)
            GUI.Label(new Rect(lx, zeroY - 7f, lw, lh), "0", _styleAxisLabel);
    }

    private void DrawCurrentValues(Rect r)
    {
        // string.Format allocates — unavoidable for live float display in an editor tool.
        float x = r.xMax - k_GraphPadOther - 52f;
        float y = r.y + k_GraphPadOther + 2f;
        float lh = 13f;
        float lw = 50f;

        GUI.Label(new Rect(x, y, lw, lh), _position.x.ToString("F3"), _styleValX);
        GUI.Label(new Rect(x, y + lh, lw, lh), _position.y.ToString("F3"), _styleValY);
        GUI.Label(new Rect(x, y + lh * 2, lw, lh), _position.z.ToString("F3"), _styleValZ);
    }

    private void DrawLegend(Rect r)
    {
        float y = r.yMax - k_GraphPadBottom;
        float x = r.x + k_GraphPadLeft;
        float sw = 14f;
        float gp = 20f;

        EditorGUI.DrawRect(new Rect(x, y + 5f, sw, 2f), k_ColorX);
        GUI.Label(new Rect(x + sw + 2f, y, gp, 14f), "X", _styleValX);
        x += sw + gp + 6f;

        EditorGUI.DrawRect(new Rect(x, y + 5f, sw, 2f), k_ColorY);
        GUI.Label(new Rect(x + sw + 2f, y, gp, 14f), "Y", _styleValY);
        x += sw + gp + 6f;

        EditorGUI.DrawRect(new Rect(x, y + 5f, sw, 2f), k_ColorZ);
        GUI.Label(new Rect(x + sw + 2f, y, gp, 14f), "Z", _styleValZ);
    }

    #endregion

    #region UI Helpers

    private static void SectionHeader(VisualElement parent, string text)
    {
        var lbl = new Label(text);
        lbl.style.marginTop = 8f;
        lbl.style.marginBottom = 3f;
        lbl.style.fontSize = 9;
        lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        lbl.style.color = new StyleColor(new Color(0.55f, 0.55f, 0.55f));
        parent.Add(lbl);
    }

    private static Slider LabeledSlider(
        VisualElement parent,
        string labelText,
        float value,
        float min,
        float max,
        Action<float> onChange
    )
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.marginBottom = 3f;
        parent.Add(row);

        row.Add(new Label(labelText) { style = { width = 58f, flexShrink = 0f } });

        var valLabel = new Label(value.ToString("F0"));
        valLabel.style.width = 32f;
        valLabel.style.flexShrink = 0f;
        valLabel.style.unityTextAlign = TextAnchor.MiddleRight;

        var slider = new Slider(min, max) { value = value };
        slider.style.flexGrow = 1f;
        slider.RegisterValueChangedCallback(e =>
        {
            valLabel.text = e.newValue.ToString("F0");
            onChange(e.newValue);
        });

        row.Add(slider);
        row.Add(valLabel);
        return slider;
    }

    private void ImpulseButton(VisualElement parent, string text, Vector3 axis)
    {
        Vector3 captured = axis;
        var btn = new Button(() => ApplyImpulse(captured)) { text = text };
        btn.style.flexGrow = 1f;
        btn.style.marginRight = 2f;
        parent.Add(btn);
    }

    private static void ActionButton(VisualElement parent, string text, Action onClick)
    {
        var btn = new Button(onClick) { text = text };
        btn.style.marginBottom = 3f;
        parent.Add(btn);
    }

    #endregion
}
