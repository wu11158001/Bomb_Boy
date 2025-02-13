using UnityEngine;
using Unity.Multiplayer.Tools.NetStatsMonitor;

public class ConsoleManager : UnitySingleton<ConsoleManager>
{
    [Header("Debug 工具")]
    [SerializeField] Canvas DebugToolCanvas;
    [SerializeField] bool IsUsingDebugTool;

    [Space(30)]
    [Header("流量檢測")]
    [SerializeField] RuntimeNetStatsMonitor RuntimeNetStatsMonitor;

    public override void Awake()
    {
        base.Awake();

        DebugToolCanvas = GameObject.Find("IngameDebugConsole").GetComponent<Canvas>();
        DebugToolCanvas.enabled = false;
    }

    private void Update()
    {
        // 開啟Debug工具
        if (Input.GetKeyDown(KeyCode.RightAlt))
        {
            IsUsingDebugTool = !IsUsingDebugTool;
            DebugToolCanvas.enabled = IsUsingDebugTool;
        }

        // 開啟流量檢測
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            RuntimeNetStatsMonitor.Visible = !RuntimeNetStatsMonitor.Visible;
        }
    }
}
