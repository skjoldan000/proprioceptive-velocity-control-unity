using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UXF;

using TMPro;

public class TaskRunner : MonoBehaviour
{
    // Toggles
    [SerializeField] private bool showDebugSpheresRotation;
    [SerializeField] private bool showDebugSphere = false;
    [SerializeField] private bool showConsole = true;
    [SerializeField] private bool debugAudio = false;
    [SerializeField] private float angleToCalibrateWith = 75f;

    // Objects and attached scripts
    private Config c;
    private FrameTimer frameTimer;
    public GameObject UserInputSelector;
    public TextMeshPro blockInstructionsText;
    public GenerateInstructions generateInstructions;
    public GameObject trialSpaceAll;
    public GameObject trialSpaceSetup;
    public GameObject trialSpaceRotatedMovementSpace;
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
    public GameObject buttonAoffset;
    public GameObject buttonB;
    public GameObject buttonBoffset;
    public GameObject tracker2d;
    public GameObject tracker1d;
    public GameObject RotatingArm;
    private ControlRotatingArmrest RotatingArmScript;
    public GameObject AudioLatencyTester;
    public GameObject VRConsole;

    // Objects relating to rotating armrest
    public GameObject gripModel;
    public GameObject rotatingArmSpace;
    public GameObject rotatingArm;
    public GameObject rotatingArmObj;
    public GameObject positionToOffsetFrom;
    // public GameObject rotationToOffsetFrom;
    public GameObject positionProjected;
    public GameObject rotationProjected;
    public GameObject positionOffsetProjected;
    // private ControlSphere positionToOffsetFromScript;
    private ControlSphere positionProjectedScript;
    private ControlSphere positionOffsetProjectedScript;
    public GameObject offsetControllerAnchor;
    public GameObject rotationPacer;
    public GameObject positionPacer;
    private ControlSphere positionPacerScript;
    public GameObject A1d;
    public GameObject A1dPos;
    private ControlSphere A1dPosScript;
    public GameObject B1d;
    public GameObject B1dPos;
    private ControlSphere B1dPosScript;
    public GameObject D1targetSpaceRotation;
    private float visualSpeedOffsetStartFraction;
    private float visualSpeedOffsetEndFraction;
    private float angleStartToControllerLastFrame;

    // Materials
    public Material targetGreen;
    public Material startTeal;
    public Material activeBlue;
    public Material readyYellow;
    public Material standbyGrey;
    public Material controllerPurple;

    // UXF settings
    private bool calibration;

    // Trial progress
    private bool calibrationComplete = false;
    public bool calibrationArmrestComplete = false;
    public bool UserInputSelectorCRComplete = false;
    public bool savePointsForCalibrationComplete = false;
    private bool blockInstructionsComplete = false;
    public string trialProgress;
    public string trialID;
    public int targetNumber;
    private bool trialEndButtonPressed = false;

    // Coroutines
    private Coroutine currentTrialCR;
    private Coroutine calibrationCR;
    private Coroutine vibrationCR;
    private Coroutine rotatePacerCR;
    private Coroutine ControllerVisibilityCR;

    // Timers
    private float trialSetupTime;
    private float trialStartedTime;
    private float trialControlVisibilityOffTime;
    private float trialControlVisibilityOnTime;
    private float trialInputTime;
    private double trialVibStart;
    private double trialVibStop;


    // Vibration source
    public AudioSource vibLeft;
    public AudioSource vibRight;
    public AudioSource vibBoth;
    [SerializeField]private float vibrationVolume = 0.7f;
    public bool vibrationTriggered = false;

    // Others
    public TextMeshPro trialCounter;
    public float trialSpaceRotationY = 0f;
    //public float angleMultiplier = 1f;
    private float radius;
    private float radiusSD;
    public float angleStartToController;
    public float angleStartToOffsetController;
    public float angleStartToPacer;
    private List<Vector3> pointsForCalibration;
    private int nDims;
    //public ArduinoReciever arduinoReciever;
    public ArduinoDualReceiver arduinoReciever;

    // Results to save
    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 trueInput;
    private Vector3 visualOffsetInput;



    void Start()
    {
        c = GetComponent<Config>();
        frameTimer = GetComponent<FrameTimer>();
        trialSpaceScript = trialSpace.GetComponent<ControlSphere>();
        trialTargetScript = trialTarget.GetComponent<ControlSphere>();
        trialStartScript = trialStart.GetComponent<ControlSphere>();
        controllerSphereScript = controllerSphere.GetComponent<ControlSphere>();
        debugSphereScript = debugSphere.GetComponent<ControlSphere>();
        debugSphereScript.PositionSphere(buttonA.transform.localPosition);
        trackedDeskScript = trackedDesk.GetComponent<PositionDesk>();
        RotatingArmScript = RotatingArm.GetComponent<ControlRotatingArmrest>();


        //relating to rotating armrest
        // positionToOffsetFromScript = positionToOffsetFrom.GetComponent<ControlSphere>();
        positionProjectedScript = positionProjected.GetComponent<ControlSphere>();
        positionOffsetProjectedScript = positionOffsetProjected.GetComponent<ControlSphere>();
        positionPacerScript = positionPacer.GetComponent<ControlSphere>();
        A1dPosScript = A1dPos.GetComponent<ControlSphere>();
        B1dPosScript = B1dPos.GetComponent<ControlSphere>();

        // apply visual offsets to match thumb button position
        controllerSphereScript.PositionSphere(buttonA.transform.localPosition);
        positionOffsetProjectedScript.PositionSphere(buttonA.transform.localPosition);
        debugSphereScript.PositionSphere(buttonA.transform.localPosition);
        A1dPosScript.PositionSphere(buttonA.transform.localPosition);
        B1dPosScript.PositionSphere(buttonA.transform.localPosition);
        positionPacerScript.PositionSphere(buttonA.transform.localPosition);
        controllerSphereScript.PositionSphere(buttonA.transform.localPosition);
        trialStartScript.PositionSphere(buttonA.transform.localPosition);
        trialTargetScript.PositionSphere(buttonA.transform.localPosition);

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
        A1dPosScript.Visible(false);
        B1dPosScript.Visible(false);
        positionPacerScript.Visible(false);
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

    public void ResetTimers()
    {
        frameTimer.StartTrialStopwatch();
        arduinoReciever.SendTrigger("zerotimer");
    }

    IEnumerator TrialCoroutine(Trial trial)
    {
        ResetTimers(); 
        trialEndButtonPressed = false;

        arduinoReciever.InitTrialDataFrame(trial);

        nDims = trial.settings.GetInt("nDims");
        debugSphereScript.Visible(showDebugSphere);

        trialID = $"{Session.instance.ppid}_{Session.instance.experimentName}_{Session.instance.number}_{Session.instance.currentBlockNum}_{Session.instance.currentTrialNum}";

        // calibration
        if (trial.settings.GetBool("calibration"))
        {
            leftControllerAnchor.SetActive(true);
            controllerSphereScript.Visible(false);
            trialProgress = "calibrationStarted";
            Debug.Log("Calibration started");
            rightControllerAnchor.SetActive(true);
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
            rightControllerAnchor.SetActive(false);
            offsetControllerAnchor.SetActive(false);
            leftControllerAnchor.SetActive(false);
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

        if (nDims == 1)
        {
            tracker1d.SetActive(true);

            Debug.Log("nDims1 started");
            // yield return new WaitUntil(() => Mathf.Abs(CalcAngleDirectional(positionProjected, A1dPos, rotatingArmSpace)) < 15);
            visualSpeedOffsetStartFraction = trial.settings.GetFloat("visualSpeedOffsetStartFraction");
            visualSpeedOffsetEndFraction = trial.settings.GetFloat("visualSpeedOffsetEndFraction");

            controllerSphereScript.Visible(false);
            positionOffsetProjectedScript.Visible(true);
            rotatingArmObj.SetActive(false);
            offsetControllerAnchor.SetActive(false);
            float randomRotationRange1d = trial.settings.GetFloat("randomRotationRange1d");
            float randomRotationRange1dThisTrial = UnityEngine.Random.Range(-randomRotationRange1d, randomRotationRange1d);
            Vector3 dirToStart = Quaternion.Euler(0, -40.0f + randomRotationRange1dThisTrial, 0) * (
                new Vector3(
                    leftHandAnchor.transform.position.x,
                    rightHandAnchor.transform.position.y,
                    leftHandAnchor.transform.position.z) - rotatingArmSpace.transform.position);
            Vector3 dirToEnd = Quaternion.Euler(0, trial.settings.GetFloat("targetDegrees"), 0) * dirToStart;
            D1targetSpaceRotation.transform.rotation = Quaternion.LookRotation(dirToStart);
            A1d.transform.rotation = Quaternion.LookRotation(dirToStart);
            B1d.transform.rotation = Quaternion.LookRotation(dirToEnd);


            positionToOffsetFrom.transform.position = A1dPos.transform.position;
            // positionToOffsetFrom.transform.rotation = A1d.transform.rotation;

            A1dPosScript.Visible(true);
            B1dPosScript.Visible(true);
            A1dPosScript.ColorObj(startTeal);
            B1dPosScript.ColorObj(standbyGrey);
            bool readyButtonPressed = false;
            float timer = 0f;
            float breakTimer = UnityEngine.Random.Range(0.5f, 0.75f);
            Quaternion initiateRotation = A1d.transform.rotation;
            while(true)
            {
                float angle = Mathf.Abs(CalcAngleDirectional(positionProjected, A1dPos, rotatingArmSpace));
                if (Input.GetKeyDown(KeyCode.P))
                {
                    break;
                }
                if (timer > breakTimer)
                {
                    break;
                }
                if (angle < 2.5)
                {
                    A1dPosScript.ColorObj(activeBlue);
                    if (OVRInput.GetDown(OVRInput.Button.Two) | readyButtonPressed)
                    {
                        if (!readyButtonPressed)
                        {
                            initiateRotation = rotationProjected.transform.rotation;
                            D1targetSpaceRotation.transform.rotation = rotationProjected.transform.rotation;
                            arduinoReciever.ResetSerialQueue();
                            arduinoReciever.saving = true;
                        }
                        // Vector3 dirToStartAdjusted = 
                        // Vector3 dirToEndAdjusted = Quaternion.Euler(0, trial.settings.GetFloat("targetDegrees"), 0) * dirToStart;
                        readyButtonPressed = true;
                        A1dPosScript.ColorObj(readyYellow);
                        B1dPosScript.ColorObj(readyYellow);
                        timer += Time.deltaTime;
                        //Debug.Log($"Keep stationary for {0.5f - timer}");
                    }
                }
                else if (readyButtonPressed && angle > 0.5)
                {
                    Debug.Log("angle exceeded triggered: " + angle);
                    timer = 0f;
                    readyButtonPressed = false;
                    A1dPosScript.ColorObj(startTeal);
                    B1dPosScript.ColorObj(standbyGrey);
                    D1targetSpaceRotation.transform.rotation = Quaternion.LookRotation(dirToStart);
                    if (arduinoReciever.saving)
                    {
                        arduinoReciever.ResetSerialQueue();
                        arduinoReciever.saving = false;
                    }
                }
                else
                {
                    timer = 0f;
                    readyButtonPressed = false;
                    A1dPosScript.ColorObj(startTeal);
                    B1dPosScript.ColorObj(standbyGrey);
                    D1targetSpaceRotation.transform.rotation = Quaternion.LookRotation(dirToStart);
                    if (arduinoReciever.saving)
                    {
                        arduinoReciever.ResetSerialQueue();
                        arduinoReciever.saving = false;
                    }
                }
                yield return null;
            }
            trialProgress = "trialStarted";
            trialStartedTime = Time.time;
            positionOffsetProjectedScript.Visible(trial.settings.GetBool("controllerVisibleTrialStart"));
            //angleMultiplier = trial.settings.GetFloat("angleMultiplier");
            vibrationCR = StartCoroutine(Vibration(trial));
            Debug.Log($"vibration set to {trial.settings.GetString("vibration")}");
            
            A1dPosScript.Visible(false);
            B1dPosScript.ColorObj(targetGreen);
            Debug.Log($"Trial 1d started");

            if (trial.settings.GetBool("rotationPacer"))
            {
                float priorDegs = 40f;
                float extraDegs = 20f;
                float duration = 2.4f;
                float totalDur = duration * (Mathf.Abs(priorDegs) + (Mathf.Abs(extraDegs)) + Mathf.Abs(trial.settings.GetInt("targetDegrees"))) / trial.settings.GetInt("targetDegrees");
                Vector3 dirToPacerStart = rotatingArmSpace.transform.position + Quaternion.Euler(0, -priorDegs, 0) * (A1dPos.transform.position - rotatingArmSpace.transform.position);
                Vector3 dirToPacerEnd = rotatingArmSpace.transform.position + Quaternion.Euler(0, extraDegs, 0) * (B1dPos.transform.position - rotatingArmSpace.transform.position);
                rotatePacerCR = StartCoroutine(RotatePacer(
                    dirToPacerStart, 
                    dirToPacerEnd, 
                    totalDur)
                    );
            }
            yield return new WaitUntil(() => (angleStartToController > 10f));
            yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
            arduinoReciever.saving = false;
            trialEndButtonPressed = true;
            trialProgress = "trialInput";
            trialInputTime = Time.time;

            B1dPosScript.ColorObj(standbyGrey);
            A1dPosScript.ColorObj(startTeal);
            yield return new WaitUntil(() => (angleStartToController < 10f));

        }
        else if (nDims == 2)
        {
            tracker2d.SetActive(true);
            // reset trialspace (2d)
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
            trialSpaceRotatedMovementSpace.transform.position = trialSpaceSetup.transform.position;
            trialSpaceRotatedMovementSpace.transform.rotation = trialSpaceSetup.transform.rotation;
            trialTarget.transform.rotation = trialSpace.transform.rotation;

            trialTarget.transform.localPosition = new Vector3(
                0f,
                0f,
                trial.settings.GetFloat("targetDistance")
            );
            

            if (trial.settings.GetBool("applyRandomRotation"))
            {
                float randomRotationRange = trial.settings.GetFloat("randomRotationRange");
                trialSpaceRotationY = UnityEngine.Random.Range(-randomRotationRange, randomRotationRange);
                trialSpace.transform.Rotate(0f, trialSpaceRotationY, 0f);
            }

            startPos = trialStart.transform.position;
            targetPos = trialTarget.transform.position;
            controllerSphereScript.Visible(true);
            trialStartScript.Visible(true);
            trialTargetScript.Visible(true);
            Debug.Log("trialTargetScript.Visible(true);");
            trialTargetScript.ColorObj(standbyGrey);
            trialStartScript.ColorObj(startTeal);
            controllerSphereScript.ResetVisualOffset();

            float breakTimer = UnityEngine.Random.Range(0.5f, 0.75f);
            float timer = 0f;
            bool readyButtonPressed = false;
            Vector3 initiatePosition = new Vector3();
            Vector3 trialStartPos = new Vector3();
            trialStartPos = trialStart.transform.position;

            // Wait for proper distance to start and ready button to be pressed. Trial will start after delay from press.
            while(true)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    break;
                }
                if (timer > breakTimer)
                {
                    break;
                }
                if (Vector3.Distance(trialStartPos, controllerSphere.transform.position) < 0.025)
                {
                    trialStartScript.ColorObj(activeBlue);
                    if (OVRInput.GetDown(OVRInput.Button.Two) | readyButtonPressed)
                    {
                        if (!readyButtonPressed)
                        {
                            initiatePosition = rightHandAnchor.transform.position;
                        }
                        arduinoReciever.saving = true;
                        trialSpace.transform.position = rightHandAnchor.transform.position;
                        readyButtonPressed = true;
                        trialStartScript.ColorObj(readyYellow);
                        trialTargetScript.ColorObj(readyYellow);
                        timer += Time.deltaTime;
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
                    if (arduinoReciever.saving)
                    {
                        arduinoReciever.ResetSerialQueue();
                        arduinoReciever.saving = false;
                    }                }
                else
                {
                    timer = 0f;
                    readyButtonPressed = false;
                    trialStartScript.ColorObj(startTeal);
                    trialTargetScript.ColorObj(standbyGrey);
                    trialSpace.transform.position = trialSpaceSetup.transform.position;
                    if (arduinoReciever.saving)
                    {
                        arduinoReciever.ResetSerialQueue();
                        arduinoReciever.saving = false;
                    }                }
                yield return null;
            }

            trialProgress = "trialStarted";
            trialStartedTime = Time.time;

            if (trial.settings.GetInt("nTargets") == 1)
            {
                vibrationCR = StartCoroutine(Vibration(trial));
                trialStartScript.Visible(false);
                controllerSphereScript.Visible(trial.settings.GetBool("controllerVisibleTrialStart"));
                trialTargetScript.Visible(trial.settings.GetBool("targetVisibleTrialStart"));
                Debug.Log($"trialTargetScript.Visible() set to {trial.settings.GetBool("targetVisibleTrialStart")}");


                // controllerSphereScript.visualOffsetFromReference(new Vector3(
                //     trial.settings.GetFloat("visualXOffset"), 
                //     0, 
                //     trial.settings.GetFloat("visualZOffset")),
                //     trialSpace.transform);

                trialSpaceRotatedMovementSpace.transform.Rotate(
                    0f,
                    trial.settings.GetFloat("visualXrotation"),
                    0f);

                trialTarget.transform.RotateAround(
                    trialSpace.transform.position,
                    Vector3.up,
                    trial.settings.GetFloat("visualTargetXrotation")
                );
                trialTargetScript.ColorObj(targetGreen);
                // Trial is now initiated
                // Controller visibility
                yield return new WaitUntil(() =>(GetRelativePosition(trialSpace.transform, rightHandAnchor.transform).z > trial.settings.GetFloat("controllerMidpointOnStart")));
                StartCoroutine(ControllerVisibility(trial));

                // Ensure input button is not fat fingered immediately before input is allowed
                yield return new WaitUntil(() => (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) > 0.1f));
                yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
                trialEndButtonPressed = true;
            }

            else if (trial.settings.GetInt("nTargets") > 1)
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
            arduinoReciever.saving = false;
            trialProgress = "trialInput";
            trialInputTime = Time.time;

            if (trial.settings.GetBool("userInputSelector"))
            {
                UserInputSelectorCRComplete = false;
                StartCoroutine(UserInputSelectorCR());
                yield return new WaitUntil(() => UserInputSelectorCRComplete);
            }

            trialStartScript.Visible(true);
            trialTargetScript.PositionAnchor(targetPos);
            trialTargetScript.ColorObj(standbyGrey);
            trialStartScript.ColorObj(startTeal);
            controllerSphereScript.ResetVisualOffset();

            yield return new WaitUntil(() => (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) < 0.1f));
        }
        else
        {
            Debug.LogError("nDims must be 1 or 2");
        }
        if (vibrationCR != null)
        {
            StopCoroutine(vibrationCR);
            if (vibLeft.isPlaying)
            {
                vibLeft.Stop();
            }
            if (vibRight.isPlaying)
            {
                vibRight.Stop();
            }
            if (vibBoth.isPlaying)
            {
                vibBoth.Stop();
            }
            Debug.LogWarning("VibrationCR stopped from main trial CR");
        }
        if (rotatePacerCR != null)
        {
            StopCoroutine(rotatePacerCR);
            positionPacerScript.Visible(false);
            Debug.LogWarning("rotatePacerCR stopped from main trial CR");
        }

        SaveResults(trial);
        arduinoReciever.SaveDataFrame(trial);

        yield return new WaitForSeconds(0.1f);
        Debug.Log($"trialEndButtonPressed is now 2: {trialEndButtonPressed}");
        trial.End();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(ConfirmQuitCR());
        }

        if (nDims == 1 & calibrationArmrestComplete)
        {

            angleStartToController = CalcAngleDirectional(positionToOffsetFrom, rightHandAnchor, rotatingArm);
            angleStartToOffsetController = CalcOffsetAngle(
                angleStartToController,
                Session.instance.CurrentTrial.settings.GetFloat("angleMultiplier"),
                Session.instance.CurrentTrial.settings.GetFloat("visualSpeedOffsetStartFraction"),
                Session.instance.CurrentTrial.settings.GetFloat("visualSpeedOffsetEndFraction"),
                Session.instance.CurrentTrial.settings.GetFloat("targetDegrees")
            );
            // float angleChange = angleStartToController - angleStartToControllerLastFrame;
            // float movementFraction = (angleStartToController / Session.instance.CurrentTrial.settings.GetFloat("targetDegrees"));
            // if (movementFraction >= Session.instance.CurrentTrial.settings.GetFloat("visualSpeedOffsetStartFraction") &&
            //     movementFraction <= Session.instance.CurrentTrial.settings.GetFloat("visualSpeedOffsetEndFraction"))
            // {
            //     angleOffset = angleOffset + (angleChange * Session.instance.CurrentTrial.settings.GetFloat("angleMultiplier"));
            //     //angleStartToOffsetController = angleStartToOffsetController + (angleChange * angleMultiplier);
            // }
            // else
            // {
            //     angleOffset = angleOffset + angleChange;
            //     //angleStartToOffsetController = angleStartToOffsetController + (angleChange);
            // }
            // angleStartToOffsetController = angleOffset;
            angleStartToPacer = CalcAngleDirectional(positionToOffsetFrom, positionPacer, rotatingArm);

            Vector3 dirToController = rightHandAnchor.transform.position - rotatingArm.transform.position;
            Vector3 dirToStart = positionToOffsetFrom.transform.position - rotatingArm.transform.position;
            Vector3 dirToOffsetController = Quaternion.Euler(0, angleStartToOffsetController, 0) * dirToStart;
            // Vector3 dirToOffsetController = Quaternion.Euler(0, angleOffset, 0) * dirToController;
            //rotatingArm.transform.forward = dirToStart;

            rotatingArm.transform.forward = dirToOffsetController;
            rotationProjected.transform.forward = dirToController;
            // angleStartToOffsetController = CalcAngleDirectional(positionToOffsetFrom, positionOffsetProjected, rotatingArm);
            // angleStartToControllerLastFrame = CalcAngleDirectional(positionToOffsetFrom, rightHandAnchor, rotatingArm);
        }

        // debug objs
        AudioLatencyTester.SetActive(debugAudio);
        VRConsole.SetActive(showConsole);
        debugSphereScript.Visible(showDebugSphere);
        // positionToOffsetFromScript.Visible(showDebugSpheresRotation);
        positionProjectedScript.Visible(showDebugSphere);
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
        StartCoroutine(calibrateRotatingArmLocation());
        yield return new WaitUntil(() => (calibrationArmrestComplete));
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
        }
        if (trial.settings.GetBool("turnTargetVisibleMidpoint"))
        {
            trialTargetScript.Visible(true);
        }

        trialProgress = "trialControlVisibilityOn";
        trialControlVisibilityOnTime = Time.time;
        for (int i = 0; i < trial.settings.GetInt("controllerMidpointVisibleFrames"); i++)
        {
            yield return null;
        }
        trialProgress = "trialControlVisibilityOff";
        trialControlVisibilityOffTime = Time.time;

        if (trial.settings.GetBool("turnControllerVisibleMidpoint"))
        {
            controllerSphereScript.Visible(false);
        }
        if (trial.settings.GetBool("turnTargetVisibleMidpoint"))
        {
            trialTargetScript.Visible(false);
        }
    }
    

    IEnumerator Vibration(Trial trial)
    {
        string vibration = trial.settings.GetString("vibration");
        int nDims = trial.settings.GetInt("nDims");
        if (trial.settings.GetFloat("vibrationStartFraction") > 0f)
        {
            while (true)
            {
                if (nDims == 1)
                {
                    float currFrac = Mathf.Abs(angleStartToPacer)/Mathf.Abs(trial.settings.GetFloat("targetDegrees"));
                    // Debug.Log($"currFrac {currFrac}, break frac is {trial.settings.GetFloat("vibrationStartFraction")}");
                    if (currFrac >= trial.settings.GetFloat("vibrationStartFraction"))
                    {
                        // Debug.Log($"break cond triggered");
                        break;
                    }
                }
                else if (nDims == 2)
                {
                    float currFrac = controllerSphere.transform.localPosition.z / trialTarget.transform.localPosition.z;
                    // Debug.Log($"currFrac {currFrac}, break frac is {trial.settings.GetFloat("vibrationStartFraction")}");
                    if (currFrac >= trial.settings.GetFloat("vibrationStartFraction"))
                    {
                        // Debug.Log($"break cond triggered");
                        break;
                    }
                }
                yield return null;
            }
        }
        trialVibStart = Time.time + FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds/1000.0;
        arduinoReciever.SendTrigger("true");
        if (vibration == "biceps")
        {
            vibLeft.Play();
        }
        else if (vibration == "triceps")
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
        Debug.Log($"Vibration {vibration} started");

        while (true)
        {
            if (trialEndButtonPressed)
            {
                break;
            }
            if (nDims == 1 && trial.settings.GetFloat("vibrationEndFraction") < 1.0f)
            {
                float currFrac = Mathf.Abs(angleStartToPacer)/Mathf.Abs(trial.settings.GetFloat("targetDegrees"));
                // Debug.Log($"currFrac {currFrac}, break frac is {trial.settings.GetFloat("vibrationEndFraction")}");
                if (currFrac >= trial.settings.GetFloat("vibrationEndFraction"))
                {
                    break;
                }
            }
            else if (nDims == 2)
            {
                float currFrac = controllerSphere.transform.localPosition.z / trialTarget.transform.localPosition.z;
                // Debug.Log($"currFrac {currFrac}, break frac is {trial.settings.GetFloat("vibrationEndFraction")}");
                if (currFrac >= trial.settings.GetFloat("vibrationEndFraction"))
                {
                    break;
                }
            }
            yield return null;
        }
        // Debug.Log("Vibration coroutine progressed");
        trialVibStop = Time.time + FrameTimer.FrameStopwatch.Elapsed.TotalMilliseconds/1000.0;
        arduinoReciever.SendTrigger("false");
        if (vibLeft.isPlaying)
        {
            vibLeft.Stop();
        }
        if (vibRight.isPlaying)
        {
            vibRight.Stop();
        }
        if (vibBoth.isPlaying)
        {
            vibBoth.Stop();
        }
        Debug.Log($"vibration {vibration} stopped, with duration: {trialVibStop - trialVibStart}");
    }

    IEnumerator BlockInstructions(Trial trial)
    {
        controllerSphereScript.Visible(false);
        GameObject b_instructionsArrow = null;
        if (nDims == 1)
        {
            offsetControllerAnchor.SetActive(true);
            b_instructionsArrow = generateInstructions.InstantiateArrowText(
                buttonBoffset,
                "Starting new task!\n Look up and read instructions. Press and hold here when ready to start.",
                true
            );
        }
        else if (nDims == 2)
        {
            rightControllerAnchor.SetActive(true);
            b_instructionsArrow = generateInstructions.InstantiateArrowText(
                buttonB,
                "Starting new task!\n Look up and read instructions. Press and hold here when ready to start.",
                true
                );
        }

        string instructions = "Block instructions: \n" + trial.settings.GetString("blockInstructions");
        string instructions2 = "\n\nWhen you have read the instructions and is ready to start the task, " +
        "press and hold B for 1 second.";
        blockInstructionsText.text = instructions + instructions2;
        Debug.Log(instructions + instructions2);
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
        offsetControllerAnchor.SetActive(false);
        rightControllerAnchor.SetActive(false);
        controllerSphereScript.Visible(true);
        blockInstructionsComplete = true;
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
    // related to rotating armrest
    IEnumerator calibrateRotatingArmLocation()
    {
        rotatingArmObj.SetActive(false);
        Debug.LogWarning("Calibration started");
        calibrationArmrestComplete = false;
        GameObject instructionsArrow1 = generateInstructions.InstantiateArrowText(
            buttonA,
            "Move armrest to the left\nThen press A to start calibration",
            true
        );
        yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
        Debug.LogWarning("Calibration start key pressed");
        Destroy(instructionsArrow1);
        positionToOffsetFrom.transform.position = rightHandAnchor.transform.position;
        positionToOffsetFrom.transform.rotation = rightHandAnchor.transform.rotation;

        StartCoroutine(SavePointsForCalibration());
        yield return new WaitUntil(() => (savePointsForCalibrationComplete == true));
        Debug.LogWarning("Calibration save points for calibration complete");
        CalibrateCircleCenter(pointsForCalibration);
        

        calibrationArmrestComplete = true;
        rotatingArmObj.SetActive(false);
        Debug.LogWarning("Calibration completed");
        Debug.LogWarning("radius " + radius);
        Debug.LogWarning("radius SD " + radiusSD);
    }
    IEnumerator SavePointsForCalibration()
    {
        savePointsForCalibrationComplete = false;
        float lastAngleDiff = 0;


        pointsForCalibration = new List<Vector3>();
        Quaternion initialRotation = rightHandAnchor.transform.rotation;

        pointsForCalibration.Add(rightHandAnchor.transform.position);

        GameObject instructionsArrow = generateInstructions.InstantiateArrowText(
            buttonA,
            "Move " + angleToCalibrateWith + " to the right",
            true
        );
        TextMeshPro instructionsArrowText = instructionsArrow.GetComponentInChildren<TextMeshPro>();

        while (Quaternion.Angle(rightHandAnchor.transform.rotation, initialRotation) < angleToCalibrateWith)
        {
            float angleDiff = Quaternion.Angle(rightHandAnchor.transform.rotation, initialRotation);
            instructionsArrowText.text = "Move " + Mathf.Round((float)(angleToCalibrateWith - Quaternion.Angle(rightHandAnchor.transform.rotation, initialRotation))) + " to the right";

            if (angleDiff - lastAngleDiff > 0.5f)
            {
                pointsForCalibration.Add(rightHandAnchor.transform.position);
                lastAngleDiff = angleDiff;
            }
            if (OVRInput.GetDown(OVRInput.Button.One)) // reset if necessary
            {
                lastAngleDiff = 0;

                pointsForCalibration = new List<Vector3>();
                initialRotation = rightHandAnchor.transform.rotation;

                pointsForCalibration.Add(rightHandAnchor.transform.position);
            }
            yield return null;
        }
        Destroy(instructionsArrow);
        savePointsForCalibrationComplete = true;
    }
    IEnumerator RotatePacer(Vector3 startPos, Vector3 endPos, float duration)
    {
        float starttime = Time.time;
        positionPacerScript.ColorObj(activeBlue);
        Debug.Log("rotatePacer started");
        positionPacerScript.Visible(true);
        Quaternion startRotation = Quaternion.LookRotation(startPos - rotationPacer.transform.position);
        Quaternion endRotation = Quaternion.LookRotation(endPos - rotationPacer.transform.position);

        float elapsedTime = 0f;
        //float duration = Mathf.Abs(Quaternion.Angle(startRotation, endRotation))/pacerDegsPerSec;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration; 
            // float easedT = Easing.IntegratedLognormal(t, 0.3f, 0.7f); 
            // float easedT = Easing.SymmetricQuad(t); 
            rotationPacer.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            // rotationPacer.transform.rotation = Quaternion.Slerp(startRotation, endRotation, easedT);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rotationPacer.transform.rotation = endRotation;
        positionPacerScript.Visible(false);
        Debug.Log($"total pacer duration set to {Time.time - starttime}, intended dur was {duration}");
    }

    IEnumerator UserInputSelectorCR()
    {
        UserInputSelector.SetActive(true);
        int selectedPane = 0;
        GameObject UserInputSelector1 = UserInputSelector.transform.Find("UserInputSelector1").gameObject;
        GameObject UserInputSelector3 = UserInputSelector.transform.Find("UserInputSelector3").gameObject;

        UserInputSelector1.GetComponent<Renderer>().material = standbyGrey;
        UserInputSelector3.GetComponent<Renderer>().material = standbyGrey;

        TextMeshPro UserInputSelectorText = UserInputSelector.GetComponentInChildren<TextMeshPro>();
        string currentInput = "____";
        UserInputSelectorText.text = $"Please indicate if you think the virtual controller was shown to the {currentInput} compared to you actual hand";

        TextMeshPro lChoiceTMP = UserInputSelector1.GetComponentInChildren<TextMeshPro>();
        TextMeshPro rChoiceTMP = UserInputSelector3.GetComponentInChildren<TextMeshPro>();
        lChoiceTMP.text = "left";
        rChoiceTMP.text = "right";

        // select whether virtual controller is believed to be slower or faster compared to real hand
        while (true)
        {
            float stickX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;
            float stickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;

            // select left (1) pane
            if (stickX < -0.75)
            {
                selectedPane = 1;
                currentInput = "[left]";
                UserInputSelectorText.text = $"Please indicate if you think the virtual controller was shown to the {currentInput} compared to you actual hand";
                UserInputSelector1.GetComponent<Renderer>().material = targetGreen;
                UserInputSelector3.GetComponent<Renderer>().material = standbyGrey;
            }
            // select right (3) pane
            if (stickX > 0.75)
            {
                selectedPane = 3;
                currentInput = "[right]";
                UserInputSelectorText.text = $"Please indicate if you think the virtual controller was shown to the {currentInput} compared to you actual hand";
                UserInputSelector1.GetComponent<Renderer>().material = standbyGrey;
                UserInputSelector3.GetComponent<Renderer>().material = targetGreen;
            }

            // deselect pane
            if (stickY < -0.75)
            {
                selectedPane = 0;
                currentInput = "____";
                UserInputSelectorText.text = $"Please indicate if you think the virtual controller was shown to the {currentInput} compared to you actual hand";
                UserInputSelector1.GetComponent<Renderer>().material = standbyGrey;
                UserInputSelector3.GetComponent<Renderer>().material = standbyGrey;
            }
            // confirm selection and break
            if (selectedPane > 0 && OVRInput.GetDown(OVRInput.Button.One))
            {
                Session.instance.CurrentTrial.result["userInput"] = selectedPane;
                Session.instance.CurrentTrial.result["trialUserInputTimme"] = Time.time;
                break;
            }
            yield return null;
        }
        Debug.Log("selected slower/faster: "+ selectedPane);

        UserInputSelector.SetActive(false);
        UserInputSelectorCRComplete = true;
    }
    private void CalibrateCircleCenter(List<Vector3> points, int nSets = 10)
    {
        if (points.Count % 3 != 0)
        {
            int excess = points.Count % 3;
            points.RemoveRange(points.Count - excess, excess);
        }

        int segmentLength = points.Count / 3;
        List<Vector3> pointsA = points.GetRange(0, segmentLength);
        List<Vector3> pointsB = points.GetRange(segmentLength, segmentLength);
        List<Vector3> pointsC = points.GetRange(2 * segmentLength, segmentLength);
        
        int loopCount = Mathf.Min(nSets, segmentLength);
        List<Vector3> centers = new List<Vector3>();

        for (int i = 0; i < loopCount; i++)
        {
            Vector3 p1 = new Vector3(pointsA[i].x, 0, pointsA[i].z);
            Vector3 p2 = new Vector3(pointsB[i].x, 0, pointsB[i].z);
            Vector3 p3 = new Vector3(pointsC[i].x, 0, pointsC[i].z);

            Vector3 mid12 = (p1 + p2) * 0.5f;
            Vector3 mid23 = (p2 + p3) * 0.5f;

            Vector3 dir12 = p2 - p1;
            Vector3 dir23 = p3 - p2;

            Vector3 perp12 = new Vector3(-dir12.z, 0, dir12.x);
            Vector3 perp23 = new Vector3(-dir23.z, 0, dir23.x);

            float a1 = perp12.x;
            float b1 = -perp23.x;
            float c1 = mid23.x - mid12.x;

            float a2 = perp12.z;
            float b2 = -perp23.z;
            float c2 = mid23.z - mid12.z;

            float denominator = a1 * b2 - a2 * b1;
            if (Mathf.Abs(denominator) < float.Epsilon)
            {
                continue;
            }

            float t = (c1 * b2 - c2 * b1) / denominator;
            Vector3 center = mid12 + perp12 * t;

            centers.Add(center);
        }

        // Average over centers
        Vector3 centersSum = Vector3.zero;
        foreach (Vector3 center in centers)
        {
            centersSum += center;
        }
        Vector3 aveCenter = centersSum / centers.Count;
        aveCenter = new Vector3(aveCenter.x, rightHandAnchor.transform.position.y, aveCenter.z);
        // Average of radii to find likely, and to check distribution
        List<float> radii = new List<float>();
        foreach (Vector3 point in points)
        {
            float dist = Vector3.Distance(point, aveCenter);
            radii.Add(dist);
        }

        // Calculate mean radius
        float aveRadius = radii.Average();
        radius = aveRadius;

        // Calculate standard deviation
        float sumOfSquares = radii.Sum(radius => Mathf.Pow(radius - aveRadius, 2));
        radiusSD = Mathf.Sqrt(sumOfSquares / radii.Count);

        Vector3 positionToOffsetFromPos = positionToOffsetFrom.transform.position;

        // Set center of rotating armrest
        rotatingArmSpace.transform.position = aveCenter;
        rotatingArmSpace.transform.eulerAngles = new Vector3(0, 0, 0);
        // Set grip to correct distance
        gripModel.transform.localPosition = new Vector3(0, 0, -(radius+0.003f));
        positionProjected.transform.localPosition = new Vector3(0, 0, radius);
        positionOffsetProjected.transform.localPosition = new Vector3(0, 0, radius);
        positionPacer.transform.localPosition = new Vector3(0, 0, radius);
        A1dPos.transform.localPosition = new Vector3(0, 0, radius);
        B1dPos.transform.localPosition = new Vector3(0, 0, radius);

        positionToOffsetFrom.transform.position = new Vector3(positionToOffsetFromPos.x, rotatingArm.transform.position.y, positionToOffsetFromPos.z);
        angleStartToController = CalcAngleDirectional(positionToOffsetFrom, rightHandAnchor, rotatingArm);
        // angleStartToOffsetController = angleStartToController * Session.instance.CurrentTrial.settings.GetFloat("angleMultiplier");
        angleStartToOffsetController = CalcOffsetAngle(
            angleStartToController,
            Session.instance.CurrentTrial.settings.GetFloat("angleMultiplier"),
            0f,
            1f,
            80f);
        Vector3 dirToController = rightHandAnchor.transform.position - rotatingArm.transform.position;
        Vector3 dirToStart = positionToOffsetFrom.transform.position - rotatingArm.transform.position;
        Vector3 dirToOffsetController = Quaternion.Euler(0, angleStartToOffsetController, 0) * dirToStart;
        //rotatingArm.transform.forward = dirToStart;
        rotatingArm.transform.forward = dirToOffsetController;

        Quaternion rightHandAnchorRotation = rightHandAnchor.transform.rotation;

        offsetControllerAnchor.transform.rotation = rightHandAnchorRotation;

        Vector3 buttonAVisualOffset = buttonA.transform.localPosition;

        // A1dPosScript.PositionSphere(buttonAVisualOffset);
        //A1dPosScript.RotateAnchor(rightHandAnchorRotation);
        
        // B1dPosScript.PositionSphere(buttonAVisualOffset);
       //B1dPosScript.RotateAnchor(rightHandAnchorRotation);
        
        // positionPacerScript.PositionSphere(buttonAVisualOffset);
        //positionPacerScript.RotateAnchor(rightHandAnchorRotation);
        // Debug.Log($"visual offset of {buttonAVisualOffset} applied");
        
    }
    private float CalcAngleDirectional(GameObject At, GameObject Bt, GameObject Ct)
    {
        Vector3 A = At.transform.position;
        Vector3 B = Bt.transform.position;
        Vector3 C = Ct.transform.position;

        // Get the normalized direction vectors
        Vector3 vectorAC = (A - C).normalized;
        Vector3 vectorBC = (B - C).normalized;

        // Calculate the dot product of the two vectors. The dot product is equal to the product of the magnitudes of the two vectors and the cosine of the angle between them.
        float dot = Vector3.Dot(vectorAC, vectorBC);
        // Calculate the determinant, which is a measure of the signed area formed by the two vectors. In this case, it's similar to finding the y-component of the cross product of the two vectors. This will help us determine the direction of rotation (clockwise or counter-clockwise) around the Y-axis.
        float det = vectorAC.x * vectorBC.z - vectorAC.z * vectorBC.x;
        // Calculate the angle between the two vectors in radians using the atan2 function, which considers the direction of rotation. Then convert the angle to degrees using Mathf.Rad2Deg. Times -1 to get pos angles for cw rotation.
        float angle = Mathf.Atan2(det, dot) * Mathf.Rad2Deg*-1;

        return angle;
    }
    private float CalcOffsetAngle(float currentAngle, float multiplier, float startOffsetFraction, float endOffsetFraction, float targetAngle)
    {
        float startOffsetAngle = targetAngle * startOffsetFraction;
        float endOffsetAngle = targetAngle * endOffsetFraction;

        float offsetAngle;
        if (currentAngle <= startOffsetAngle)
        {
            offsetAngle = currentAngle;
        }
        else if (currentAngle <= endOffsetAngle)
        {
            offsetAngle = startOffsetAngle + ((currentAngle - startOffsetAngle) * multiplier);
        }
        else
        {
            offsetAngle = startOffsetAngle 
                + ((endOffsetAngle - startOffsetAngle) * multiplier) 
                + (currentAngle - endOffsetAngle);
        }
        return offsetAngle;
    }
    public class Easing
    {
        public static float IntegratedLognormal(float t, float peak, float sigma, int steps = 100)
        {
            t = Mathf.Clamp01(t);
            float dt = 1f / steps;

            // First: integrate the full curve from 0 to 1
            float maxIntegration = 0f;
            for (int i = 0; i < steps; i++)
            {
                float currentT = i * dt;
                float nextT = (i + 1) * dt;
                float currentValue = Lognormal(currentT, peak, sigma);
                float nextValue = Lognormal(nextT, peak, sigma);
                maxIntegration += (currentValue + nextValue) * 0.5f * dt;
            }

            // Then integrate from 0 to t
            float partialIntegration = 0f;
            int fullSteps = Mathf.FloorToInt(t * steps);
            for (int i = 0; i < fullSteps; i++)
            {
                float currentT = i * dt;
                float nextT = (i + 1) * dt;
                float currentValue = Lognormal(currentT, peak, sigma);
                float nextValue = Lognormal(nextT, peak, sigma);
                partialIntegration += (currentValue + nextValue) * 0.5f * dt;
            }

            // If t doesn't land exactly on a step boundary, integrate the remainder fraction
            float remainder = (t * steps) - fullSteps;
            if (remainder > 0f)
            {
                float currentT = fullSteps * dt;
                float nextT = t;
                float currentValue = Lognormal(currentT, peak, sigma);
                float nextValue = Lognormal(nextT, peak, sigma);
                float partialDt = nextT - currentT;
                partialIntegration += (currentValue + nextValue) * 0.5f * partialDt;
            }

            return partialIntegration / maxIntegration;

        }

        private static float Lognormal(float t, float peak, float sigma)
        {
            // Avoid log(0) by clamping
            t = Mathf.Clamp(t, 0.0001f, 1.0f);
            float mu = Mathf.Log(peak); // Mean to align the peak
            return Mathf.Exp(-Mathf.Pow(Mathf.Log(t) - mu, 2) / (2 * sigma * sigma))
                / (t * sigma * Mathf.Sqrt(2 * Mathf.PI));
        }
        public static float SymmetricQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }
        
    }
}
