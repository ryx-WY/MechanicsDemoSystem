using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsObjectConfig", menuName = "力学系统/物体配置文件")]
public class PhysicsObjectConfig : ScriptableObject
{
    [Header("设置面板参数配置")]
    public bool showMass = true;          // 是否显示质量
    public bool showInitialVelocity = true; // 是否显示初速度
    public bool showInitialVelocityX = true; // 初速度X轴（水平）
    public bool showInitialVelocityY = false; // 初速度Y轴（竖直）
    public bool showInitialVelocityZ = false; // 初速度Z轴（3D场景用）

    [Header("显示面板参数配置")]
    public bool showHorizontalSpeed = true; // 水平速度（X轴）
    public bool showVerticalSpeed = true;   // 竖直速度（Y轴）
    public bool showTotalSpeed = true;      // 合速度
}
