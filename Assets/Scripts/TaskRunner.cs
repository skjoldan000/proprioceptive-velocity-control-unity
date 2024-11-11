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

    // Materials
    public Material targetGreen;
    public Material startTeal;
    public Material activeBlue;
    public Material readyYellow;
    public Material standbyGrey;
    public Material controllerPurple;

    // UXF settings
    private Coroutine currentTrial;
    private bool calibration;
    private int nDims;

    // Trial progress
    private bool calibrationComplete = false;
    private string trialProgress;

    // Timers
    private float trialSetupTime;
    private float trialStartedTime;
    private float trialInputTime;
    private float trialControlVisibilityOffTime;
    private float trialControlVisibilityOnTime;

    // Others
    private Vector3 posControllerInTrialSpace;
    
    

    void Start()
    {
        c = GetComponent<Config>();
        trialSpaceScript = trialSpace.GetComponent<ControlSphere>();
        trialTargetScript = trialTarget.GetComponent<ControlSphere>();
        trialStartScript = trialStart.GetComponent<ControlSphere>();
        controllerSphereScript = controllerSphere.GetComponent<ControlSphere>();
        trackedDeskScript = trackedDesk.GetComponent<PositionDesk>();
    }

    public void StartTrial(Trial trial)
    {
        //currentTrial = StartCoroutine(TrialCoroutine(trial));
        StartCoroutine(TrialCoroutine(trial));
    }

    IEnumerator TrialCoroutine(Trial trial)
    {
        // load trial values
        GetTrialSettings(trial);

        // calibration
        if (calibration == true)
        {
            trialProgress = "calibrationStarted";
            Debug.Log("Calibration started");
            if (nDims == 2)
            {
                StartCoroutine(calibration2d());
                yield return new WaitUntil(() => (calibrationComplete));
            }
            Debug.Log("Calibration completed");
        }
        trialProgress = "trialSetup";
        trialSetupTime = Time.time;


        Debug.Log("Trial " + trial.numberInBlock + " started.");
        Vector3 targetPos = new Vector3(0f, 0f, 0.35f);
        trialStartScript.VisibleObj(true);
        trialTargetScript.PositionAnchor(targetPos);
        trialTargetScript.ColorObj(standbyGrey);
        trialStartScript.ColorObj(startTeal);

        float timer = 0f;
        bool readyButtonPressed = false;
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
                    readyButtonPressed = true;
                    trialStartScript.ColorObj(readyYellow);
                    trialTargetScript.ColorObj(readyYellow);
                    timer += 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }
                yield return null;
            }
            else
            {
                timer = 0f;
                readyButtonPressed = false;
                trialStartScript.ColorObj(startTeal);
                trialTargetScript.ColorObj(standbyGrey);
                yield return null;
            }
        }

        trialProgress = "trialStarted";
        trialStartedTime = Time.time;

        if (!(trial.settings.GetBool("controllerVisibleTrialStart")))
        {
            controllerSphereScript.VisibleObj(false);
        }

        controllerSphereScript.visualOffsetFromReference(new Vector3(
            trial.settings.GetValue("visualXOffset"), 
            0, 
            0));

        trialStartScript.VisibleObj(false);
        trialTargetScript.ColorObj(targetGreen);

        // Controller visibility
        yield return new WaitUntil(() =>(posControllerInTrialSpace.z > .15));
        StartCoroutine(ControllerVisibility(trial));

        // Ensure input button is not fat fingered immediately before input is allowed
        yield return new WaitUntil(() => (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) > 0.1f));
        yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));

        trialProgress = "trialInput";
        trialInputTime = Time.time;

        //

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
        trialSpace.transform.position = new Vector3(
            trialSpace.transform.position.x,
            rightControllerAnchor.transform.position.y,
            trialSpace.transform.position.z);
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
            controllerSphereScript.VisibleObj(true);
        }

        yield return new WaitForSeconds(trial.settings.GetFloat("controllerMidpointVisibleTime"));
        trialProgress = "trialControlVisibilityOn";
        trialControlVisibilityOnTime = Time.time;

        if (trial.settings.GetBool("turnControllerVisibleMidpoint"))
        {
            controllerSphereScript.VisibleObj(false);
        }
    }

    private void GetTrialSettings(Trial trial)
    {
        calibration = trial.settings.GetBool("calibration");
        nDims = trial.settings.GetInt("nDims");
    }
}
