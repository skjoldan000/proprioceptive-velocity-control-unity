using System.Diagnostics;
using UnityEngine;

public class FrameTimer : MonoBehaviour
{
    public static Stopwatch FrameStopwatch { get; private set; }
    public static Stopwatch SessionStopwatch { get; private set; }

    private void Awake()
    {
        FrameStopwatch = new Stopwatch();
        SessionStopwatch = new Stopwatch();
    }
    void Start()
    {
        SessionStopwatch.Start();
    }

    private void Update()
    {
        FrameStopwatch.Reset();
        FrameStopwatch.Start();
    }
}
