using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UXF;

using TMPro;

public class TaskRunner : MonoBehaviour
{
    // Objects and attached scripts
    private Config c;
    public GenerateInstructions generateInstructions;
    public GameObject trialSpaceSetup;
    public GameObject trialSpace;
    private ControlSphere trialSpaceScript;
    public GameObject trialTarget;
    private ControlSphere trialTargetScript;
    public GameObject trialStart;
    private ControlSphere trialStartScript;
    public GameObject controllerSphere;
    private ControlSphere controllerSphereScript;
    public GameObject trackedDesk;
    private PositionDesk trackedDeskScript;
    public GameObject rightControllerAnchor;
    public GameObject rightHandAnchor;
    public GameObject leftControllerAnchor;
    public GameObject leftHandAnchor;
    public GameObject buttonA;
    public GameObject tracker2d;
    public GameObject tracker1d;
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
    public string trialProgress;
    public string trialID;

    // Coroutines
    private Coroutine currentSubTrialCR;
    private Coroutine currentTrialCR;
    private Coroutine calibrationCR;

    // Timers
    private float trialSetupTime;
    private float trialStartedTime;
    private float trialControlVisibilityOffTime;
    private float trialControlVisibilityOnTime;
    private float trialInputTime;

    // Others
    private Vector3 posControllerInTrialSpace;
    public TextMeshPro trialCounter;

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
        trackedDeskScript = trackedDesk.GetComponent<PositionDesk>();

        trialStartScript.visible = false;
        trialTargetScript.visible = false;
        controllerSphereScript.visible = false;
        tracker1d.SetActive(false);
        tracker2d.SetActive(false);
    }

    public void StartTrial(Trial trial)
    {
        //currentTrial = StartCoroutine(TrialCoroutine(trial));
        currentTrialCR = StartCoroutine(TrialCoroutine(trial));
    }

    public void EndTrialCoroutine()
    {
        StopCoroutine(currentTrialCR);
        StopCoroutine(currentSubTrialCR);
    }

    IEnumerator TrialCoroutine(Trial trial)
    {
        trialSpace.transform.position = trialSpaceSetup.transform.position;
        
        if (trial.settings.GetInt("nDims") == 1)
        {
            tracker1d.SetActive(true);
        }
        if (trial.settings.GetInt("nDims") == 2)
        {
            tracker2d.SetActive(true);
        }
        trialID = $"{Session.instance.ppid}_{Session.instance.experimentName}_{Session.instance.number}_{Session.instance.currentBlockNum}_{Session.instance.currentTrialNum}";

        if (trial.numberInBlock == 1)
        {
            Debug.Log("Block: ");
        }
        trialCounter.text = "Trial " + trial.number + "/" + Session.instance.LastTrial.number;
        Debug.Log("Trial " + trial.number + " started.");
        Debug.Log("visualXOffset set to " + trial.settings.GetFloat("visualXOffset"));

        // calibration
        if (trial.settings.GetBool("calibration"))
        {
            trialProgress = "calibrationStarted";
            Debug.Log("Calibration started");
            if (trial.settings.GetInt("nDims") == 2)
            {
                calibrationCR = StartCoroutine(calibration2d());
                yield return new WaitUntil(() => (calibrationComplete));
                StopCoroutine(calibrationCR);

            }
            leftControllerAnchor.SetActive(false);
            Debug.Log("Calibration completed");
            trial.End();
        }

        // trial setup
        trialProgress = "trialSetup";
        trialSetupTime = Time.time;

        startPos = trialStart.transform.position;
        targetPos = new Vector3(0f, 0f, 0.4f);
        controllerSphereScript.visible = true;
        trialStartScript.visible = true;
        trialTargetScript.visible = true;
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

        // Trial is now initiated
        if (trial.settings.GetInt("nTargets") == 1)
        {
            currentSubTrialCR = StartCoroutine(TrialSingleTargetCoroutine(trial));
        }
        if (trial.settings.GetInt("nTargets") > 1)
        {
            currentSubTrialCR = StartCoroutine(TrialMultiTargetCoroutine(trial));
        }

    }
    IEnumerator TrialSingleTargetCoroutine(Trial trial)
    {
        Debug.Log("target local: " + trialTarget.transform.localPosition);

        trialProgress = "trialStarted";
        trialStartedTime = Time.time;

        trialStartScript.visible = false;
        if (!(trial.settings.GetBool("controllerVisibleTrialStart")))
        {
            controllerSphereScript.visible = false;
        }
        yield return null;
        controllerSphereScript.visualOffsetFromReference(new Vector3(
            trial.settings.GetFloat("visualXOffset"), 
            0, 
            0),
            trialSpace.transform);
        trialTargetScript.ColorObj(targetGreen);

        // Controller visibility
        yield return new WaitUntil(() =>(posControllerInTrialSpace.z > .1));
        StartCoroutine(ControllerVisibility(trial));

        // Ensure input button is not fat fingered immediately before input is allowed
        yield return new WaitUntil(() => (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) > 0.1f));
        yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
        controllerSphereScript.visible = false;

        trueInput = GetRelativePosition(trialSpace, rightHandAnchor);
        visualOffsetInput = GetRelativePosition(trialSpace, controllerSphere);

        trialProgress = "trialInput";
        trialInputTime = Time.time;

        trialStartScript.visible = true;
        trialTargetScript.PositionAnchor(targetPos);
        trialTargetScript.ColorObj(standbyGrey);
        trialStartScript.ColorObj(startTeal);
        controllerSphereScript.ResetVisualOffset();

        SaveResults(trial);
        yield return new WaitUntil(() => (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) < 0.1f));


        trial.End();
    }
    IEnumerator TrialMultiTargetCoroutine(Trial trial)
    {
        GameObject currentTarget
        Debug.Log("target local: " + trialTarget.transform.localPosition);

        trialProgress = "trialStarted";
        trialStartedTime = Time.time;

        trialStartScript.visible = false;
        if (!(trial.settings.GetBool("controllerVisibleTrialStart")))
        {
            controllerSphereScript.visible = false;
        }
        yield return null;
        controllerSphereScript.visualOffsetFromReference(new Vector3(
            trial.settings.GetFloat("visualXOffset"), 
            0, 
            0),
            trialSpace.transform);
        trialTargetScript.ColorObj(targetGreen);

        trial.End();
    }
    // Update is called once per frame
    void Update()
    {
        posControllerInTrialSpace = trialSpace.transform.InverseTransformPoint(rightHandAnchor.transform.position);
    }
    IEnumerator calibration2d()
    {
        trackedDeskScript.StartCalibrateDesk();
        yield return new WaitUntil(() => (trackedDeskScript.DeskPositioned));
        GameObject a_instructionsArrow = generateInstructions.InstantiateArrowText(
            buttonA,
            "Rest controller on table and\npress A to calibrate right controller",
            true
        );
        yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
        trialSpaceSetup.transform.position = new Vector3(
            trialSpaceSetup.transform.position.x,
            rightControllerAnchor.transform.position.y,
            trialSpaceSetup.transform.position.z);
        Destroy(a_instructionsArrow);
        calibrationComplete = true;
        rightControllerAnchor.SetActive(false);
    }
    IEnumerator ControllerVisibility(Trial trial)
    {
        trialProgress = "trialControlVisibilityOff";
        trialControlVisibilityOffTime = Time.time;

        if (trial.settings.GetBool("turnControllerVisibleMidpoint"))
        {
            controllerSphereScript.visible = true;
        }

        yield return new WaitForSeconds(trial.settings.GetFloat("controllerMidpointVisibleTime"));
        trialProgress = "trialControlVisibilityOn";
        trialControlVisibilityOnTime = Time.time;

        if (trial.settings.GetBool("turnControllerVisibleMidpoint"))
        {
            controllerSphereScript.visible = false;
        }
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
        Session.instance.CurrentTrial.result["trialSetupTime"] = trialSetupTime;
        Session.instance.CurrentTrial.result["trialStartedTime"] = trialStartedTime;
        Session.instance.CurrentTrial.result["trialControlVisibilityOffTime"] = trialControlVisibilityOffTime;
        Session.instance.CurrentTrial.result["trialControlVisibilityOnTime"] = trialControlVisibilityOnTime;
        Session.instance.CurrentTrial.result["trialInputTime"] = trialInputTime;
    }
    private Vector3 GetRelativePosition(GameObject reference, GameObject target)
    {
        return reference.transform.InverseTransformPoint(target.transform.position);
    }
}
