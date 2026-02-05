using UnityEngine;

/// <summary>
/// 所有物理物体的抽象基类
/// </summary>
public abstract class PhysicsObjectBase : MonoBehaviour
{
    [Header("物理核心组件")]
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected LineRenderer trajectoryLine; // 轨迹线

    // 私有属性（外部不可直接修改）
    private float _mass = 1f;
    private Vector3 _initialVelocity = Vector3.zero;
    private bool _isInitialized = false;

    // 公有访问器（UI通过这里读取）
    public float Mass => _mass;
    public Vector3 InitialVelocity => _initialVelocity;
    public Rigidbody Rigidbody => rb;
    public bool IsInitialized => _isInitialized;

    // 实时数据访问（供DisplayPanel读取）
    public abstract Vector3 GetCurrentVelocity();
    public abstract float GetCurrentSpeed();

    // 属性设置器（供SettingPanel调用）
    public virtual void SetMass(float value)
    {
        _mass = Mathf.Max(0.01f, value);
        if (rb != null) rb.mass = _mass;
    }

    public virtual void SetInitialVelocity(Vector3 velocity)
    {
        _initialVelocity = velocity;
    }

    // 生命周期方法（由SceneController统一调用）
    public abstract void OnSimulationStart();  // 开始运动
    public abstract void OnSimulationReset();  // 重置（保留参数）
    public abstract void OnSimulationClear();  // 清空（恢复默认）

    protected virtual void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (trajectoryLine == null)
            trajectoryLine = GetComponent<LineRenderer>();

        // 初始状态：运动学锁定（等待开始）
        rb.isKinematic = true;
        rb.useGravity = false;
    }
}