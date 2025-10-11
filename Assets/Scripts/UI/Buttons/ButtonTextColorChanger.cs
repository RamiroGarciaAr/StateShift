using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
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
        if (_buttonText == null)
            Debug.LogError("Could not find TextMeshProUGUI in children of button.");

        _button = GetComponent<Button>();
    }

    void Start()
    {
        SetColor(normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isSelected)
            SetColor(selectedColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isSelected)
            SetColor(normalColor);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SelectThisButton();
    }

    private void SelectThisButton()
    {
        // Deseleccionar todos los demás
        foreach (var btn in otherButtons)
            btn.Deselect();

        // Seleccionar este
        _isSelected = true;
        SetColor(selectedColor);
    }

    private void Deselect()
    {
        _isSelected = false;
        SetColor(normalColor);
    }

    private void SetColor(Color color)
    {
        if (_buttonText != null)
            _buttonText.color = color;
    }
}
