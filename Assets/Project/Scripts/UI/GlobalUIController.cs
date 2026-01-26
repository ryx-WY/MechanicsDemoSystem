//--------------------------------------------------------------------------------------------------
//E:\Unity\Project\MechanicsDemoSystem\Assets\Project\Scripts\UI\GlobalUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 全局UI控制器（统一管理UI显示/交互）
/// 挂载到Canvas
/// </summary>
public class GlobalUIController : MonoBehaviour
{
    // 单例模式
    public static GlobalUIController Instance;

    [Header("UI面板")]
    public GameObject sceneControlBar;       // 全局控制栏
    public GameObject sceneSettingPanel;     // 场景设置面板（重力）
    public GameObject objectSettingPanel;    // 物体设置面板（质量/速度）
    public GameObject objectDisplayPanel;    // 实时显示面板（速度）

    [Header("控制按钮")]
    public Button controlBtn;        // 开始/暂停/继续按钮
    public Button resetSceneBtn;     // 重置场景按钮（原重置按钮）
    public Button resetBallBtn;      // 新增：重置小球按钮
    public TMP_Text controlBtnText;  // 控制按钮文本

    private bool isMotionRunning = false;    // 运动是否运行
    private bool isMotionPaused = false;     // 运动是否暂停
    private bool isMotionLocked = false;     // 运动是否锁定（碰地面后）

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 初始化UI状态
        InitUIState();
        // 绑定按钮事件
        BindButtonEvents();
    }

    /// <summary>
    /// 初始化UI状态
    /// </summary>
    private void InitUIState()
    {
        sceneControlBar.SetActive(true);
        sceneSettingPanel.SetActive(true);
        objectSettingPanel.SetActive(true);
        objectDisplayPanel.SetActive(false);
        controlBtnText.text = "开始";
        controlBtn.interactable = true; // 按钮可用
        isMotionRunning = false;
        isMotionPaused = false;
        isMotionLocked = false;
    }

    /// <summary>
    /// 绑定所有按钮事件
    /// </summary>
    private void BindButtonEvents()
    {
        controlBtn.onClick.AddListener(OnControlClick);
        resetSceneBtn.onClick.AddListener(OnResetSceneClick);
        resetBallBtn.onClick.AddListener(OnResetBallClick);
    }

    /// <summary>
    /// 控制按钮逻辑（开始/暂停/继续）
    /// </summary>
    public void OnControlClick()
    {
        // 锁定状态下不响应
        if (isMotionLocked) return;

        // 1. 未运行：开始运动
        if (!isMotionRunning)
        {
            isMotionRunning = true;
            isMotionPaused = false;
            controlBtnText.text = "暂停";

            // 切换UI：隐藏设置面板，显示实时面板
            objectSettingPanel.SetActive(false);
            objectDisplayPanel.SetActive(true);

            // 通知小球开始运动
            FindObjectOfType<ObjectPhysicsController>().StartMotion();
        }
        // 2. 运行中未暂停：暂停运动
        else if (isMotionRunning && !isMotionPaused)
        {
            isMotionPaused = true;
            controlBtnText.text = "继续";

            // 通知小球暂停
            FindObjectOfType<ObjectPhysicsController>().PauseMotion(true);
        }
        // 3. 运行中已暂停：恢复运动
        else if (isMotionRunning && isMotionPaused)
        {
            isMotionPaused = false;
            controlBtnText.text = "暂停";

            // 通知小球恢复
            FindObjectOfType<ObjectPhysicsController>().PauseMotion(false);
        }
    }

    /// <summary>
    /// 重置场景（恢复所有默认参数）
    /// </summary>
    public void OnResetSceneClick()
    {
        // 重置UI状态
        InitUIState();
        // 重置场景重力
        FindObjectOfType<ScenePhysicsController>().ResetScene();
        // 重置小球参数
        FindObjectOfType<ObjectPhysicsController>().ResetObject();
        // 重置相机
        Camera2DTrajectoryViewer camera = FindObjectOfType<Camera2DTrajectoryViewer>();
        if (camera != null) camera.ResetCamera();
    }

    /// <summary>
    /// 重置小球（保留当前参数，仅重置位置+轨迹）
    /// </summary>
    public void OnResetBallClick()
    {
        // 解锁按钮
        isMotionLocked = false;
        controlBtn.interactable = true;
        // 重置UI状态
        InitUIState();
        // 通知小球重置
        FindObjectOfType<ObjectPhysicsController>().ResetBall();
    }

    /// <summary>
    /// 小球碰地面后停止运动，锁定按钮
    /// </summary>
    public void StopMotionOnGroundHit()
    {
        isMotionRunning = false;
        isMotionPaused = false;
        isMotionLocked = true;
        controlBtnText.text = "结束";
        controlBtn.interactable = false; // 禁用按钮
        
    }

    /// <summary>
    /// 重置小球后的UI状态恢复
    /// </summary>
    public void ResetBallUIState()
    {
        isMotionRunning = false;
        isMotionPaused = false;
        controlBtnText.text = "开始";
        objectSettingPanel.SetActive(true);
        objectDisplayPanel.SetActive(false);
        controlBtn.interactable = true;
    }

    // 外部获取运动状态
    public bool IsMotionRunning() => isMotionRunning;
    public bool IsMotionPaused() => isMotionPaused;
}
