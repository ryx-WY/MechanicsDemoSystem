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
    public Button btnStart;
    public Button btnReset;
    public Button btnPause;
    public TMP_Text btnPauseText;

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
        btnPauseText.text = "暂停";
        isMotionRunning = false;
        isMotionPaused = false;
    }

    /// <summary>
    /// 绑定所有按钮事件
    /// </summary>
    private void BindButtonEvents()
    {
        btnStart.onClick.AddListener(OnStartClick);
        btnReset.onClick.AddListener(OnResetClick);
        btnPause.onClick.AddListener(OnPauseClick);
    }

    /// <summary>
    /// 开始按钮点击
    /// </summary>
    public void OnStartClick()
    {
        if (isMotionRunning && !isMotionPaused) return; // 已运行且未暂停，不重复点击

        isMotionRunning = true;
        isMotionPaused = false;
        btnPauseText.text = "暂停";

        // 切换UI：隐藏可配置项，显示实时项
        objectSettingPanel.SetActive(false);
        objectDisplayPanel.SetActive(true);

        // 通知物体开始运动（通过事件/直接调用，二选一）
        FindObjectOfType<ObjectPhysicsController>().StartMotion();
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
    }

    /// <summary>
    /// 暂停/继续按钮点击
    /// </summary>
    public void OnPauseClick()
    {
        if (!isMotionRunning) return; // 未开始运动，不响应

        isMotionPaused = !isMotionPaused;
        btnPauseText.text = isMotionPaused ? "继续" : "暂停";

        // 通知物体暂停/继续
        FindObjectOfType<ObjectPhysicsController>().PauseMotion(isMotionPaused);
    }

    /// <summary>
    /// 外部获取运动状态（供物理脚本调用）
    /// </summary>
    public bool IsMotionRunning() => isMotionRunning;
    public bool IsMotionPaused() => isMotionPaused;
}
