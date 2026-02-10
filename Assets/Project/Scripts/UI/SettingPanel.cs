using UnityEngine;
using System.Collections.Generic;

public class SettingPanel : MonoBehaviour
{
    [Header("UI 模板")]
    public ParamSettingItem paramItemPrefab; // 参数预制体
    public Transform itemParent;             // Item 父节点

    private PhysicsObjectBase targetObject;
    private PhysicsObjectConfig objectConfig;
    private List<ParamSettingItem> spawnedItems = new List<ParamSettingItem>();

    // 绑定物理对象和配置
    public void BindObject(PhysicsObjectBase obj, PhysicsObjectConfig config)
    {
        targetObject = obj;
        objectConfig = config;
        ClearSpawnedItems(); // 清空原有Item
        SpawnSettingItems(); // 动态生成Item
    }

    // 动态生成参数设置项
    private void SpawnSettingItems()
    {
        if (objectConfig == null || targetObject == null) return;

        // 质量参数（原逻辑保留）
        if (objectConfig.showMass)
        {
            var massItem = SpawnParamItem("质量(kg)", 0.1f, 10f, targetObject.Mass);
            massItem.OnValueChanged += (value) => targetObject.SetMass(value);
        }

        // 初始速度相关配置（核心修复：确保修改单轴时保留其他轴值）
        if (objectConfig.showInitialVelocity)
        {
            // 水平速度（X轴）
            if (objectConfig.showInitialVelocityX)
            {
                var vxItem = SpawnParamItem("水平速度(m/s)", 0f, 20f, targetObject.InitialVelocity.x);
                vxItem.OnValueChanged += (value) =>
                {
                    // 读取当前完整的初始速度向量（保留Y、Z轴原值）
                    Vector3 currentVelocity = targetObject.InitialVelocity;
                    currentVelocity.x = value; // 仅修改X轴
                    targetObject.SetInitialVelocity(currentVelocity);

                    Debug.Log($"水平速度修改后：{currentVelocity}");
                };
            }

            // 竖直速度（Y轴）
            if (objectConfig.showInitialVelocityY)
            {
                var vyItem = SpawnParamItem("竖直速度(m/s)", -10f, 20f, targetObject.InitialVelocity.y);
                vyItem.OnValueChanged += (value) =>
                {
                    // 读取当前完整的初始速度向量（保留X、Z轴原值）
                    Vector3 currentVelocity = targetObject.InitialVelocity;
                    currentVelocity.y = value; // 仅修改Y轴
                    targetObject.SetInitialVelocity(currentVelocity);

                    Debug.Log($"竖直速度修改后：{currentVelocity}");
                };
            }

            // 补充Z轴速度配置（可选，根据需求扩展）
            if (objectConfig.showInitialVelocityZ)
            {
                var vzItem = SpawnParamItem("纵深速度(m/s)", -10f, 20f, targetObject.InitialVelocity.z);
                vzItem.OnValueChanged += (value) =>
                {
                    // 读取当前完整的初始速度向量（保留X、Y轴原值）
                    Vector3 currentVelocity = targetObject.InitialVelocity;
                    currentVelocity.z = value; // 仅修改Z轴
                    targetObject.SetInitialVelocity(currentVelocity);

                    Debug.Log($"纵深速度修改后：{currentVelocity}");
                };
            }
        }
    }

    // 生成参数项
    private ParamSettingItem SpawnParamItem(string label, float min, float max, float defaultValue)
    {
        var itemObj = Instantiate(paramItemPrefab, itemParent);
        itemObj.Initialize(label, min, max, defaultValue);
        spawnedItems.Add(itemObj);
        return itemObj;
    }

    // 清空生成的参数项
    private void ClearSpawnedItems()
    {
        foreach (var item in spawnedItems)
            Destroy(item.gameObject);
        spawnedItems.Clear();
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
