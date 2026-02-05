using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GravitySettingItem : MonoBehaviour
{
    public Slider gravitySlider;
    public TMP_InputField gravityInput;

    void Start()
    {
        gravitySlider.onValueChanged.AddListener(OnSliderChanged);
        gravityInput.onEndEdit.AddListener(OnInputChanged);

        // ≥ı ºÕ¨≤Ω
        gravitySlider.value = SceneController.Instance.GlobalGravity;
    }

    void OnSliderChanged(float value)
    {
        SceneController.Instance.GlobalGravity = value;
        gravityInput.text = value.ToString("F2");
    }

    void OnInputChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            value = Mathf.Clamp(value, gravitySlider.minValue, gravitySlider.maxValue);
            SceneController.Instance.GlobalGravity = value;
            gravitySlider.value = value;
        }
    }
}