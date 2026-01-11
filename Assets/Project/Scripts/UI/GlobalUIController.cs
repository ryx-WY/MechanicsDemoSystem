//--------------------------------------------------------------------------------------------------
//E:\Unity\Project\MechanicsDemoSystem\Assets\Project\Scripts\UI\GlobalUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 全局UI控制器（统一管理所有UI的显示/隐藏）
/// 挂载到Canvas上
/// </summary>
public class GlobalUIController : MonoBehaviour
{
    // 单例模式，全局唯一
    public static GlobalUIController Instance;

    [Header("UI引用")]
    public GameObject sceneControlBar;       // 全局控制栏
    public GameObject sceneSettingPanel;     // 场景参数面板
    public GameObject objectSettingPanel;    // 物体可配置面板
    public GameObject objectDisplayPanel;    // 物体实时展示面板

    [Header("控制按钮引用")]
    public Button controlBtn;
    public Button resetBtn;
    public TMP_Text controlBtnText;

    private bool isMotionRunning = false;    // 运动是否进行中
    private bool isMotionPaused = false;     // 运动是否暂停

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 初始化UI状态：显示可配置项，隐藏实时项
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
        isMotionRunning = false;
        isMotionPaused = false;
    }

    /// <summary>
    /// 绑定所有按钮事件
    /// </summary>
    private void BindButtonEvents()
    {
        controlBtn.onClick.AddListener(OnControlClick);
        resetBtn.onClick.AddListener(OnResetClick);
        
    }

    /// <summary>
    /// 合并按钮点击逻辑（统一处理开始/暂停/继续）
    /// </summary>
    public void OnControlClick()
    {
        // 1. 未运行状态：执行“开始”逻辑
        if (!isMotionRunning)
        {
            isMotionRunning = true;
            isMotionPaused = false;
            controlBtnText.text = "暂停";  // 文本切换为“暂停”

            // 切换UI：隐藏可配置面板，显示实时速度面板
            objectSettingPanel.SetActive(false);
            objectDisplayPanel.SetActive(true);

            // 通知小球开始运动（复用原有方法）
            FindObjectOfType<ObjectPhysicsController>().StartMotion();
        }
        // 2. 已运行但未暂停：执行“暂停”逻辑
        else if (isMotionRunning && !isMotionPaused)
        {
            isMotionPaused = true;
            controlBtnText.text = "继续";  // 文本切换为“继续”

            // 通知小球暂停运动（复用原有方法）
            FindObjectOfType<ObjectPhysicsController>().PauseMotion(true);
        }
        // 3. 已运行且已暂停：执行“继续”逻辑
        else if (isMotionRunning && isMotionPaused)
        {
            isMotionPaused = false;
            controlBtnText.text = "暂停";  // 文本切换为“暂停”

            // 通知小球继续运动（复用原有方法）
            FindObjectOfType<ObjectPhysicsController>().PauseMotion(false);
        }
    }


    /// <summary>
    /// 重置按钮点击
    /// </summary>
    public void OnResetClick()
    {
        // 恢复初始UI状态
        InitUIState();
        // 通知场景和物体重置
        FindObjectOfType<ScenePhysicsController>().ResetScene();
        FindObjectOfType<ObjectPhysicsController>().ResetObject();
        // 相机重置
        Camera2DTrajectoryViewer camera = FindObjectOfType<Camera2DTrajectoryViewer>();
        if (camera != null) camera.ResetCamera();
    }

 

    /// <summary>
    /// 外部获取运动状态（供物理脚本调用）
    /// </summary>
    public bool IsMotionRunning() => isMotionRunning;
    public bool IsMotionPaused() => isMotionPaused;
}
