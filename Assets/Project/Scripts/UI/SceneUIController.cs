using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 平抛场景UI总控制器
/// </summary>
public class SceneUIController : MonoBehaviour
{
    [Header("面板预制体")]
    [SerializeField] private GameObject settingPanelPrefab;
    [SerializeField] private GameObject displayPanelPrefab;

    [Header("场景控制按钮")]
    [SerializeField] private Button controlBtn;
    [SerializeField] private TextMeshProUGUI controlBtnText;
    [SerializeField] private Button resetBtn;      // 重置小球
    [SerializeField] private Button clearBtn;      // 重置场景

    [Header("面板挂载点")]
    [SerializeField] private Transform panelParent;

    private SettingPanel settingPanel;
    private DisplayPanel displayPanel;
    private ProjectileObject projectileObject;

    void Start()
    {
        projectileObject = FindObjectOfType<ProjectileObject>();
        if (projectileObject == null)
        {
            Debug.LogError("场景中未找到ProjectileObject！");
            return;
        }

        SceneController.Instance.RegisterObject(projectileObject);

        CreatePanels();

        controlBtn.onClick.AddListener(OnControlClick);
        resetBtn.onClick.AddListener(OnResetClick);
        clearBtn.onClick.AddListener(OnClearClick);

        SwitchToSettingMode();
    }

    void CreatePanels()
    {
        GameObject settingObj = Instantiate(settingPanelPrefab, panelParent);
        settingPanel = settingObj.GetComponent<SettingPanel>();
        settingPanel.BindObject(projectileObject);

        GameObject displayObj = Instantiate(displayPanelPrefab, panelParent);
        displayPanel = displayObj.GetComponent<DisplayPanel>();
        displayPanel.BindObject(projectileObject);
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

        // 新增：重置相机跟踪
        Camera2DTrajectoryViewer camera = FindObjectOfType<Camera2DTrajectoryViewer>();
        if (camera != null) camera.ResetCamera();
    }

    void OnClearClick()
    {
        SceneController.Instance.ClearScene();
        controlBtnText.text = "开始";
        SwitchToSettingMode();

        // 新增：重置相机跟踪
        Camera2DTrajectoryViewer camera = FindObjectOfType<Camera2DTrajectoryViewer>();
        if (camera != null) camera.ResetCamera();
    }

    void SwitchToSettingMode()
    {
        settingPanel.Show();
        displayPanel.Hide();
    }

    void SwitchToDisplayMode()
    {
        settingPanel.Hide();
        displayPanel.Show();
    }
}