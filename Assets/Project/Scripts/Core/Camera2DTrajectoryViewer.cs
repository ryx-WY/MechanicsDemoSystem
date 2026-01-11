using UnityEngine;

/// <summary>
/// 2D平抛相机（双模式：锁定跟随+自由视角，支持完整轨迹查看）
/// 挂载到Main Camera，右键拖拽→自由视角；双击屏幕→恢复锁定；滚轮缩放→查看全轨迹
/// </summary>
public class Camera2DTrajectoryViewer : MonoBehaviour
{
    [Header("核心配置")]
    public ObjectPhysicsController targetBall; // 跟踪目标（平抛小球）
    public TrajectoryDrawer trajectoryDrawer;  // 轨迹绘制器（用于判断轨迹范围）

    [Header("跟随参数（解决“跟不上小球”）")]
    public Vector2 followOffset = new(0, 2f);  // 相机相对小球的偏移（正交模式下居中）
    public float followSmooth = 0.05f;         // 跟随平滑度（<0.1f，快速跟紧小球）
    private Vector3 smoothVelocity;            // 平滑运动缓存

    [Header("缩放参数（解决“看不清完整轨迹”）")]
    public float initialOrthoSize = 5f;        // 初始正交视野大小
    public float minOrthoSize = 2f;            // 最小视野（拉近看细节）
    public float maxOrthoSize = 30f;           // 最大视野（拉远看全轨迹，覆盖X=-5到X=25）
    public float zoomSensitivity = 0.8f;       // 滚轮缩放灵敏度

    [Header("自由视角参数（解决“强制锁定”）")]
    public float dragSensitivity = 0.15f;      // 右键拖拽灵敏度
    private bool isFreeView = false;           // 是否自由视角模式（true=不跟随，false=跟随）
    private Vector3 lastMousePos;              // 拖拽时的上一帧鼠标位置
    private Camera orthoCamera;                // 正交相机引用

    void Awake()
    {
        // 1. 强制相机为正交模式（无透视，纯2D）
        orthoCamera = GetComponent<Camera>();
        if (orthoCamera == null) orthoCamera = gameObject.AddComponent<Camera>();
        orthoCamera.orthographic = true;
        orthoCamera.orthographicSize = initialOrthoSize;

        // 2. 自动查找小球和轨迹绘制器（容错，避免手动绑定）
        if (targetBall == null)
            targetBall = FindObjectOfType<ObjectPhysicsController>();
        if (trajectoryDrawer == null && targetBall != null)
            trajectoryDrawer = targetBall.GetComponent<TrajectoryDrawer>();

        // 3. 初始状态：锁定跟随模式
        isFreeView = false;
    }

    void Start()
    {
        // 初始相机位置：对准小球初始位置（避免启动时小球不在视野）
        if (targetBall != null)
        {
            Vector3 initialBallPos = new(
                targetBall.transform.position.x,
                targetBall.transform.position.y,
                -10f // 正交相机Z轴固定，不影响2D显示
            );
            transform.position = initialBallPos + new Vector3(followOffset.x, followOffset.y, 0);
        }
    }

    // 关键：LateUpdate确保小球先移动，相机再跟随（无滞后）
    void LateUpdate()
    {
        if (orthoCamera == null) return;

        // 1. 处理滚轮缩放（无论哪种模式都支持）
        HandleZoom();

        // 2. 处理右键拖拽（切换自由视角/移动相机）
        HandleDrag();

        // 3. 处理双击恢复锁定（自由模式下双击屏幕回到跟随）
        HandleDoubleClick();

        // 4. 模式判断：锁定跟随/自由视角
        if (!isFreeView && targetBall != null)
        {
            FollowTarget(); // 锁定模式：跟紧小球
        }
        // 自由模式：不跟随，保持当前位置（用户可随意查看轨迹）
    }

    /// <summary>
    /// 锁定模式：流畅跟随小球（解决“跟不上”问题）
    /// </summary>
    private void FollowTarget()
    {
        // 小球当前位置（锁定Z=0，纯2D）
        Vector3 currentBallPos = new(
            targetBall.transform.position.x,
            targetBall.transform.position.y,
            -10f
        );

        // 相机目标位置（小球位置+偏移，确保小球在画面居中）
        Vector3 targetCamPos = currentBallPos + new Vector3(followOffset.x, followOffset.y, 0);

        // 平滑跟随（低延迟，快速跟紧）
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetCamPos,
            ref smoothVelocity,
            followSmooth
        );
    }

    /// <summary>
    /// 滚轮缩放（支持大范围缩放，看清完整轨迹）
    /// </summary>
    private void HandleZoom()
    {
        float zoomDelta = Input.mouseScrollDelta.y * zoomSensitivity;
        if (zoomDelta == 0) return;

        // 调整正交视野（上滚=缩小视野→拉近，下滚=放大视野→拉远）
        float newOrthoSize = orthoCamera.orthographicSize - zoomDelta;
        orthoCamera.orthographicSize = Mathf.Clamp(newOrthoSize, minOrthoSize, maxOrthoSize);
    }

    /// <summary>
    /// 右键拖拽（进入自由视角，松开后不锁定）
    /// </summary>
    private void HandleDrag()
    {
        // 按下右键：进入自由视角，记录初始鼠标位置
        if (Input.GetMouseButtonDown(1))
        {
            isFreeView = true;
            lastMousePos = Input.mousePosition;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        // 释放右键：保持自由视角（不自动锁定），恢复鼠标显示
        else if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // 拖拽中：移动相机（自由视角下）
        else if (Input.GetMouseButton(1) && isFreeView)
        {
            Vector3 deltaMouse = Input.mousePosition - lastMousePos;
            // 拖拽方向：鼠标右移=相机左移，鼠标上移=相机下移（符合2D直觉）
            Vector3 moveOffset = new(
                -deltaMouse.x * dragSensitivity * (orthoCamera.orthographicSize / 5f),
                -deltaMouse.y * dragSensitivity * (orthoCamera.orthographicSize / 5f),
                0f // Z轴固定，无3D移动
            );

            // 应用拖拽位移
            transform.position += moveOffset;
            lastMousePos = Input.mousePosition;
        }
    }

    /// <summary>
    /// 双击屏幕：从自由视角恢复到锁定跟随（方便查看后重新跟踪）
    /// </summary>
    private void HandleDoubleClick()
    {
        if (Input.GetMouseButtonDown(0) && Input.touchCount != 1) // 排除触屏误触
        {
            // 检测双击（间隔<0.3秒）
            if (Time.time - lastClickTime < 0.3f)
            {
                isFreeView = false; // 恢复锁定跟随
                Cursor.visible = true;
            }
            lastClickTime = Time.time;
        }
    }

    /// <summary>
    /// 重置相机（与小球/场景重置同步）
    /// </summary>
    public void ResetCamera()
    {
        if (orthoCamera == null || targetBall == null) return;

        // 1. 恢复初始正交视野
        orthoCamera.orthographicSize = Mathf.Clamp(initialOrthoSize, minOrthoSize, maxOrthoSize);

        // 2. 回到小球初始位置的跟随状态
        Vector3 initialBallPos = new(
            targetBall.transform.position.x,
            targetBall.transform.position.y,
            -10f
        );
        transform.position = initialBallPos + new Vector3(followOffset.x, followOffset.y, 0);

        // 3. 恢复锁定跟随模式
        isFreeView = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 双击检测辅助变量
    private float lastClickTime = 0f;
}
