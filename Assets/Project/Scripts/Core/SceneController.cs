using UnityEngine;
using System.Collections.Generic;

public enum SimulationState { Idle, Running, Paused, Finished }

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("全局物理参数")]
    [SerializeField] private float globalGravity = 9.81f;

    // 属性：设置重力时自动更新Physics.gravity
    public float GlobalGravity
    {
        get => globalGravity;
        set
        {
            globalGravity = value;
            Physics.gravity = new Vector3(0, -value, 0);
            Debug.Log($"重力更新为: {value} m/s2");
        }
    }

    [Header("场景物体管理")]
    [SerializeField] private List<PhysicsObjectBase> sceneObjects = new List<PhysicsObjectBase>();

    public SimulationState CurrentState { get; private set; } = SimulationState.Idle;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Physics.gravity = new Vector3(0, -globalGravity, 0);
    }

    public void RegisterObject(PhysicsObjectBase obj)
    {
        if (!sceneObjects.Contains(obj))
            sceneObjects.Add(obj);
    }

    public void StartSimulation()
    {
        if (CurrentState == SimulationState.Running) return;

        Time.timeScale = 1f;
        CurrentState = SimulationState.Running;

        foreach (var obj in sceneObjects)
        {
            obj.OnSimulationStart();
        }
    }

    public void PauseSimulation()
    {
        if (CurrentState != SimulationState.Running) return;

        Time.timeScale = 0f;
        CurrentState = SimulationState.Paused;
    }

    public void ResumeSimulation()
    {
        if (CurrentState != SimulationState.Paused) return;

        Time.timeScale = 1f;
        CurrentState = SimulationState.Running;
    }

    public void ResetSimulation()
    {
        Time.timeScale = 1f;
        CurrentState = SimulationState.Idle;

        foreach (var obj in sceneObjects)
        {
            obj.OnSimulationReset();
        }
    }

    public void ClearScene()
    {
        Time.timeScale = 1f;
        CurrentState = SimulationState.Idle;

        foreach (var obj in sceneObjects)
        {
            obj.OnSimulationClear();
        }
    }

    // 物体碰地回调：自动暂停并切换到Finished状态
    public void OnObjectHitGround(PhysicsObjectBase obj)
    {
        CurrentState = SimulationState.Finished;

        // 可选：完全暂停时间，方便观察数据
        Time.timeScale = 0f;

        Debug.Log("小球落地，已自动暂停。可观察最终速度数据验证");
    }
}