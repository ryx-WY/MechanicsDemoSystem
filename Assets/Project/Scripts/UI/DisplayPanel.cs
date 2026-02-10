using UnityEngine;
using TMPro;

public class DisplayPanel : MonoBehaviour
{
    [Header("显示文本模板")]
    public TextMeshProUGUI speedTemplate; // 速度显示模板（复制用）
    public Transform textParent;          // 文本挂载点

    private PhysicsObjectBase targetObject;
    private PhysicsObjectConfig objectConfig;
    private TextMeshProUGUI horizontalSpeedText;
    private TextMeshProUGUI verticalSpeedText;
    private TextMeshProUGUI totalSpeedText;

    // 绑定物体与配置文件
    public void BindObject(PhysicsObjectBase obj, PhysicsObjectConfig config)
    {
        targetObject = obj;
        objectConfig = config;
        InitializeDisplayTexts(); // 初始化显示文本
    }

    // 根据配置初始化显示文本
    private void InitializeDisplayTexts()
    {
        // 清除原有文本（保留模板，隐藏模板）
        foreach (Transform child in textParent)
        {
            if (child != speedTemplate.transform)
                Destroy(child.gameObject);
        }
        speedTemplate.gameObject.SetActive(false);

        // 1. 水平速度（X轴）
        if (objectConfig.showHorizontalSpeed)
        {
            horizontalSpeedText = CreateSpeedText("水平速度：");
        }

        // 2. 竖直速度（Y轴）
        if (objectConfig.showVerticalSpeed)
        {
            verticalSpeedText = CreateSpeedText("竖直速度：");
        }

        // 3. 合速度
        if (objectConfig.showTotalSpeed)
        {
            totalSpeedText = CreateSpeedText("合速度：");
        }
    }

    // 创建单个速度显示文本
    private TextMeshProUGUI CreateSpeedText(string prefix)
    {
        var textObj = Instantiate(speedTemplate, textParent);
        textObj.gameObject.SetActive(true);
        textObj.text = $"{prefix}0.00";
        return textObj;
    }

    void Update()
    {
        if (targetObject == null || !gameObject.activeSelf || Time.timeScale == 0)
            return;

        Vector3 velocity = targetObject.GetCurrentVelocity();
        float totalSpeed = targetObject.GetCurrentSpeed();

        // 按需更新显示
        if (horizontalSpeedText != null)
            horizontalSpeedText.text = $"水平速度：{velocity.x:F2} m/s";
        if (verticalSpeedText != null)
            verticalSpeedText.text = $"竖直速度：{velocity.y:F2} m/s";
        if (totalSpeedText != null)
            totalSpeedText.text = $"合速度：{totalSpeed:F2} m/s";
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
