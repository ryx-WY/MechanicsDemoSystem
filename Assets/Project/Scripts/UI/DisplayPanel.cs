using UnityEngine;
using TMPro;

/// <summary>
/// 物体实时数据显示面板
/// </summary>
public class DisplayPanel : MonoBehaviour
{
    [Header("显示文本")]
    [SerializeField] private TextMeshProUGUI horizontalSpeedText;
    [SerializeField] private TextMeshProUGUI verticalSpeedText;
    [SerializeField] private TextMeshProUGUI totalSpeedText;

    private PhysicsObjectBase targetObject;

    public void BindObject(PhysicsObjectBase obj)
    {
        targetObject = obj;
    }

    void Update()
    {
        if (targetObject == null || !gameObject.activeSelf) return;
        if (Time.timeScale == 0) return; // 暂停时不更新显示

        Vector3 velocity = targetObject.GetCurrentVelocity();
        horizontalSpeedText.text = velocity.x.ToString("F2");
        verticalSpeedText.text = velocity.y.ToString("F2");
        totalSpeedText.text = targetObject.GetCurrentSpeed().ToString("F2");
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}