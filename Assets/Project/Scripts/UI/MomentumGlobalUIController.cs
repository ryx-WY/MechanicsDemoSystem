using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 动量守恒场景-全局UI控制器
/// 管理面板显示、按钮事件、运动状态（单例模式）
/// </summary>
public class MomentumGlobalUIController : MonoBehaviour
{
    // 单例实例（全局唯一）
    public static MomentumGlobalUIController Instance;

    [Header("UI面板引用")]
    public GameObject smallCubeSettingPanel; // 小方块设置面板
    public GameObject bigCubeSettingPanel;   // 大方块设置面板
    public GameObject objectDisplayPanel;    // 实时数据显示面板
    public GameObject sceneControlBar;       // 控制栏（按钮）

    [Header("控制按钮引用")]
    public Button controlBtn;        // 开始/暂停/继续按钮
    public Button resetBallBtn;      // 重置方块按钮
    public Button resetSceneBtn;     // 重置场景按钮
    public TMP_Text controlBtnText;  // 按钮文本

    [Header("运动状态")]
    private bool isMotionRunning = false;  // 是否正在运动
    private bool isMotionPaused = false;   // 是否暂停
    private bool isMotionLocked = false;   // 是否锁定（碰撞后无需锁定，弹性碰撞后持续运动）

    void Awake()
    {
        // 单例初始化（确保全局唯一）
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 初始化UI状态
        InitUIState();
        // 绑定所有按钮事件
        BindButtonEvents();
    }

    #region UI初始化与状态管理
    /// <summary>
    /// 初始化UI默认状态
    /// </summary>
    private void InitUIState()
    {
        // 显示设置面板，隐藏实时显示面板
        smallCubeSettingPanel.SetActive(true);
        bigCubeSettingPanel.SetActive(true);
        objectDisplayPanel.SetActive(false);
        sceneControlBar.SetActive(true);

        // 按钮状态初始化
        controlBtnText.text = "开始";
        controlBtn.interactable = true;

        // 运动状态初始化
        isMotionRunning = false;
        isMotionPaused = false;
        isMotionLocked = false;
    }

    /// <summary>
    /// 重置方块后的UI状态恢复
    /// </summary>
    public void ResetBallUIState()
    {
        smallCubeSettingPanel.SetActive(true);
        bigCubeSettingPanel.SetActive(true);
        objectDisplayPanel.SetActive(false);
        controlBtnText.text = "开始";
        controlBtn.interactable = true;
        isMotionRunning = false;
        isMotionPaused = false;
    }
    #endregion

    #region 按钮事件绑定
    /// <summary>
    /// 绑定所有按钮点击事件
    /// </summary>
    private void BindButtonEvents()
    {
        controlBtn.onClick.AddListener(OnControlClick);
        resetBallBtn.onClick.AddListener(OnResetBallClick);
        resetSceneBtn.onClick.AddListener(OnResetSceneClick);
    }
    #endregion

    #region 按钮逻辑实现
    /// <summary>
    /// 控制按钮（开始/暂停/继续）
    /// </summary>
    public void OnControlClick()
    {
        if (isMotionLocked) return; // 锁定状态不响应

        // 1. 未运动 → 开始运动
        if (!isMotionRunning)
        {
            isMotionRunning = true;
            isMotionPaused = false;
            controlBtnText.text = "暂停";
            // 切换面板：隐藏设置，显示实时数据
            smallCubeSettingPanel.SetActive(false);
            bigCubeSettingPanel.SetActive(false);
            objectDisplayPanel.SetActive(true);
            // 通知物理控制器开始运动
            FindObjectOfType<MomentumCubeController>().StartMotion();
        }
        // 2. 运动中未暂停 → 暂停
        else if (isMotionRunning && !isMotionPaused)
        {
            isMotionPaused = true;
            controlBtnText.text = "继续";
            // 通知物理控制器暂停
            FindObjectOfType<MomentumCubeController>().PauseMotion(true);
        }
        // 3. 运动中已暂停 → 继续
        else if (isMotionRunning && isMotionPaused)
        {
            isMotionPaused = false;
            controlBtnText.text = "暂停";
            // 通知物理控制器恢复
            FindObjectOfType<MomentumCubeController>().PauseMotion(false);
        }
    }

    /// <summary>
    /// 重置方块（保留参数，仅重置位置和状态）
    /// </summary>
    public void OnResetBallClick()
    {
        isMotionLocked = false;
        controlBtn.interactable = true;
        InitUIState();
        // 通知物理控制器重置方块
        FindObjectOfType<MomentumCubeController>().ResetBall();
    }

    /// <summary>
    /// 重置场景（恢复默认参数和所有状态）
    /// </summary>
    public void OnResetSceneClick()
    {
        InitUIState();
        // 通知物理控制器重置场景
        FindObjectOfType<MomentumCubeController>().ResetObject();
    }
    #endregion

    #region 外部状态访问（给物理控制器用）
    public bool IsMotionRunning() => isMotionRunning;
    public bool IsMotionPaused() => isMotionPaused;
    #endregion
}
