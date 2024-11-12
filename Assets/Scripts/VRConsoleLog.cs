using UnityEngine;
using System.Linq;
using TMPro;

public class VRConsoleLog : MonoBehaviour
{
    public TextMeshPro textMeshPro; // Reference to the TextMeshPro component
    private string logText = "";    // Stores all log messages
    [SerializeField] private int nLines = 10;

    void Start()
    {
        // Initialize TextMeshPro if it's not set in the Inspector
        if (textMeshPro == null)
        {
            GameObject textObj = new GameObject("VRConsoleText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = new Vector3(0f, 0f, 1f); // Place it 1 meter in front of the headset
            textObj.transform.localRotation = Quaternion.identity;

            textMeshPro = textObj.AddComponent<TextMeshPro>();
            //textMeshPro.fontSize = 2f;
            //textMeshPro.color = Color.black;
            //textMeshPro.alignment = TextAlignmentOptions.Center;
        }

        // Subscribe to Unity's debug log
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        // Unsubscribe when the object is destroyed
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Add the log message to logText
        logText += logString + "\n";

        // Keep the last few lines (optional, to avoid excessive text)
        if (logText.Split('\n').Length > nLines) 
        {
            logText = string.Join("\n", logText.Split('\n').Skip(1).ToArray());
        }

        // Update the TextMeshPro text
        textMeshPro.text = logText;
    }
}
