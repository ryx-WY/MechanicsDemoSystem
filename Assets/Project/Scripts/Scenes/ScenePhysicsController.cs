using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 场景物理控制器（仅管理全局重力）
/// 挂载到SceneController空物体
/// </summary>
public class ScenePhysicsController : MonoBehaviour
{
    public static ScenePhysicsController Instance;

    [Header("全局物理参数")]
    public float globalGravity = 9.81f;

    [Header("UI引用 - 重力参数项")]
    public Slider gravitySlider;
    public TMP_InputField gravityInput; // 重力输入框

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 初始化重力参数
        globalGravity = gravitySlider.value;
        UpdateGravityDisplay(globalGravity); // 统一更新滑块+输入框
        // 绑定滑块事件
        gravitySlider.onValueChanged.AddListener(OnGravitySliderChanged);
        // 绑定输入框事件
        gravityInput.onEndEdit.AddListener(OnGravityInputChanged);
    }

    /// <summary>
    /// 滑块拖动时，同步输入框
    /// </summary>
    public void OnGravitySliderChanged(float value)
    {
        globalGravity = value;
        UpdateGravityDisplay(value);
    }
    /// <summary>
    /// 输入框输入时，同步滑块
    /// </summary>
    public void OnGravityInputChanged(string inputValue)
    {
        if (float.TryParse(inputValue, out float value))
        {
            // 限制数值在滑块范围内（避免输入超出Min/Max的值）
            value = Mathf.Clamp(value, gravitySlider.minValue, gravitySlider.maxValue);
            globalGravity = value;
            gravitySlider.value = value;
            UpdateGravityDisplay(value);
        }
        else
        {
            // 输入非法字符，恢复之前的数值
            UpdateGravityDisplay(globalGravity);
        }
    }
    /// <summary>
    /// 统一更新滑块、输入框的显示
    /// </summary>
    private void UpdateGravityDisplay(float value)
    {
        string displayValue = value.ToString("F2"); // 保留两位小数
        gravitySlider.value = value;
        gravityInput.text = displayValue;
    }
    /// <summary>
    /// 重置场景物理参数
    /// </summary>
    public void ResetScene()
    {
        gravitySlider.value = 9.81f; // 恢复默认重力
    }
}
