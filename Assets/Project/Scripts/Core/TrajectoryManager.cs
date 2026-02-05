using UnityEngine;
using System.Collections.Generic;

public class TrajectoryManager : MonoBehaviour
{
    public static TrajectoryManager Instance;

    [Header("轨迹样式")]
    public float lineWidth = 0.05f;
    public float recordInterval = 0.05f;        // 记录间隔（降低频率）
    public float minRecordDistance = 0.1f;      // 新增：最小记录距离，防止密集点
    public Transform trajectoryParent;

    private List<LineRenderer> allTrajectories = new List<LineRenderer>();
    private LineRenderer currentTrajectory;
    private List<Vector3> currentPoints = new List<Vector3>();
    private float lastRecordTime;
    private bool isRecording = false;
    private GameObject currentTarget;

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (trajectoryParent == null)
        {
            GameObject parent = new GameObject("Trajectories");
            trajectoryParent = parent.transform;
        }
    }

    public Color StartNewTrajectory(GameObject target, Vector3 startPos)
    {
        StopRecording();

        currentTarget = target;
        currentPoints.Clear();
        currentPoints.Add(startPos);

        GameObject trajObj = new GameObject($"Trajectory_{allTrajectories.Count + 1}");
        trajObj.transform.SetParent(trajectoryParent);

        currentTrajectory = trajObj.AddComponent<LineRenderer>();
        Color randomColor = GetRandomColor();
        currentTrajectory.material = new Material(Shader.Find("Unlit/Color"));
        currentTrajectory.material.color = randomColor;
        currentTrajectory.startColor = randomColor;
        currentTrajectory.endColor = randomColor;
        currentTrajectory.startWidth = lineWidth;
        currentTrajectory.endWidth = lineWidth;
        currentTrajectory.useWorldSpace = true;
        currentTrajectory.positionCount = 1;
        currentTrajectory.SetPosition(0, startPos);

        isRecording = true;
        lastRecordTime = Time.time;
        allTrajectories.Add(currentTrajectory);

        return randomColor;
    }

    public void RecordTrajectoryPoint(Vector3 pos)
    {
        if (!isRecording || currentTrajectory == null) return;

        if (Time.time - lastRecordTime >= recordInterval)
        {
            pos.z = 0;

            // 新增：距离检查，防止点太密集导致抽搐和重影
            if (currentPoints.Count > 0)
            {
                float dist = Vector3.Distance(pos, currentPoints[currentPoints.Count - 1]);
                if (dist < minRecordDistance) return;
            }

            currentPoints.Add(pos);
            currentTrajectory.positionCount = currentPoints.Count;
            currentTrajectory.SetPositions(currentPoints.ToArray());
            lastRecordTime = Time.time;
        }
    }

    public bool IsRecording() => isRecording;

    public void StopRecording()
    {
        isRecording = false;
        currentTrajectory = null;
        currentPoints.Clear();
        currentTarget = null;
    }

    public void ClearAllTrajectories()
    {
        StopRecording();
        foreach (var line in allTrajectories)
        {
            if (line != null) Destroy(line.gameObject);
        }
        allTrajectories.Clear();
    }

    public List<LineRenderer> GetAllTrajectories() => allTrajectories;

    private Color GetRandomColor()
    {
        return new Color(
            Random.Range(0.5f, 1f),
            Random.Range(0.5f, 1f),
            Random.Range(0.5f, 1f),
            0.8f
        );
    }
}