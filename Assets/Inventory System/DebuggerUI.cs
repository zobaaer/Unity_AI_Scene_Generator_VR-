using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScrollableDebugConsole : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxLines = 100;

    private Queue<string> logQueue = new Queue<string>();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string formattedLog = $"[{type}] {logString}";
        logQueue.Enqueue(formattedLog);

        if (logQueue.Count > maxLines)
            logQueue.Dequeue();

        debugText.text = string.Join("\n", logQueue);

        // Scroll to bottom automatically
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
