using UnityEngine;
using System.Collections;
using TMPro;
using UXF;
public class AudioLatencyTester : MonoBehaviour
{
    [SerializeField]private float vibrationVolume = 0.7f;
    [SerializeField]private string vibration = "both";
    [SerializeField]private int testReps = 100;
    public AudioSource vibLeft;
    public AudioSource vibRight;
    public AudioSource vibBoth;
    public GameObject rightEye;
    public TextMeshPro tmp;
    public float trialVibStart;
    public float trialVibStop;
    public float trialVibStartStopwatch;
    public float trialVibStopStopwatch;
    private bool testLatencyStarted = false;
    private bool vibrationCRComplete = false;
    private int frameCounter;
    private float timer;
    public GameObject sphere;
    private ControlSphere sphereScript;
    public Material targetGreen;
    public Material standbyGrey;
    public GameObject backGround;
    public ArduinoDualReceiver arduinoReciever;
    public TaskRunner taskRunner;
    private Coroutine VibrationCR;
    public double frameStartToAudioStartPre;
    public double frameStartToAudioStartPost;
    public double frameStartToAudioStopPre;
    public double frameStartToAudioStopPost;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("AudioLatencyTester started");
        sphereScript = sphere.GetComponent<ControlSphere>();
        vibBoth.volume = vibrationVolume;
        vibLeft.volume = vibrationVolume;
        vibRight.volume = vibrationVolume;
        sphereScript.ColorObj(standbyGrey);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = rightEye.transform.position;
        frameCounter += 1;
        timer = Time.time - trialVibStart;
        tmp.text = $"Frame: {frameCounter}\nTime: {timer}";
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("key press registered");
            
            StartCoroutine(TestLatency());
            sphereScript.ColorObj(standbyGrey);
        }
        //Debug.Log($"test frameTimer: {FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds}");
        //Debug.Log($"test frameTimer: {frameStartToAudioStartPre}");
    }
    IEnumerator TestLatency()
    {
        arduinoReciever.InitTrialDataFrame(Session.instance.CurrentTrial);
        arduinoReciever.ResetSerialQueue();
        arduinoReciever.saving = true;
        sphereScript.ColorObj(standbyGrey);
        testLatencyStarted = true;
        Debug.Log($"TestLatency started");
        taskRunner.ResetTimers();
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < testReps; i++)
        {
            taskRunner.targetNumber = i;
            //yield return new WaitForSeconds(.1f);
            VibrationCR = StartCoroutine(Vibration());
            yield return new WaitUntil(() => vibrationCRComplete);
            StopCoroutine(VibrationCR);
            yield return new WaitForSeconds(.25f);
        }
        arduinoReciever.saving = false;
        arduinoReciever.SaveDataFrame(Session.instance.CurrentTrial);
    }
    IEnumerator Vibration()
    {
        vibrationCRComplete = false;
        
        frameCounter = 0;
        trialVibStart = Time.time;
        arduinoReciever.SendTrigger("true");
        frameStartToAudioStartPre = FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds;
        double vibstart = FrameTimer.TrialStopwatch.Elapsed.TotalMilliseconds;
        if (vibration == "left")
        {
            vibLeft.Play();
        }
        else if (vibration == "right")
        {
            vibRight.Play();
        }
        else if (vibration == "both")
        {
            vibBoth.Play();
        }
        else if (vibration == "none")
        {
        }
        else
        {
            Debug.LogError($"vibration was set to: {vibration}. Must be either left, right, both or none");
        }
        tmp.text = $"Frame: {frameCounter}\nTime: {timer}";
        sphereScript.ColorObj(targetGreen);
        backGround.GetComponent<Renderer>().material = targetGreen;
        for (int i = 0; i < 30; i++)
        {
            yield return null;
        }
        trialVibStop = Time.time;
        arduinoReciever.SendTrigger("false");
        frameStartToAudioStopPre = FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds;
        double vibstop = FrameTimer.TrialStopwatch.Elapsed.TotalMilliseconds;
        vibLeft.Stop();
        vibRight.Stop();
        vibBoth.Stop();
        sphereScript.ColorObj(standbyGrey);
        backGround.GetComponent<Renderer>().material = standbyGrey;
        vibrationCRComplete = true;
        Debug.Log($"timing of 30 frames: {trialVibStop - trialVibStart}");
    }
}
