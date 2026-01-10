using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 单个物理物体控制器（管理单物体参数+速度计算）
/// 挂载到平抛小球ProjectileBall
/// </summary>
public class ObjectPhysicsController : MonoBehaviour
{
    [Header("物体引用")]
    public Rigidbody rb;

    [Header("物体参数")]
    public float objectMass = 1f;
    public float initialVelocity = 5f;

    [Header("UI引用 - 可配置项")]
    public Slider massSlider;
    public TMP_InputField massInput;
    public Slider velocitySlider;
    public TMP_InputField velocityInput;

    [Header("UI引用 - 实时速度展示（仅显示）")]
    public TMP_Text horizontalSpeedText;
    public TMP_Text verticalSpeedText;
    public TMP_Text totalSpeedText;

    private float motionStartTime;    // 运动开始时间
    private float horizontalSpeed;    // 实时水平速度
    private float verticalSpeed;      // 实时竖直速度
    private float totalSpeed;         // 实时合速度

    void Start()
    {
        // 初始化物体参数
        InitObjectParams();
        // 绑定滑块和输入框事件
        BindSliderEvents();
        // 初始化刚体状态
        rb.isKinematic = true;
        rb.useGravity = false;
        // 初始化速度显示
        UpdateSpeedDisplay(initialVelocity, 0, initialVelocity);
    }

    void Update()
    {
        // 仅在运动中且未暂停时计算速度
        if (GlobalUIController.Instance.IsMotionRunning() && !GlobalUIController.Instance.IsMotionPaused())
        {
            CalculateSpeed();
            UpdateSpeedDisplay(horizontalSpeed, verticalSpeed, totalSpeed);
        }
    }

    /// <summary>
    /// 初始化物体参数（同步滑块和输入框）
    /// </summary>
    private void InitObjectParams()
    {
        // 质量初始化
        objectMass = massSlider.value;
        string massDisplay = objectMass.ToString("F2");
        massInput.text = massDisplay;
        rb.mass = objectMass;

        // 初速度初始化
        initialVelocity = velocitySlider.value;
        string velocityDisplay = initialVelocity.ToString("F2");
        velocityInput.text = velocityDisplay;
        horizontalSpeed = initialVelocity;
    }

    /// <summary>
    /// 绑定滑块和输入框事件
    /// </summary>
    private void BindSliderEvents()
    {
        // 滑块事件
        massSlider.onValueChanged.AddListener(OnMassChanged);
        velocitySlider.onValueChanged.AddListener(OnVelocityChanged);

        // 输入框事件
        massInput.onEndEdit.AddListener(OnMassInputChanged);
        velocityInput.onEndEdit.AddListener(OnVelocityInputChanged);
    }

    /// <summary>
    /// 质量滑块变化回调
    /// </summary>
    public void OnMassChanged(float value)
    {
        objectMass = value;
        string displayValue = value.ToString("F2");
        massInput.text = displayValue;
        rb.mass = value;
    }

    /// <summary>
    /// 质量输入框输入回调
    /// </summary>
    private void OnMassInputChanged(string inputValue)
    {
        if (float.TryParse(inputValue, out float value))
        {
            value = Mathf.Clamp(value, massSlider.minValue, massSlider.maxValue);
            objectMass = value;
            massSlider.value = value;
            rb.mass = value;
        }
        else
        {
            massInput.text = objectMass.ToString("F2");
        }
    }

    /// <summary>
    /// 初速度滑块变化回调
    /// </summary>
    public void OnVelocityChanged(float value)
    {
        initialVelocity = value;
        string displayValue = value.ToString("F2");
        velocityInput.text = displayValue;
        horizontalSpeed = value;

        // 未运动时更新速度显示
        if (!GlobalUIController.Instance.IsMotionRunning())
        {
            UpdateSpeedDisplay(horizontalSpeed, 0, horizontalSpeed);
        }
    }

    /// <summary>
    /// 初速度输入框输入回调
    /// </summary>
    private void OnVelocityInputChanged(string inputValue)
    {
        if (float.TryParse(inputValue, out float value))
        {
            value = Mathf.Clamp(value, velocitySlider.minValue, velocitySlider.maxValue);
            initialVelocity = value;
            velocitySlider.value = value;
            horizontalSpeed = value;

            // 未运动时更新速度显示
            if (!GlobalUIController.Instance.IsMotionRunning())
            {
                UpdateSpeedDisplay(horizontalSpeed, 0, horizontalSpeed);
            }
        }
        else
        {
            velocityInput.text = initialVelocity.ToString("F2");
        }
    }

    /// <summary>
    /// 计算平抛运动速度
    /// </summary>
    private void CalculateSpeed()
    {
        // 运动时长
        float motionTime = Time.time - motionStartTime;
        // 水平速度（恒定）
        horizontalSpeed = initialVelocity;
        // 竖直速度（v = g*t）
        verticalSpeed = ScenePhysicsController.Instance.globalGravity * motionTime;
        // 合速度（矢量和）
        totalSpeed = Mathf.Sqrt(Mathf.Pow(horizontalSpeed, 2) + Mathf.Pow(verticalSpeed, 2));

        // 应用重力（模拟平抛）
        rb.AddForce(Vector3.down * ScenePhysicsController.Instance.globalGravity * objectMass, ForceMode.Force);
    }

    /// <summary>
    /// 更新速度显示文本
    /// </summary>
    private void UpdateSpeedDisplay(float vx, float vy, float vTotal)
    {
        // 保留两位小数显示
        horizontalSpeedText.text = vx.ToString("F2");
        verticalSpeedText.text = vy.ToString("F2");
        totalSpeedText.text = vTotal.ToString("F2");

        // 可选：设置文本颜色区分
        horizontalSpeedText.color = new Color(0, 0, 1);   // 蓝色
        verticalSpeedText.color = new Color(1, 0, 0);     // 红色
        totalSpeedText.color = new Color(0, 1, 0);       // 绿色
    }

    /// <summary>
    /// 开始物体运动
    /// </summary>
    public void StartMotion()
    {
        rb.isKinematic = false;
        rb.velocity = new Vector3(initialVelocity, 0, 0);
        motionStartTime = Time.time;
    }

    /// <summary>
    /// 暂停/继续物体运动
    /// </summary>
    public void PauseMotion(bool isPause)
    {
        rb.isKinematic = isPause;
    }

    /// <summary>
    /// 重置物体状态
    /// </summary>
    public void ResetObject()
    {
        // 1. 先取消Kinematic状态（避免设置速度警告）
        rb.isKinematic = false;

        // 2. 重置速度和角速度
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 3. 恢复Kinematic状态
        rb.isKinematic = true;

        // 回到初始位置
        transform.position = new Vector3(-5, 2, 0);

        // 恢复参数
        InitObjectParams();
    }
}
