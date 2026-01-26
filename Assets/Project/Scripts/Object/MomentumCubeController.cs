using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 动量守恒场景-双方块物理控制器（完全弹性碰撞）
/// 支持两个方块的参数独立控制、实时数据计算、运动状态管理
/// </summary>
public class MomentumCubeController : MonoBehaviour
{
    [Header("物理组件-双方块")]
    public Rigidbody smallRb; // 小方块刚体
    public Rigidbody bigRb;   // 大方块刚体

    [Header("小方块参数")]
    public float smallCubeMass = 1f;          // 质量(kg)
    public float smallCubeInitialVelocity = 3f; // 初始速度(m/s，正负代表方向)

    [Header("大方块参数")]
    public float bigCubeMass = 2f;            // 质量(kg)
    public float bigCubeInitialVelocity = -1f; // 初始速度(m/s，正负代表方向)

    [Header("UI绑定-小方块设置")]
    public Slider smallMassSlider;
    public TMP_InputField smallMassInput;
    public Slider smallVelocitySlider;
    public TMP_InputField smallVelocityInput;

    [Header("UI绑定-大方块设置")]
    public Slider bigMassSlider;
    public TMP_InputField bigMassInput;
    public Slider bigVelocitySlider;
    public TMP_InputField bigVelocityInput;

    [Header("UI绑定-实时数据显示")]
    // 小方块显示
    public TMP_Text smallMassDisplay;
    public TMP_Text smallVelocityDisplay;
    public TMP_Text smallMomentumDisplay;
    // 大方块显示
    public TMP_Text bigMassDisplay;
    public TMP_Text bigVelocityDisplay;
    public TMP_Text bigMomentumDisplay;
    // 总动量显示
    public TMP_Text totalMomentumDisplay;

    [Header("运动状态配置")]
    private float pauseStartTime;
    private float totalPausedTime;
    // 缓存两个方块的速度（暂停用）
    private Vector3 smallCachedVelocity;
    private Vector3 smallCachedAngularVelocity;
    private Vector3 bigCachedVelocity;
    private Vector3 bigCachedAngularVelocity;
    // 实时计算数据
    private float smallCubeCurrentVelocity;
    private float bigCubeCurrentVelocity;
    private float smallCubeMomentum;
    private float bigCubeMomentum;
    private float totalMomentum;

    void Start()
    {
        // 初始化参数与UI绑定
        InitCubeParams();
        BindSliderEvents();
        // 初始状态：运动学锁定（不可动）
        SetRigidbodyKinematic(true);
        // 初始化显示
        UpdateDisplay();
        totalPausedTime = 0;
    }

    #region 初始化与参数配置
    /// <summary>
    /// 初始化双方块参数（同步滑块、输入框、刚体质量）
    /// </summary>
    private void InitCubeParams()
    {
        // 小方块参数初始化
        smallMassSlider.value = smallCubeMass;
        smallMassInput.text = smallCubeMass.ToString("F2");
        smallVelocitySlider.value = smallCubeInitialVelocity;
        smallVelocityInput.text = smallCubeInitialVelocity.ToString("F2");
        smallRb.mass = smallCubeMass;

        // 大方块参数初始化
        bigMassSlider.value = bigCubeMass;
        bigMassInput.text = bigCubeMass.ToString("F2");
        bigVelocitySlider.value = bigCubeInitialVelocity;
        bigVelocityInput.text = bigCubeInitialVelocity.ToString("F2");
        bigRb.mass = bigCubeMass;

        // 初始实时速度（未运动时显示初始速度）
        smallCubeCurrentVelocity = smallCubeInitialVelocity;
        bigCubeCurrentVelocity = bigCubeInitialVelocity;
    }

    /// <summary>
    /// 绑定所有滑块和输入框事件
    /// </summary>
    private void BindSliderEvents()
    {
        // 小方块质量
        smallMassSlider.onValueChanged.AddListener(OnSmallMassChanged);
        smallMassInput.onEndEdit.AddListener(OnSmallMassInputChanged);
        // 小方块速度
        smallVelocitySlider.onValueChanged.AddListener(OnSmallVelocityChanged);
        smallVelocityInput.onEndEdit.AddListener(OnSmallVelocityInputChanged);
        // 大方块质量
        bigMassSlider.onValueChanged.AddListener(OnBigMassChanged);
        bigMassInput.onEndEdit.AddListener(OnBigMassInputChanged);
        // 大方块速度
        bigVelocitySlider.onValueChanged.AddListener(OnBigVelocityChanged);
        bigVelocityInput.onEndEdit.AddListener(OnBigVelocityInputChanged);
    }
    #endregion

    #region UI事件回调（参数修改）
    // 小方块质量滑块变化
    private void OnSmallMassChanged(float value)
    {
        smallCubeMass = value;
        smallMassInput.text = value.ToString("F2");
        smallRb.mass = value;
        if (!MomentumGlobalUIController.Instance.IsMotionRunning())
        {
            UpdateDisplay();
        }
    }

    // 小方块质量输入框变化
    private void OnSmallMassInputChanged(string inputValue)
    {
        if (float.TryParse(inputValue, out float value))
        {
            value = Mathf.Clamp(value, smallMassSlider.minValue, smallMassSlider.maxValue);
            smallCubeMass = value;
            smallMassSlider.value = value;
            smallRb.mass = value;
            if (!MomentumGlobalUIController.Instance.IsMotionRunning())
            {
                UpdateDisplay();
            }
        }
        else
        {
            smallMassInput.text = smallCubeMass.ToString("F2");
        }
    }

    // 小方块速度滑块变化
    private void OnSmallVelocityChanged(float value)
    {
        smallCubeInitialVelocity = value;
        smallVelocityInput.text = value.ToString("F2");
        if (!MomentumGlobalUIController.Instance.IsMotionRunning())
        {
            smallCubeCurrentVelocity = value;
            UpdateDisplay();
        }
    }

    // 小方块速度输入框变化（支持正负值）
    private void OnSmallVelocityInputChanged(string inputValue)
    {
        if (float.TryParse(inputValue, out float value))
        {
            value = Mathf.Clamp(value, smallVelocitySlider.minValue, smallVelocitySlider.maxValue);
            smallCubeInitialVelocity = value;
            smallVelocitySlider.value = value;
            if (!MomentumGlobalUIController.Instance.IsMotionRunning())
            {
                smallCubeCurrentVelocity = value;
                UpdateDisplay();
            }
        }
        else
        {
            smallVelocityInput.text = smallCubeInitialVelocity.ToString("F2");
        }
    }

    // 大方块质量滑块变化
    private void OnBigMassChanged(float value)
    {
        bigCubeMass = value;
        bigMassInput.text = value.ToString("F2");
        bigRb.mass = value;
        if (!MomentumGlobalUIController.Instance.IsMotionRunning())
        {
            UpdateDisplay();
        }
    }

    // 大方块质量输入框变化
    private void OnBigMassInputChanged(string inputValue)
    {
        if (float.TryParse(inputValue, out float value))
        {
            value = Mathf.Clamp(value, bigMassSlider.minValue, bigMassSlider.maxValue);
            bigCubeMass = value;
            bigMassSlider.value = value;
            bigRb.mass = value;
            if (!MomentumGlobalUIController.Instance.IsMotionRunning())
            {
                UpdateDisplay();
            }
        }
        else
        {
            bigMassInput.text = bigCubeMass.ToString("F2");
        }
    }

    // 大方块速度滑块变化
    private void OnBigVelocityChanged(float value)
    {
        bigCubeInitialVelocity = value;
        bigVelocityInput.text = value.ToString("F2");
        if (!MomentumGlobalUIController.Instance.IsMotionRunning())
        {
            bigCubeCurrentVelocity = value;
            UpdateDisplay();
        }
    }

    // 大方块速度输入框变化（支持正负值）
    private void OnBigVelocityInputChanged(string inputValue)
    {
        if (float.TryParse(inputValue, out float value))
        {
            value = Mathf.Clamp(value, bigVelocitySlider.minValue, bigVelocitySlider.maxValue);
            bigCubeInitialVelocity = value;
            bigVelocitySlider.value = value;
            if (!MomentumGlobalUIController.Instance.IsMotionRunning())
            {
                bigCubeCurrentVelocity = value;
                UpdateDisplay();
            }
        }
        else
        {
            bigVelocityInput.text = bigCubeInitialVelocity.ToString("F2");
        }
    }
    #endregion

    #region 运动状态控制（开始/暂停/重置）
    /// <summary>
    /// 开始运动（给两个方块赋初始速度）
    /// </summary>
    public void StartMotion()
    {
        SetRigidbodyKinematic(false);
        // 赋值初始速度（x轴方向，正负代表运动方向）
        smallRb.velocity = new Vector3(smallCubeInitialVelocity, 0, 0);
        bigRb.velocity = new Vector3(bigCubeInitialVelocity, 0, 0);
        totalPausedTime = 0;
    }

    /// <summary>
    /// 暂停/恢复运动（同步控制两个方块）
    /// </summary>
    public void PauseMotion(bool isPause)
    {
        if (isPause)
        {
            // 缓存速度和角速度
            smallCachedVelocity = smallRb.velocity;
            smallCachedAngularVelocity = smallRb.angularVelocity;
            bigCachedVelocity = bigRb.velocity;
            bigCachedAngularVelocity = bigRb.angularVelocity;
            pauseStartTime = Time.time;
            SetRigidbodyKinematic(true);
        }
        else
        {
            SetRigidbodyKinematic(false);
            // 恢复速度和角速度
            smallRb.velocity = smallCachedVelocity;
            smallRb.angularVelocity = smallCachedAngularVelocity;
            bigRb.velocity = bigCachedVelocity;
            bigRb.angularVelocity = bigCachedAngularVelocity;
            totalPausedTime += Time.time - pauseStartTime;
        }
    }

    /// <summary>
    /// 重置方块（保留当前参数，仅重置位置和运动状态）
    /// </summary>
    public void ResetBall()
    {
        // 重置位置（水平对称，避免重叠）
        smallRb.transform.position = new Vector3(-5f, 0f, 0f);
        bigRb.transform.position = new Vector3(5f, 0f, 0f);

        // 重置运动状态
        SetRigidbodyKinematic(false);
        smallRb.velocity = Vector3.zero;
        smallRb.angularVelocity = Vector3.zero;
        bigRb.velocity = Vector3.zero;
        bigRb.angularVelocity = Vector3.zero;
        SetRigidbodyKinematic(true);

        // 重置实时显示（恢复初始速度）
        smallCubeCurrentVelocity = smallCubeInitialVelocity;
        bigCubeCurrentVelocity = bigCubeInitialVelocity;
        UpdateDisplay();

        // 重置暂停变量
        totalPausedTime = 0;
        smallCachedVelocity = Vector3.zero;
        bigCachedVelocity = Vector3.zero;

        // 通知UI恢复状态
        MomentumGlobalUIController.Instance.ResetBallUIState();
    }

    /// <summary>
    /// 重置场景（恢复默认参数，清空所有状态）
    /// </summary>
    public void ResetObject()
    {
        // 重置位置
        smallRb.transform.position = new Vector3(-5f, 0f, 0f);
        bigRb.transform.position = new Vector3(5f, 0f, 0f);

        // 恢复默认参数
        smallCubeMass = 1f;
        smallCubeInitialVelocity = 3f;
        bigCubeMass = 2f;
        bigCubeInitialVelocity = -1f;

        // 重新初始化参数和UI
        InitCubeParams();
        SetRigidbodyKinematic(false);
        smallRb.velocity = Vector3.zero;
        smallRb.angularVelocity = Vector3.zero;
        bigRb.velocity = Vector3.zero;
        bigRb.angularVelocity = Vector3.zero;
        SetRigidbodyKinematic(true);

        // 重置显示和暂停变量
        UpdateDisplay();
        totalPausedTime = 0;
        smallCachedVelocity = Vector3.zero;
        bigCachedVelocity = Vector3.zero;
    }

    /// <summary>
    /// 统一设置两个刚体的运动学状态
    /// </summary>
    private void SetRigidbodyKinematic(bool isKinematic)
    {
        smallRb.isKinematic = isKinematic;
        bigRb.isKinematic = isKinematic;
    }
    #endregion

    #region 数据计算与显示
    void Update()
    {
        // 运动中且未暂停时，实时计算并更新数据
        if (MomentumGlobalUIController.Instance.IsMotionRunning() && !MomentumGlobalUIController.Instance.IsMotionPaused())
        {
            CalculateMomentum();
            UpdateDisplay();
        }
    }

    /// <summary>
    /// 计算动量（动量=质量×速度，矢量，支持正负）
    /// </summary>
    private void CalculateMomentum()
    {
        // 获取当前速度（仅x轴，水平碰撞）
        smallCubeCurrentVelocity = smallRb.velocity.x;
        bigCubeCurrentVelocity = bigRb.velocity.x;

        // 计算动量（kg·m/s）
        smallCubeMomentum = smallCubeMass * smallCubeCurrentVelocity;
        bigCubeMomentum = bigCubeMass * bigCubeCurrentVelocity;

        // 总动量（矢量和）
        totalMomentum = smallCubeMomentum + bigCubeMomentum;
    }

    /// <summary>
    /// 更新UI实时显示（保留2位小数，颜色区分）
    /// </summary>
    private void UpdateDisplay()
    {
        // 小方块显示
        smallMassDisplay.text = smallCubeMass.ToString("F2");
        smallVelocityDisplay.text = smallCubeCurrentVelocity.ToString("F2");
        smallMomentumDisplay.text = smallCubeMomentum.ToString("F2");

        // 大方块显示
        bigMassDisplay.text = bigCubeMass.ToString("F2");
        bigVelocityDisplay.text = bigCubeCurrentVelocity.ToString("F2");
        bigMomentumDisplay.text = bigCubeMomentum.ToString("F2");

        // 总动量显示
        totalMomentumDisplay.text = totalMomentum.ToString("F2");

        // 颜色区分（便于观察）
        smallVelocityDisplay.color = Color.blue;
        bigVelocityDisplay.color = Color.red;
        smallMomentumDisplay.color = Color.green;
        bigMomentumDisplay.color = Color.yellow;
        totalMomentumDisplay.color = Color.magenta;
    }
    #endregion
}
