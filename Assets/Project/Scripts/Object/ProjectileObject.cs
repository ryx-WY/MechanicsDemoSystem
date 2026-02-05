using UnityEngine;

public class ProjectileObject : PhysicsObjectBase
{
    [Header("平抛专用参数")]
    [SerializeField] private float initialHeight = 20f;

    private float motionStartTime;
    private float pausedTimeOffset = 0f; // 记录暂停累积时间
    private bool isInMotion = false;
    private TrajectoryDrawer trajectoryDrawer;

    // 缓存最终速度用于显示
    private Vector3 finalVelocity;
    private bool hasLanded = false;

    protected override void Awake()
    {
        base.Awake();
        trajectoryDrawer = GetComponent<TrajectoryDrawer>();
    }

    public override Vector3 GetCurrentVelocity()
    {
        // 如果已落地，返回缓存的最终速度
        if (hasLanded) return finalVelocity;

        if (!isInMotion) return new Vector3(InitialVelocity.x, 0, 0);

        // 计算实际运动时间（扣除暂停时间）
        float t = (Time.time - pausedTimeOffset) - motionStartTime;
        float vx = InitialVelocity.x;

        // 关键修复：向下为正方向（符合高中物理习惯）
        // 竖直向下速度 vy = g*t
        float vy = SceneController.Instance.GlobalGravity * t;

        return new Vector3(vx, vy, 0);
    }

    public override float GetCurrentSpeed()
    {
        Vector3 v = GetCurrentVelocity();
        return v.magnitude;
    }

    public void SetHorizontalVelocity(float vx)
    {
        SetInitialVelocity(new Vector3(vx, 0, 0));
    }

    public override void OnSimulationStart()
    {
        hasLanded = false;
        pausedTimeOffset = 0f;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = Mass;

        // 设置初速度：水平方向，竖直为0
        rb.velocity = new Vector3(InitialVelocity.x, 0, 0);

        motionStartTime = Time.time;
        isInMotion = true;

        if (trajectoryDrawer != null)
            trajectoryDrawer.StartCurrentTrajectory();
    }

    public override void OnSimulationReset()
    {
        // 修复Kinematic警告
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        transform.position = new Vector3(0, initialHeight, 0);
        isInMotion = false;
        hasLanded = false;

        if (trajectoryDrawer != null)
            trajectoryDrawer.StopCurrentTrajectory();
    }

    public override void OnSimulationClear()
    {
        SetMass(1f);
        SetHorizontalVelocity(5f);
        OnSimulationReset();

        if (trajectoryDrawer != null)
            trajectoryDrawer.ClearAllTrajectories();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;
            isInMotion = false;

            // 缓存落地瞬间的速度（此时竖直速度向下为正）
            finalVelocity = GetCurrentVelocity();

            // 停止物理运动
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;

            if (trajectoryDrawer != null)
                trajectoryDrawer.StopCurrentTrajectory();

            SceneController.Instance.OnObjectHitGround(this);
        }
    }

    // 关键修复：轨迹记录改用Update，与渲染帧同步，避免重影
    void Update()
    {
        if (isInMotion && Time.timeScale > 0 && trajectoryDrawer != null)
        {
            // 只有真正移动了才记录，防止静止时重复记录同一点
            TrajectoryManager.Instance.RecordTrajectoryPoint(transform.position);
        }
    }

    // 供SceneController调用，处理暂停时间累积
    public void AddPausedTime(float duration)
    {
        pausedTimeOffset += duration;
    }
}