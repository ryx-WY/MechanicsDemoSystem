using UnityEngine;

public class Camera2DTrajectoryViewer : MonoBehaviour
{
    [Header("核心配置")]
    public PhysicsObjectBase targetBall;

    [Header("跟随参数")]
    public Vector2 followOffset = new(0, 2f);
    [SerializeField] private bool useSmoothFollow = false; // 新增：可选平滑（默认关闭防重影）
    public float followSmooth = 0.05f;
    private Vector3 smoothVelocity;

    [Header("缩放参数")]
    public float initialOrthoSize = 5f;
    public float minOrthoSize = 2f;
    public float maxOrthoSize = 30f;
    public float zoomSensitivity = 0.8f;

    [Header("自由视角参数")]
    public float dragSensitivity = 0.15f;
    private bool isFreeView = false;
    private Vector3 lastMousePos;
    private Camera orthoCamera;
    private float lastClickTime = 0f;

    void Awake()
    {
        orthoCamera = GetComponent<Camera>();
        if (orthoCamera == null) orthoCamera = gameObject.AddComponent<Camera>();
        orthoCamera.orthographic = true;
        orthoCamera.orthographicSize = initialOrthoSize;

        if (targetBall == null)
            targetBall = FindObjectOfType<ProjectileObject>();

        isFreeView = false;
    }

    void Start()
    {
        if (targetBall != null)
        {
            // 初始位置对齐
            Vector3 initialBallPos = targetBall.transform.position;
            transform.position = new Vector3(
                initialBallPos.x + followOffset.x,
                initialBallPos.y + followOffset.y,
                -10f
            );
        }
    }

    void LateUpdate()
    {
        if (orthoCamera == null) return;

        HandleZoom();
        HandleDrag();
        HandleDoubleClick();

        if (!isFreeView && targetBall != null)
        {
            FollowTarget();
        }
    }

    private void FollowTarget()
    {
        // 获取小球当前位置（使用transform.position确保与渲染同步）
        Vector3 targetPos = targetBall.transform.position;
        Vector3 desiredCamPos = new Vector3(
            targetPos.x + followOffset.x,
            targetPos.y + followOffset.y,
            -10f
        );

        if (useSmoothFollow)
        {
            // 平滑模式：使用极短平滑时间，减少拖影
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredCamPos,
                ref smoothVelocity,
                followSmooth,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
        }
        else
        {
            // 关键修复：直接赋值，无滞后，彻底消除重影
            transform.position = desiredCamPos;
        }
    }

    private void HandleZoom()
    {
        float zoomDelta = Input.mouseScrollDelta.y * zoomSensitivity;
        if (Mathf.Abs(zoomDelta) > 0.01f)
        {
            float newSize = orthoCamera.orthographicSize - zoomDelta;
            orthoCamera.orthographicSize = Mathf.Clamp(newSize, minOrthoSize, maxOrthoSize);
        }
    }

    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isFreeView = true;
            lastMousePos = Input.mousePosition;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButton(1) && isFreeView)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            Vector3 moveOffset = new(
                -delta.x * dragSensitivity * (orthoCamera.orthographicSize / 5f),
                -delta.y * dragSensitivity * (orthoCamera.orthographicSize / 5f),
                0f
            );
            transform.position += moveOffset;
            lastMousePos = Input.mousePosition;
        }
    }

    private void HandleDoubleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < 0.3f)
            {
                isFreeView = false;
                Cursor.visible = true;
            }
            lastClickTime = Time.time;
        }
    }

    public void ResetCamera()
    {
        if (orthoCamera == null || targetBall == null) return;
        orthoCamera.orthographicSize = initialOrthoSize;

        Vector3 ballPos = targetBall.transform.position;
        transform.position = new Vector3(
            ballPos.x + followOffset.x,
            ballPos.y + followOffset.y,
            -10f
        );

        smoothVelocity = Vector3.zero;
        isFreeView = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}