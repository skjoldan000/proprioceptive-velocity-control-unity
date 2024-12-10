using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UXF
{
    public class RelativeRotationTracker : Tracker
    {
        public override string MeasurementDescriptor => "movement";
        public override IEnumerable<string> CustomHeader => new string[] {
            "trialID", "trialProgress", "angleStartToController", "angleStartToOffsetController", "angleStartToPacer",
            "controllerSphereVisible", "aButtonDown", "targetNumber", "vibrationTriggered", 
            "vibBoth", "vibLeft", "vibRight", "arduinoListenerStopwatch"};
        public TaskRunner taskRunner;
        public GameObject controllerSphere;
        private ControlSphere controllerSphereScript;
        public AudioSource vibBoth;
        public AudioSource vibLeft;
        public AudioSource vibRight;
        public ArduinoReciever arduinoReciever;

        void Start()
        {
            controllerSphereScript = controllerSphere.GetComponent<ControlSphere>();
        }

        protected override UXFDataRow GetCurrentValues()
        {
            var values = new UXFDataRow()
            {
                ("trialID", taskRunner.trialID),
                ("trialProgress", taskRunner.trialProgress),
                ("angleStartToController", taskRunner.angleStartToController),
                ("angleStartToOffsetController", taskRunner.angleStartToOffsetController),
                ("angleStartToPacer", taskRunner.angleStartToPacer),
                ("controllerSphereVisible", controllerSphereScript.visible),
                ("aButtonDown", OVRInput.GetDown(OVRInput.Button.One)),
                ("targetNumber", taskRunner.targetNumber),
                ("vibrationTriggered", taskRunner.vibrationTriggered),
                ("vibBoth", vibBoth.isPlaying),
                ("vibLeft", vibLeft.isPlaying),
                ("vibRight", vibRight.isPlaying),
                ("arduinoListenerStopwatch", arduinoReciever.alignedStopwatch)
            };


            return values;
        }
    private Vector3 GetRelativePosition(GameObject reference, GameObject target)
    {
        return reference.transform.InverseTransformPoint(target.transform.position);
    }
    }
}