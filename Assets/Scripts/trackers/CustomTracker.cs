using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UXF
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record position/rotation of the object at each frame.
    /// </summary>
    public class CustomTracker : Tracker
    {
        public override string MeasurementDescriptor => "movement";
        public override IEnumerable<string> CustomHeader => new string[] {"trialStatus", "vibrationOn"};
        public TaskRunner taskRunner;

        /// <summary>
        /// Returns current position and rotation values
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            // get position and rotation
            Vector3 p = gameObject.transform.position;
            Vector3 r = gameObject.transform.eulerAngles;

            // return position, rotation (x, y, z) as an array
            var values = new UXFDataRow()
            {
                 //("trialStatus", taskRunner.trialStatus),
                 //("vibrationOn", taskRunner.vibrationOn),

            };

            return values;
        }
    }
}