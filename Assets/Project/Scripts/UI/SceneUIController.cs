using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneUIController : MonoBehaviour
{
    [Header("面板预制体")]
    public GameObject settingPanelPrefab;
    public GameObject displayPanelPrefab;
    [Header("场景控制按钮")]
    public Button controlBtn;
    public TextMeshProUGUI controlBtnText;
    public Button resetBtn;
    public Button clearBtn;
    [Header("面板挂载点")]
    public Transform panelParent;

    private SettingPanel settingPanel;
    private DisplayPanel displayPanel;
    private PhysicsBall physicsBall;
    private PhysicsObjectConfig ballConfig;

    void Start()
    {
        // 查找通用小球（替代原 ProjectileObject）
        physicsBall = FindObjectOfType<PhysicsBall>();
        if (physicsBall == null)
        {
            Debug.LogError("场景中未找到 PhysicsBall！");
            return;
        }

        // 获取小球的配置文件
        ballConfig = physicsBall.objectConfig;
        // 修复：通过 ScriptableObject 调用 CreateInstance
        if (ballConfig == null)
        {
            ballConfig = ScriptableObject.CreateInstance<PhysicsObjectConfig>();
            physicsBall.objectConfig = ballConfig;
        }

        // 注册物体到场景控制器
        SceneController.Instance.RegisterObject(physicsBall);
        // 创建动态面板
        CreateDynamicPanels();
        // 绑定按钮事件
        BindButtonEvents();
        // 默认进入设置模式
        SwitchToSettingMode();
    }

    // 创建动态面板（设置+显示）
    private void CreateDynamicPanels()
    {
        // 1. 设置面板
        GameObject settingObj = Instantiate(settingPanelPrefab, panelParent);
        settingPanel = settingObj.GetComponent<SettingPanel>();
        settingPanel.BindObject(physicsBall, ballConfig);

        // 2. 显示面板
        GameObject displayObj = Instantiate(displayPanelPrefab, panelParent);
        displayPanel = displayObj.GetComponent<DisplayPanel>();
        displayPanel.BindObject(physicsBall, ballConfig);
    }

    // 绑定按钮事件
    private void BindButtonEvents()
    {
        controlBtn.onClick.AddListener(OnControlClick);
        resetBtn.onClick.AddListener(OnResetClick);
        clearBtn.onClick.AddListener(OnClearClick);
    }

    void OnControlClick()
    {
        var state = SceneController.Instance.CurrentState;
        switch (state)
        {
            case SimulationState.Idle:
            case SimulationState.Finished:
                SceneController.Instance.StartSimulation();
                controlBtnText.text = "暂停";
                SwitchToDisplayMode();
                break;
            case SimulationState.Running:
                SceneController.Instance.PauseSimulation();
                controlBtnText.text = "继续";
                break;
            case SimulationState.Paused:
                SceneController.Instance.ResumeSimulation();
                controlBtnText.text = "暂停";
                break;
        }
    }

    void OnResetClick()
    {
        SceneController.Instance.ResetSimulation();
        controlBtnText.text = "开始";
        SwitchToSettingMode();

        // 关键修复：重新绑定物体，让 SettingPanel 读取最新默认参数（X=5）
        settingPanel.BindObject(physicsBall, ballConfig);

        // 重置相机
        var camera = FindObjectOfType<Camera2DTrajectoryViewer>();
        camera?.ResetCamera();
    }

    void OnClearClick()
    {
        SceneController.Instance.ClearScene();
        controlBtnText.text = "开始";
        SwitchToSettingMode();

        // 关键修复：重新绑定物体，同步默认参数
        settingPanel.BindObject(physicsBall, ballConfig);

        // 重置相机
        var camera = FindObjectOfType<Camera2DTrajectoryViewer>();
        camera?.ResetCamera();
    }

    void SwitchToSettingMode()
    {
        settingPanel?.Show();
        displayPanel?.Hide();
    }

    void SwitchToDisplayMode()
    {
        settingPanel?.Hide();
        displayPanel?.Show();
    }
}
