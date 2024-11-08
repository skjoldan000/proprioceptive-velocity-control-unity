using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UXF;

using TMPro;

public class TaskRunner : MonoBehaviour
{
    private Config c;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Coroutine currentTrial;
    public GameObject trialSpace;
    private ControlSphere trialSpaceScript;
    public GameObject trialTarget;
    private ControlSphere trialTargetScript;
    public GameObject trialStart;
    private ControlSphere trialStartScript;
    public GameObject controllerSphere;
    private ControlSphere controllerSphereScript;

    // Materials
    public Material targetGreen;
    public Material startTeal;
    public Material activeBlue;
    public Material readyYellow;
    public Material standbyGrey;
    public Material controllerPurple;

    void Start()
    {
        c = GetComponent<Config>();
        trialSpaceScript = trialSpace.GetComponent<ControlSphere>();
        trialTargetScript = trialTarget.GetComponent<ControlSphere>();
        trialStartScript = trialStart.GetComponent<ControlSphere>();
        controllerSphereScript = controllerSphere.GetComponent<ControlSphere>();
    }

    public void StartTrial(Trial trial)
    {
        //currentTrial = StartCoroutine(TrialCoroutine(trial));
        StartCoroutine(TrialCoroutine(trial));
    }

    IEnumerator TrialCoroutine(Trial trial)
    {
        Debug.LogWarning("Trial " + trial.numberInBlock + " started.");
        Vector3 targetPos = new Vector3(0f, 0f, 0.5f);
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
        trialStartScript.VisibleObj(false);
        trialTargetScript.ColorObj(targetGreen);

        // Ensure input button is not fat fingered immediately
        yield return new WaitUntil(() => (Vector3.Distance(trialStart.transform.position, controllerSphere.transform.position) > 0.1f));
        yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
        trial.End();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
