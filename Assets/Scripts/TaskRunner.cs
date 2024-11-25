using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UXF;

using TMPro;

public class TaskRunner : MonoBehaviour
{
    // Toggles
    [SerializeField]private bool showDebugSphere = false;
    [SerializeField]private bool showConsole = true;
    [SerializeField]private bool debugAudio = false;

    // Objects and attached scripts
    private Config c;
    public TextMeshPro blockInstructionsText;
    public GenerateInstructions generateInstructions;
    public GameObject trialSpaceAll;
    public GameObject trialSpaceSetup;
    public GameObject trialSpaceSetupForwardBack;
    public GameObject trialSpaceSetupLateral;
    public GameObject trialSpace;
    private ControlSphere trialSpaceScript;
    public GameObject trialTarget;
    private ControlSphere trialTargetScript;
    public GameObject trialStart;
    private ControlSphere trialStartScript;
    public GameObject controllerSphere;
    private ControlSphere controllerSphereScript;
    public GameObject debugSphere;
    private ControlSphere debugSphereScript;
    public GameObject trackedDesk;
    private PositionDesk trackedDeskScript;
    public GameObject rightControllerAnchor;
    public GameObject rightHandAnchor;
    public GameObject leftControllerAnchor;
    public GameObject leftHandAnchor;
    public GameObject buttonA;
    public GameObject buttonB;
    public GameObject tracker2d;
    public GameObject tracker1d;
    public GameObject RotatingArm;
    private ControlRotatingArmrest RotatingArmScript;
    public GameObject AudioLatencyTester;
    public GameObject VRConsole;
    // Materials
    public Material targetGreen;
    public Material startTeal;
    public Material activeBlue;
    public Material readyYellow;
    public Material standbyGrey;
    public Material controllerPurple;

    // UXF settings
    private bool calibration;
    private int nDims;

    // Trial progress
    private bool calibrationComplete = false;
    private bool blockInstructionsComplete = false;
    public string trialProgress;
    public string trialID;
    public int targetNumber;

    // Coroutines
    private Coroutine currentTrialCR;
    private Coroutine calibrationCR;
    private Coroutine ControllerVisibilityCR;

    // Timers
    private float trialSetupTime;
    private float trialStartedTime;
    private float trialControlVisibilityOffTime;
    private float trialControlVisibilityOnTime;
    private float trialInputTime;
    private float trialVibStart;
    private float trialVibStop;


    // Vibration source
    public AudioSource vibLeft;
    public AudioSource vibRight;
    public AudioSource vibBoth;
    [SerializeField]private float vibrationVolume = 0.7f;

    // Others
    public TextMeshPro trialCounter;
    public float trialSpaceRotationY = 0f;

    // Results to save
    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 trueInput;
    private Vector3 visualOffsetInput;



    void Start()
    {
        c = GetComponent<Config>();
        trialSpaceScript = trialSpace.GetComponent<ControlSphere>();
        trialTargetScript = trialTarget.GetComponent<ControlSphere>();
        trialStartScript = trialStart.GetComponent<ControlSphere>();
        controllerSphereScript = controllerSphere.GetComponent<ControlSphere>();
        debugSphereScript = debugSphere.GetComponent<ControlSphere>();
        trackedDeskScript = trackedDesk.GetComponent<PositionDesk>();
        RotatingArmScript = RotatingArm.GetComponent<ControlRotatingArmrest>();
        if (trialStartScript == null)
        {
            Debug.LogError("ControlSphere component is missing on trialStart.");
        }
        vibBoth.volume = vibrationVolume;
        vibLeft.volume = vibrationVolume;
        vibRight.volume = vibrationVolume;

        trialStartScript.Visible(false);
        trialTargetScript.Visible(false);
        controllerSphereScript.Visible(false);
        tracker1d.SetActive(false);
        tracker2d.SetActive(false);
        Debug.Log("TaskRunner start completed");
    }

    public void StartTrial(Trial trial)
    {
        //currentTrial = StartCoroutine(TrialCoroutine(trial));
        currentTrialCR = StartCoroutine(TrialCoroutine(trial));
    }

    public void EndTrialCoroutine()
    {
        StopCoroutine(currentTrialCR);
    }

    IEnumerator TrialCoroutine(Trial trial)
    {
        debugSphereScript.Visible(showDebugSphere);
        if (trial.settings.GetString("trialOrientation") == "lateral")
        {
            trialSpaceSetup.transform.position = trialSpaceSetupLateral.transform.position;
            trialSpaceSetup.transform.rotation = trialSpaceSetupLateral.transform.rotation;
        }
        else if (trial.settings.GetString("trialOrientation") == "forwardBack")
        {
            trialSpaceSetup.transform.position = trialSpaceSetupForwardBack.transform.position;
            trialSpaceSetup.transform.rotation = trialSpaceSetupForwardBack.transform.rotation;
        }
        else
        {
            Debug.LogError("trialOrientation must be either 'lateral' or 'forwardBack'");
            trial.End();
            Session.instance.End();
        }

        trialSpace.transform.position = trialSpaceSetup.transform.position;
        trialSpace.transform.rotation = trialSpaceSetup.transform.rotation;

        if (trial.settings.GetBool("applyRandomRotation"))
        {
            List<float> randomRotationRange = trial.settings.GetFloatList("randomRotationRange");
            trialSpaceRotationY = Random.Range(randomRotationRange[0], randomRotationRange[1]);
            trialSpace.transform.Rotate(0f, trialSpaceRotationY, 0f);
        }

        if (trial.settings.GetInt("nDims") == 1)
        {
            tracker1d.SetActive(true);
        }
        if (trial.settings.GetInt("nDims") == 2)
        {
            tracker2d.SetActive(true);
        }
        trialID = $"{Session.instance.ppid}_{Session.instance.experimentName}_{Session.instance.number}_{Session.instance.currentBlockNum}_{Session.instance.currentTrialNum}";
        // calibration
        if (trial.settings.GetBool("calibration"))
        {
            leftControllerAnchor.SetActive(true);
            controllerSphereScript.Visible(false);
            trialProgress = "calibrationStarted";
            Debug.Log("Calibration started");
            if (trial.settings.GetInt("nDims") == 1)
            {
                calibrationCR = StartCoroutine(calibration1d());
                yield return new WaitUntil(() => (calibrationComplete));
                StopCoroutine(calibrationCR);
            }
            else if (trial.settings.GetInt("nDims") == 2)
            {
                calibrationCR = StartCoroutine(calibration2d());
                yield return new WaitUntil(() => (calibrationComplete));
                StopCoroutine(calibrationCR);
            }
            leftControllerAnchor.SetActive(false);
            controllerSphereScript.Visible(true);
            Debug.Log("Calibration completed");
            trial.End();
            yield return new WaitForSeconds(0.5f);
        }

        trialCounter.text = $"Trial {trial.number}/{Session.instance.LastTrial.number}\vBlock {Session.instance.currentBlockNum-1}/{trial.settings.GetIntList("runBlocks").Count}";
        PrintTrialConditions(trial);

        if (calibrationComplete && Session.instance.CurrentTrial.numberInBlock == 1)
        {
            blockInstructionsComplete = false;
            StartCoroutine(BlockInstructions(trial));
            yield return new WaitUntil(() => (blockInstructionsComplete));
        }

        // trial setup
        trialProgress = "trialSetup";
        trialSetupTime = Time.time;

        startPos = trialStart.transform.position;
        targetPos = new Vector3(0f, 0f, 0.4f);
        controllerSphereScript.Visible(true);
        trialStartScript.Visible(true);
        trialTargetScript.Visible(true);
        trialTargetScript.PositionAnchor(targetPos);
        trialTargetScript.ColorObj(standbyGrey);
        trialStartScript.ColorObj(startTeal);
        controllerSphereScript.ResetVisualOffset();

        float timer = 0f;
        bool readyButtonPressed = false;
        Vector3 initiatePosition = new Vector3();
        // Wait for proper distance to start and ready button to be pressed. Trial will start after delay from press.
        while(true)
        {
            if (timer > 0.5f)
            {
                break;
            }
            if (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) < 0.025)
            {
                trialStartScript.ColorObj(activeBlue);
                if (OVRInput.GetDown(OVRInput.Button.Two) | readyButtonPressed)
                {
                    if (!readyButtonPressed)
                    {
                        initiatePosition = rightHandAnchor.transform.position;
                    }
                    trialSpace.transform.position = rightHandAnchor.transform.position;
                    readyButtonPressed = true;
                    trialStartScript.ColorObj(readyYellow);
                    trialTargetScript.ColorObj(readyYellow);
                    timer += 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if (readyButtonPressed && Vector3.Distance(initiatePosition, rightHandAnchor.transform.position) > 0.01)
            {
                Debug.Log("Distance exceeded triggered: " + Vector3.Distance(initiatePosition, rightHandAnchor.transform.position));
                timer = 0f;
                readyButtonPressed = false;
                trialStartScript.ColorObj(startTeal);
                trialTargetScript.ColorObj(standbyGrey);
                trialSpace.transform.position = trialSpaceSetup.transform.position;
            }
            else
            {
                timer = 0f;
                readyButtonPressed = false;
                trialStartScript.ColorObj(startTeal);
                trialTargetScript.ColorObj(standbyGrey);
                trialSpace.transform.position = trialSpaceSetup.transform.position;
            }
            yield return null;
        }


        trialProgress = "trialStarted";
        trialStartedTime = Time.time;

        if (trial.settings.GetInt("nTargets") == 1)
        {
            trialStartScript.Visible(false);
            controllerSphereScript.Visible(trial.settings.GetBool("controllerVisibleTrialStart"));


            controllerSphereScript.visualOffsetFromReference(new Vector3(
                trial.settings.GetFloat("visualXOffset"), 
                0, 
                trial.settings.GetFloat("visualZOffset")),
                trialSpace.transform);
            trialTargetScript.ColorObj(targetGreen);
            // Trial is now initiated
            // Controller visibility
            yield return new WaitUntil(() =>(GetRelativePosition(trialSpace.transform, rightHandAnchor.transform).z > trial.settings.GetFloat("controllerMidpointOnStart")));
            StartCoroutine(ControllerVisibility(trial));

            // Ensure input button is not fat fingered immediately before input is allowed
            yield return new WaitUntil(() => (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) > 0.1f));
            yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
        }

        if (trial.settings.GetInt("nTargets") > 1)
        {
            controllerSphereScript.Visible(trial.settings.GetBool("controllerVisibleTrialStart"));
            Vector3 lastInputPosition = trialStart.transform.position;
            //targetNumber = 1;
            for (int i = 1; i <= trial.settings.GetInt("nTargets"); i++)
            {
                if (i > 1)
                {
                   yield return null; // pause till next frame to not increment targetNumber on the same frame as Button.One is pressed
                }
                controllerSphereScript.ResetVisualOffset(); 
                targetNumber = i;
                if (i % 2 == 1)
                {
                    trialStartScript.ColorObj(standbyGrey);
                    trialTargetScript.ColorObj(targetGreen);
                }
                else
                {
                    trialStartScript.ColorObj(targetGreen);
                    trialTargetScript.ColorObj(standbyGrey);
                }
                yield return new WaitUntil(() => Vector3.Distance(lastInputPosition, rightHandAnchor.transform.position) > trial.settings.GetFloat("controllerMidpointOnStart"));
                if (i == 9 | i == 11)
                {
                    StartCoroutine(ControllerVisibility(trial));
                    controllerSphereScript.visualOffsetFromReference(new Vector3(
                        trial.settings.GetFloat("visualXOffset"),
                        0, 
                        trial.settings.GetFloat("visualZOffset")),
                        trialSpace.transform);
                }
                yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
                lastInputPosition = rightHandAnchor.transform.position;
            }
        }

        controllerSphereScript.Visible(false);

        trueInput = GetRelativePosition(trialSpace.transform, rightHandAnchor.transform);
        visualOffsetInput = GetRelativePosition(trialSpace.transform, controllerSphere.transform);

        trialProgress = "trialInput";
        trialInputTime = Time.time;

        trialStartScript.Visible(true);
        trialTargetScript.PositionAnchor(targetPos);
        trialTargetScript.ColorObj(standbyGrey);
        trialStartScript.ColorObj(startTeal);
        controllerSphereScript.ResetVisualOffset();

        SaveResults(trial);
        yield return new WaitUntil(() => (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) < 0.1f));

        trial.End();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(ConfirmQuitCR());
        }
        AudioLatencyTester.SetActive(debugAudio);
        VRConsole.SetActive(showConsole);
        debugSphereScript.Visible(showDebugSphere);
    }
    IEnumerator ConfirmQuitCR()
    {
        Debug.LogWarning("Press Y to confirm quit, else press N.");
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Session.instance.CurrentTrial.End();
                Session.instance.End();
                yield return new WaitForSeconds(1);
                // Exit the application
                Application.Quit();

                // If you are running in the Unity editor
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.LogWarning("Quit cancelled.");
                break;
            }
            yield return null;
        }
    }
    IEnumerator calibration1d()
    {
        blockInstructionsText.text = (
            "Calibration instructions\n\n"+
            "First calibrate the table position by holding the left "+
            "controller attached to the table and press X.");
        trackedDeskScript.StartCalibrateDesk();
        yield return new WaitUntil(() => (trackedDeskScript.DeskPositioned));
        RotatingArmScript.StartCalibration();
        yield return new WaitUntil(() => (RotatingArmScript.calibrationComplete));
        calibrationComplete = true;
    }
    IEnumerator calibration2d()
    {
        blockInstructionsText.text = (
            "Calibration instructions\n\n"+
            "First calibrate the table position by holding the left "+
            "controller attached to the table and press X.");
        trackedDeskScript.StartCalibrateDesk();
        yield return new WaitUntil(() => (trackedDeskScript.DeskPositioned));
        blockInstructionsText.text = (
            "Calibration instructions\n\n"+
            "Now calibrate the right controller by resting it on the table and pressing A");
        GameObject a_instructionsArrow = generateInstructions.InstantiateArrowText(
            buttonA,
            "Rest controller on table and\npress A to calibrate right controller",
            true
        );
        yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
        trialSpaceAll.transform.position = new Vector3(
            trialSpaceAll.transform.position.x,
            rightControllerAnchor.transform.position.y,
            trialSpaceAll.transform.position.z);

        Destroy(a_instructionsArrow);
        rightControllerAnchor.SetActive(false);
        calibrationComplete = true;
    }
    IEnumerator ControllerVisibility(Trial trial)
    {
        // This coroutine is called once a certain distance/time is travelled in each trial
        if (trial.settings.GetBool("turnControllerVisibleMidpoint"))
        {
            controllerSphereScript.Visible(true);
            Debug.Log("ControllerVisibility, controllerSphereScript set to true");
        }

        trialProgress = "trialControlVisibilityOn";
        trialControlVisibilityOnTime = Time.time;
        if (trial.settings.GetBool("turnControllerVisibleMidpoint"))
        {
            for (int i = 0; i < trial.settings.GetInt("controllerMidpointVisibleFrames"); i++)
            {
                yield return null;
            }
        }
        trialProgress = "trialControlVisibilityOff";
        trialControlVisibilityOffTime = Time.time;

        if (trial.settings.GetBool("turnControllerVisibleMidpoint"))
        {
            controllerSphereScript.Visible(false);
            Debug.Log("ControllerVisibility, controllerSphereScript set to false");
        }
    }

    IEnumerator Vibration(Trial trial)
    {
        string vibration = trial.settings.GetString("vibration");
        trialVibStart = Time.time;
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
        vibLeft.Stop();
        vibRight.Stop();
        vibBoth.Stop();
    }

    IEnumerator BlockInstructions(Trial trial)
    {
        controllerSphereScript.Visible(false);
        rightControllerAnchor.SetActive(true);
        GameObject b_instructionsArrow = generateInstructions.InstantiateArrowText(
            buttonB,
            "Starting new task!\n Look up and read instructions. Press and hold here when ready to start.",
            true
        );
        string instructions = "Block instructions: \n" + trial.settings.GetString("blockInstructions");
        string instructions2 = "\n\nWhen you have read the instructions and is ready to start the task, " +
        "press and hold B for 1 second.";
        blockInstructionsText.text = instructions + instructions2;
        float timer = 0f;
        while (timer < 1f)
        {
            if (OVRInput.Get(OVRInput.Button.Two))
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0f;
            }
            yield return null;
        }
        Destroy(b_instructionsArrow);
        blockInstructionsText.text = instructions;
        rightControllerAnchor.SetActive(false);
        controllerSphereScript.Visible(true);
        blockInstructionsComplete = true;
    }

    private void GetTrialSettings(Trial trial)
    {
        calibration = trial.settings.GetBool("calibration");
        nDims = trial.settings.GetInt("nDims");
    }

    private void SaveResults(Trial trial)
    {
        Session.instance.CurrentTrial.result["trialID"] = trialID;
        Session.instance.CurrentTrial.result["targetPos.x"] = targetPos.x;
        Session.instance.CurrentTrial.result["targetPos.y"] = targetPos.y;
        Session.instance.CurrentTrial.result["targetPos.z"] = targetPos.z;
        Session.instance.CurrentTrial.result["startPos.x"] = startPos.x;
        Session.instance.CurrentTrial.result["startPos.y"] = startPos.y;
        Session.instance.CurrentTrial.result["startPos.z"] = startPos.z;
        Session.instance.CurrentTrial.result["trueInput.x"] = trueInput.x;
        Session.instance.CurrentTrial.result["trueInput.y"] = trueInput.y;
        Session.instance.CurrentTrial.result["trueInput.z"] = trueInput.z;
        Session.instance.CurrentTrial.result["visualOffsetInput.x"] = visualOffsetInput.x;
        Session.instance.CurrentTrial.result["visualOffsetInput.y"] = visualOffsetInput.y;
        Session.instance.CurrentTrial.result["visualOffsetInput.z"] = visualOffsetInput.z;
        Session.instance.CurrentTrial.result["trialSpaceRotation.y"] = trialSpaceRotationY;
        Session.instance.CurrentTrial.result["trialSetupTime"] = trialSetupTime;
        Session.instance.CurrentTrial.result["trialStartedTime"] = trialStartedTime;
        Session.instance.CurrentTrial.result["trialControlVisibilityOffTime"] = trialControlVisibilityOffTime;
        Session.instance.CurrentTrial.result["trialControlVisibilityOnTime"] = trialControlVisibilityOnTime;
        Session.instance.CurrentTrial.result["trialInputTime"] = trialInputTime;
        Session.instance.CurrentTrial.result["trialVibStart"] = trialVibStart;
        Session.instance.CurrentTrial.result["trialVibStop"] = trialVibStop;
    }
    private Vector3 GetRelativePosition(Transform reference, Transform target)
    {
        return reference.InverseTransformPoint(target.position);
    }
    private void PrintTrialConditions(Trial trial)
    {
        Debug.Log($"Block no: {Session.instance.currentBlockNum}, Trial no: {Session.instance.currentTrialNum}, Trial no in block: {Session.instance.CurrentTrial.numberInBlock}");
        Debug.Log($"controllerVisibleTrialStart: {trial.settings.GetBool("controllerVisibleTrialStart")}");
        Debug.Log($"turnControllerVisibleMidpoint: {trial.settings.GetBool("turnControllerVisibleMidpoint")}");
        Debug.Log($"X offset: {trial.settings.GetFloat("visualXOffset")}, Z offset: {trial.settings.GetFloat("visualZOffset")}, rotation.y: {trialSpaceRotationY}");
    }
}
