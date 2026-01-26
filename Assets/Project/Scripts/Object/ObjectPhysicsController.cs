using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectPhysicsController : MonoBehaviour
{
    [Header("物理参数")]
    public Rigidbody rb;

    [Header("初始参数")]
    public float objectMass = 1f;
    public float initialVelocity = 5f;

    [Header("UI引用 - 参数设置")]
    public Slider massSlider;
    public TMP_InputField massInput;
    public Slider velocitySlider;
    public TMP_InputField velocityInput;

    [Header("UI引用 - 实时速度显示（文本赋值）")]
    public TMP_Text horizontalSpeedText;
    public TMP_Text verticalSpeedText;
    public TMP_Text totalSpeedText;

    // 运动相关
    private float motionStartTime;
    private float horizontalSpeed;
    private float verticalSpeed;
    private float totalSpeed;
    public TrajectoryDrawer trajectoryDrawer;

    // 暂停/恢复相关
    private Vector3 cachedVelocity;
    private Vector3 cachedAngularVelocity;
    private float pauseStartTime;
    private float totalPausedTime;

    void Start()
    {
        InitObjectParams();
        BindSliderEvents();
        rb.isKinematic = true;
        rb.useGravity = false;
        UpdateSpeedDisplay(initialVelocity, 0, initialVelocity);
        totalPausedTime = 0;
    }

    // 碰撞检测：小球碰到地面停止
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            // 停止当前轨迹记录
            trajectoryDrawer.StopCurrentTrajectory();
            // 通知UI锁定按钮
            GlobalUIController.Instance.StopMotionOnGroundHit();
        }
    }

    void Update()
    {
        if (GlobalUIController.Instance.IsMotionRunning() && !GlobalUIController.Instance.IsMotionPaused())
        {
            CalculateSpeed();
            UpdateSpeedDisplay(horizontalSpeed, verticalSpeed, totalSpeed);
        }
    }

   

    /// <summary>
    /// 计算平抛运动速度
    /// </summary>
    private void CalculateSpeed()
    {
        float motionTime = (Time.time - totalPausedTime) - motionStartTime;
        horizontalSpeed = initialVelocity;
        verticalSpeed = ScenePhysicsController.Instance.globalGravity * motionTime;
        totalSpeed = Mathf.Sqrt(Mathf.Pow(horizontalSpeed, 2) + Mathf.Pow(verticalSpeed, 2));

        if (!rb.isKinematic)
        {
            rb.AddForce(Vector3.down * ScenePhysicsController.Instance.globalGravity * objectMass, ForceMode.Force);
        }
    }

    /// <summary>
    /// 更新速度显示文本
    /// </summary>
    private void UpdateSpeedDisplay(float vx, float vy, float vTotal)
    {
        horizontalSpeedText.text = vx.ToString("F2");
        verticalSpeedText.text = vy.ToString("F2");
        totalSpeedText.text = vTotal.ToString("F2");
        horizontalSpeedText.color = Color.blue;
        verticalSpeedText.color = Color.red;
        totalSpeedText.color = Color.green;
    }

    /// <summary>
    /// 开始运动（关键修改：启动新轨迹记录）
    /// </summary>
    public void StartMotion()
    {
        rb.isKinematic = false;
        rb.velocity = new Vector3(initialVelocity, 0, 0);
        motionStartTime = Time.time;
        totalPausedTime = 0;
        // 通知轨迹管理器开始记录新轨迹
        trajectoryDrawer.StartCurrentTrajectory();
    }

    /// <summary>
    /// 暂停/恢复运动
    /// </summary>
    public void PauseMotion(bool isPause)
    {
        if (isPause)
        {
            cachedVelocity = rb.velocity;
            cachedAngularVelocity = rb.angularVelocity;
            pauseStartTime = Time.time;
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
            rb.velocity = cachedVelocity;
            rb.angularVelocity = cachedAngularVelocity;
            totalPausedTime += Time.time - pauseStartTime;
        }
    }

    /// <summary>
    /// 重置小球（仅重置位置，保留历史轨迹）
    /// </summary>
    public void ResetBall()
    {
        // 停止当前轨迹记录（如果正在记录）
        trajectoryDrawer.StopCurrentTrajectory();

        // 修复核心：先取消运动学状态，再设置速度/角速度（避免警告）
        rb.isKinematic = false; // 临时取消kinematic
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero; // 此时刚体非kinematic，设置有效
        rb.isKinematic = true; // 恢复kinematic

        // 重置小球位置
        transform.position = new Vector3(0, 20, 0); // 初始位置

        // 重置速度显示和暂停变量
        UpdateSpeedDisplay(initialVelocity, 0, initialVelocity);
        totalPausedTime = 0;
        cachedVelocity = Vector3.zero;

        // 通知UI解锁
        GlobalUIController.Instance.ResetBallUIState();
    }

    /// <summary>
    /// 重置物体（重置场景时调用，清空所有轨迹）
    /// </summary>
    public void ResetObject()
    {
        // 清空所有历史轨迹
        trajectoryDrawer.ClearAllTrajectories();

        // 修复核心：调整顺序，先取消kinematic再设置速度
        rb.isKinematic = false; // 临时取消运动学状态
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero; // 此时设置不会触发警告
        rb.isKinematic = true; // 恢复运动学状态

        // 重置小球位置
        transform.position = new Vector3(0, 20, 0);

        // 恢复默认参数
        objectMass = 1f;
        initialVelocity = 5f;
        InitObjectParams();
        UpdateSpeedDisplay(initialVelocity, 0, initialVelocity);


        // 重置暂停变量
        totalPausedTime = 0;
        cachedVelocity = Vector3.zero;
        objectMass = 1f;
        initialVelocity = 5f;
}


    private void InitObjectParams()
    {
        objectMass = massSlider.value;
        massInput.text = objectMass.ToString("F2");
        rb.mass = objectMass;

        initialVelocity = velocitySlider.value;
        velocityInput.text = initialVelocity.ToString("F2");
        horizontalSpeed = initialVelocity;
    }

    private void BindSliderEvents()
    {
        massSlider.onValueChanged.AddListener(OnMassChanged);
        velocitySlider.onValueChanged.AddListener(OnVelocityChanged);
        massInput.onEndEdit.AddListener(OnMassInputChanged);
        velocityInput.onEndEdit.AddListener(OnVelocityInputChanged);
    }

    public void OnMassChanged(float value)
    {
        objectMass = value;
        massInput.text = value.ToString("F2");
        rb.mass = value;
    }

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

    public void OnVelocityChanged(float value)
    {
        initialVelocity = value;
        velocityInput.text = value.ToString("F2");
        horizontalSpeed = value;
        if (!GlobalUIController.Instance.IsMotionRunning())
        {
            UpdateSpeedDisplay(horizontalSpeed, 0, horizontalSpeed);
        }
    }

    private void OnVelocityInputChanged(string inputValue)
    {
        if (float.TryParse(inputValue, out float value))
        {
            value = Mathf.Clamp(value, velocitySlider.minValue, velocitySlider.maxValue);
            initialVelocity = value;
            velocitySlider.value = value;
            horizontalSpeed = value;
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
}
