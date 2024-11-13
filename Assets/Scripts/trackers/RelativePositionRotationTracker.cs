using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UXF
{
    public class RelativePositionRotationTracker : Tracker
    {
        public override string MeasurementDescriptor => "movement";
        public override IEnumerable<string> CustomHeader => new string[] {"trialID", "trialProgress", "truepos_x", "truepos_y", "truepos_z", "vispos_x", "vispos_y", "vispos_z", "controllerSphereVisible", "aButtonDown", "targetNumber"};
        public TaskRunner taskRunner;
        public GameObject trialSpace;
        public GameObject controllerSphere;
        private ControlSphere controllerSphereScript;
        public GameObject rightHandAnchor;

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
                ("truepos_x", truePosition.x),
                ("truepos_y", truePosition.y),
                ("truepos_z", truePosition.z),
                ("vispos_x", visualPosition.x),
                ("vispos_y", visualPosition.y),
                ("vispos_z", visualPosition.z),
                ("controllerSphereVisible", controllerSphereScript.visible),
                ("aButtonDown", OVRInput.GetDown(OVRInput.Button.One)),
                ("targetNumber", taskRunner.targetNumber)
            };


            return values;
        }
    private Vector3 GetRelativePosition(GameObject reference, GameObject target)
    {
        return reference.transform.InverseTransformPoint(target.transform.position);
    }
    }
}