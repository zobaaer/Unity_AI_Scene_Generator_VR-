using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class XRDebug : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxLines = 100;

    private Queue<string> logQueue = new Queue<string>();

    private void Awake()
    {
        Application.logMessageReceived += HandleLog;

        Debug.Log("[XRDebug] Debug console initialized.");
        Debug.Log("[XRDebug] This should appear in your scroll view.");
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string formattedLog = $"[{type}] {logString}";
        logQueue.Enqueue(formattedLog);

        if (logQueue.Count > maxLines)
            logQueue.Dequeue();

        debugText.text = string.Join("\n", logQueue.ToArray());

        // Force scroll to bottom
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
