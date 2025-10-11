using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonTextColorChanger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color selectedColor = Color.black;

    [Header("References")]
    [SerializeField] private List<ButtonTextColorChanger> otherButtons;

    private TextMeshProUGUI _buttonText;
    private Button _button;
    private bool _isSelected = false;

    public bool IsSelected => _isSelected;

    void Awake()
    {
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (_buttonText == null) Debug.LogError("Could not find TextMeshProUGUI in children of button.");
        _button = GetComponent<Button>();
    }

    void Start() => SetColor(normalColor);

    public void OnPointerEnter(PointerEventData _) { if (!_isSelected) SetColor(selectedColor); }
    public void OnPointerExit(PointerEventData _)  { if (!_isSelected) SetColor(normalColor); }

    public void OnPointerDown(PointerEventData _)
    {
        // Tomá el foco explícitamente
        EventSystem.current?.SetSelectedGameObject(gameObject);
        SelectThisButton();
    }

    public void OnSelect(BaseEventData _) => SelectThisButton();

    public void OnDeselect(BaseEventData _)
    {
        _isSelected = false;
        SetColor(normalColor);
    }

    private void SelectThisButton()
    {
        foreach (var btn in otherButtons) btn.ForceDeselect();
        _isSelected = true;
        SetColor(selectedColor);
    }

    public void ForceDeselect()
    {
        _isSelected = false;
        SetColor(normalColor);
    }

    private void SetColor(Color c) { if (_buttonText != null) _buttonText.color = c; }
}
