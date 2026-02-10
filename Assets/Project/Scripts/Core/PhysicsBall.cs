using UnityEngine;

public class PhysicsBall : PhysicsObjectBase
{
    [Header("初始条件（Inspector 直接配置）")]
    public Vector3 initialPosition = new(0, 20, 0); // 初始位置（默认平抛高度）
    public Vector3 initialVelocity = new(5, 0, 0);  // 初始速度（默认水平，平抛）

    [Header("配置文件")]
    public PhysicsObjectConfig objectConfig;

    private float motionStartTime;
    private float pausedTimeOffset = 0f;
    private bool isInMotion = false;
    private Vector3 finalVelocity;
    private bool hasLanded = false;

  
    protected override void Awake()
    {
        base.Awake();
        if (objectConfig == null)
            objectConfig = ScriptableObject.CreateInstance<PhysicsObjectConfig>();
        // 同步 Inspector 中配置的默认初始速度 (5,0,0) 到基类
        base.SetInitialVelocity(initialVelocity);
    }
    // 实时速度计算（通用力学公式：v = v0 + a*t）
    public override Vector3 GetCurrentVelocity()
    {
        if (hasLanded) return finalVelocity;
        if (!isInMotion) return initialVelocity;
        float t = (Time.time - pausedTimeOffset) - motionStartTime;
        // 核心修复：重力加速度 Y 轴设为负（与 SceneController 重力方向一致，竖直向下）
        Vector3 gravity = new(0, -SceneController.Instance.GlobalGravity, 0);
        return initialVelocity + gravity * t;
    }

    public override float GetCurrentSpeed() => GetCurrentVelocity().magnitude;

    // 质量设置（继承基类，保持兼容）
    public override void SetMass(float value)
    {
        base.SetMass(value);
        if (rb != null) rb.mass = value;
    }

    // 初速度设置（支持全方向，替代原水平速度专用方法）
    
    public override void SetInitialVelocity(Vector3 velocity)
    {
        // 关键修复：同步更新基类的 _initialVelocity（让 SettingPanel 能读取到正确值）
        base.SetInitialVelocity(velocity);
        // 保留原有逻辑：更新自身初始速度字段（Inspector 显示+刚体速度同步）
        initialVelocity = velocity;
        if (rb != null && !rb.isKinematic)
        {
            rb.velocity = initialVelocity;
        }
    }




    // 补充：确认 OnSimulationStart 方法中会设置刚体速度（模拟启动时执行，此时isKinematic=false）
    public override void OnSimulationStart()
    {
        hasLanded = false;
        pausedTimeOffset = 0f;
        rb.isKinematic = false; // 关键：模拟启动时关闭运动学
        rb.useGravity = true;
        rb.mass = Mass;

        // 核心：模拟启动时，用保存的initialVelocity设置刚体速度（此时无警告）
        rb.velocity = initialVelocity;
        motionStartTime = Time.time;
        isInMotion = true;

        if (GetComponent<TrajectoryDrawer>() != null)
            GetComponent<TrajectoryDrawer>().StartCurrentTrajectory();
    }


    // 重置模拟（恢复初始位置和状态）
    public override void OnSimulationReset()
    {
        rb.isKinematic = true; // 重置后立即设为运动学
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
        isInMotion = false;
        hasLanded = false;

        var drawer = GetComponent<TrajectoryDrawer>();
        if (drawer != null) drawer.StopCurrentTrajectory();
    }

    // 清空模拟（恢复默认参数）
    public override void OnSimulationClear()
    {
        SetMass(1f);
        SetInitialVelocity(new Vector3(5, 0, 0));
        OnSimulationReset();

        var drawer = GetComponent<TrajectoryDrawer>();
        if (drawer != null) drawer.ClearAllTrajectories();
    }

    // 碰撞地面处理
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;
            isInMotion = false;
            finalVelocity = GetCurrentVelocity();
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;

            var drawer = GetComponent<TrajectoryDrawer>();
            if (drawer != null) drawer.StopCurrentTrajectory();

            SceneController.Instance.OnObjectHitGround(this);
        }
    }

    // 轨迹记录（与渲染帧同步）
    void Update()
    {
        if (isInMotion && Time.timeScale > 0)
            TrajectoryManager.Instance.RecordTrajectoryPoint(transform.position);
    }

    // 暂停时间累积
    public void AddPausedTime(float duration) => pausedTimeOffset += duration;
}
