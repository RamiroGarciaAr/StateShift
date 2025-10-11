using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI _buttonText;
    private Button _button;
    private bool _isSelected = false;
    [Header("Text Colors")]
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color selectedColor = Color.black;

    void Awake()
    {
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (_buttonText == null)
        {
            Debug.LogError("Could not Find text in the Button Componenet");
        }
        _button = GetComponent<Button>();
        _buttonText.color = normalColor;
    }

    private void ResetColor()
    {
        _buttonText.color = selectedColor;
    }

    void Update()
    {
        //if (_isSelected) _buttonText.color = selectedColor;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _buttonText.color = selectedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _buttonText.color = normalColor;
    }

}
