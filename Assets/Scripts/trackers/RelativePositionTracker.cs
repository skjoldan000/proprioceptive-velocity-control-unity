using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UXF
{
    public class RelativePositionTracker : Tracker
    {
        public override string MeasurementDescriptor => "movement";
        public override IEnumerable<string> CustomHeader => new string[] {
            "trialID", "trialProgress", "truepos.x", "truepos.y", "truepos.z", "vispos.x", "vispos.y", "vispos.z", 
            "controllerSphereVisible", "aButtonDown", "targetNumber", "vibrationTriggered", 
            "vibBoth", "vibLeft", "vibRight", "arduinoListenerStopwatch", "frameStartToAudioStartPre", 
            "frameStartToAudioStopPre"};
        public TaskRunner taskRunner;
        public GameObject trialSpace;
        public GameObject controllerSphere;
        private ControlSphere controllerSphereScript;
        public GameObject rightHandAnchor;
        public AudioSource vibBoth;
        public AudioSource vibLeft;
        public AudioSource vibRight;
        public ArduinoReciever arduinoReciever;
        public AudioLatencyTester audioLatencyTester;

        void Start()
        {
            controllerSphereScript = controllerSphere.GetComponent<ControlSphere>();
        }

        protected override UXFDataRow GetCurrentValues()
        {
            // get position and rotation
            Vector3 visualPosition = GetRelativePosition(trialSpace, controllerSphere);
            Vector3 truePosition = GetRelativePosition(trialSpace, rightHandAnchor);

            // return position, rotation (x, y, z) as an array
            var values = new UXFDataRow()
            {
                ("trialID", taskRunner.trialID),
                ("trialProgress", taskRunner.trialProgress),
                ("truepos.x", truePosition.x),
                ("truepos.y", truePosition.y),
                ("truepos.z", truePosition.z),
                ("vispos.x", visualPosition.x),
                ("vispos.y", visualPosition.y),
                ("vispos.z", visualPosition.z),
                ("controllerSphereVisible", controllerSphereScript.visible),
                ("aButtonDown", OVRInput.GetDown(OVRInput.Button.One)),
                ("targetNumber", taskRunner.targetNumber),
                ("vibrationTriggered", taskRunner.vibrationTriggered),
                ("vibBoth", vibBoth.isPlaying),
                ("vibLeft", vibLeft.isPlaying),
                ("vibRight", vibRight.isPlaying),
                ("arduinoListenerStopwatch", arduinoReciever.alignedStopwatch),
                ("frameStartToAudioStartPre", audioLatencyTester.frameStartToAudioStartPre),
                ("frameStartToAudioStopPre", audioLatencyTester.frameStartToAudioStopPre)
            };


            return values;
        }
    private Vector3 GetRelativePosition(GameObject reference, GameObject target)
    {
        return reference.transform.InverseTransformPoint(target.transform.position);
    }
    }
}