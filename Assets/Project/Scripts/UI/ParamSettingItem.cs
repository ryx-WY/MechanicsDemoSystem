using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 通用参数设置项（滑块+输入框）
/// </summary>
public class ParamSettingItem : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI labelText;
    public Slider slider;
    public TMP_InputField inputField;

    // 回调：当数值改变时
    public event Action<float> OnValueChanged;

    private bool isUpdating = false; // 防止循环触发

    public void Initialize(string label, float min, float max, float defaultValue, int decimalPlaces = 2)
    {
        labelText.text = label;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultValue;
        inputField.text = defaultValue.ToString($"F{decimalPlaces}");

        // 绑定事件
        slider.onValueChanged.AddListener(OnSliderChanged);
        inputField.onEndEdit.AddListener(OnInputChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (isUpdating) return;
        isUpdating = true;

        inputField.text = value.ToString("F2");
        OnValueChanged?.Invoke(value);

        isUpdating = false;
    }

    private void OnInputChanged(string text)
    {
        if (isUpdating) return;
        if (float.TryParse(text, out float value))
        {
            value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
            isUpdating = true;

            slider.value = value;
            inputField.text = value.ToString("F2");
            OnValueChanged?.Invoke(value);

            isUpdating = false;
        }
    }

    public float GetValue() => slider.value;
    public void SetValue(float value)
    {
        slider.value = value;
        inputField.text = value.ToString("F2");
    }
}