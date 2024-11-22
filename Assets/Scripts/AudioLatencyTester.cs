using UnityEngine;
using System.Collections;
using TMPro;
public class AudioLatencyTester : MonoBehaviour
{
    public AudioSource vibLeft;
    public AudioSource vibRight;
    public AudioSource vibBoth;
    [SerializeField]private float vibrationVolume = 0.7f;
    [SerializeField]private string vibration = "both";
    public GameObject rightEye;
    public TextMeshPro tmp;
    public float trialVibStart;
    public float trialVibStop;
    private bool testLatencyStarted = false;
    private bool vibrationCRComplete = false;
    private int frameCounter;
    private float timer;
    public GameObject sphere;
    private ControlSphere sphereScript;
    public Material targetGreen;
    public Material standbyGrey;
    public GameObject backGround;
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
        }
    }
    IEnumerator TestLatency()
    {
        sphereScript.ColorObj(standbyGrey);
        testLatencyStarted = true;
        Debug.Log($"TestLatency started");
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 10; i++)
        {

            StartCoroutine(Vibration());
            yield return new WaitUntil(() => vibrationCRComplete);
            yield return new WaitForSeconds(.5f);
        }
    }
    IEnumerator Vibration()
    {
        vibrationCRComplete = false;
        trialVibStart = Time.time;
        frameCounter = 0;
        tmp.text = $"Frame: {frameCounter}\nTime: {timer}";
        sphereScript.ColorObj(targetGreen);
        backGround.GetComponent<Renderer>().material = targetGreen;
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
        for (int i = 0; i < 30; i++)
        {
            yield return null;
        }
        trialVibStop = Time.time;
        sphereScript.ColorObj(standbyGrey);
        backGround.GetComponent<Renderer>().material = standbyGrey;
        vibLeft.Stop();
        vibRight.Stop();
        vibBoth.Stop();
        vibrationCRComplete = true;
    }
}
