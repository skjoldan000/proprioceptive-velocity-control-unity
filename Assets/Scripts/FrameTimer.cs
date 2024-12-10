using System.Diagnostics;
using UnityEngine;

public class FrameTimer : MonoBehaviour
{
    public static Stopwatch FrameStopwatch { get; private set; }
    public static Stopwatch TrialStopwatch { get; private set; }
    public static Stopwatch SessionStopwatch { get; private set; }

    private void Awake()
    {
        FrameStopwatch = new Stopwatch();
        TrialStopwatch = new Stopwatch();
        SessionStopwatch = new Stopwatch();
        SessionStopwatch.Start();
    }

    public void StartTrialStopwatch()
    {
        TrialStopwatch.Reset();
        TrialStopwatch.Start();
    }

    private void Update()
    {
        FrameStopwatch.Reset();
        FrameStopwatch.Start();
    }
}
