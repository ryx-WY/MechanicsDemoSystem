using UnityEngine;

/// <summary>
/// 物体参数设置面板
/// </summary>
public class SettingPanel : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private ParamSettingItem massItem;
    [SerializeField] private ParamSettingItem velocityItem;

    private PhysicsObjectBase targetObject;

    // 初始化时绑定目标物体
    public void BindObject(PhysicsObjectBase obj)
    {
        targetObject = obj;

        // 初始化UI数值（从物体读取当前值）
        massItem.Initialize("质量(kg)", 0.1f, 10f, obj.Mass);
        velocityItem.Initialize("水平速度(m/s)", 0f, 20f, obj.InitialVelocity.x);

        // 绑定修改事件（修改物体属性）
        massItem.OnValueChanged += (value) => obj.SetMass(value);
        velocityItem.OnValueChanged += (value) => {
            if (obj is ProjectileObject projectile)
                projectile.SetHorizontalVelocity(value);
        };
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}