using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ControlrotatingArm : MonoBehaviour
{
    public GameObject handAnchor;
    public GameObject controllerAnchor;
    public GameObject a_button;
    public GameObject gripModel;
    public GameObject rotatingArm;
    public GameObject rotatingArmObj;
    public float angleMultiplier = 1f;
    public GameObject positionToOffsetFrom;
    public GameObject positionProjected;
    public GameObject positionOffsetProjected;
    public GameObject offsetControllerAnchor;
    public bool rotatingArmVisible = true;
    public bool calibrationComplete = false;
    public GenerateInstructions generateInstructions;
    [SerializeField] private float angleToCalibrateWith = 75f;
    public Coroutine calibrationCoroutine;
    private float radius;
    private float radiusSD;
    public float angleStartToController;
    public float angleStartToOffsetController;
    public GameObject trackedDesk;
    public bool savePointsForCalibrationComplete = false;
    private List<Vector3> pointsForCalibration;



    void Start()
    {
        StartCalibration();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCalibration();
            Debug.LogWarning("Calibration restarted");
        }
        if (calibrationComplete)
        {
            controllerAnchor.SetActive(false);
            offsetControllerAnchor.SetActive(true);

            angleStartToController = CalculateAngleDirectional(positionToOffsetFrom, handAnchor, rotatingArm);
            angleStartToOffsetController = angleStartToController * angleMultiplier;
            Vector3 dirToController = handAnchor.transform.position - rotatingArm.transform.position;
            Vector3 dirToStart = positionToOffsetFrom.transform.position - rotatingArm.transform.position;
            Vector3 dirToOffsetController = Quaternion.Euler(0, angleStartToOffsetController, 0) * dirToStart;

            rotatingArm.transform.forward = dirToOffsetController;

        }
        else
        {
            controllerAnchor.SetActive(true);
            offsetControllerAnchor.SetActive(false);
        }
    }

    IEnumerator calibrateRotatingArmLocation()
    {
        rotatingArmObj.SetActive(false);
        Debug.LogWarning("Calibration started");
        calibrationComplete = false;
        GameObject instructionsArrow1 = generateInstructions.InstantiateArrowText(
            a_button,
            "Press A to start calibration",
            true
        );
        yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
        Debug.LogWarning("Calibration start key pressed");
        Destroy(instructionsArrow1);
        positionToOffsetFrom.transform.position = handAnchor.transform.position;
        positionToOffsetFrom.transform.rotation = handAnchor.transform.rotation;

        StartCoroutine(SavePointsForCalibration());
        yield return new WaitUntil(() => (savePointsForCalibrationComplete == true));
        Debug.LogWarning("Calibration save points for calibration complete");
        CalibrateCircleCenter(pointsForCalibration);
        

        calibrationComplete = true;
        rotatingArmObj.SetActive(true);
        Debug.LogWarning("Calibration completed");
        Debug.LogWarning("radius " + radius);
        Debug.LogWarning("radius SD " + radiusSD);
    }
    IEnumerator SavePointsForCalibration()
    {
        savePointsForCalibrationComplete = false;
        float lastAngleDiff = 0;


        pointsForCalibration = new List<Vector3>();
        Quaternion initialRotation = handAnchor.transform.rotation;

        pointsForCalibration.Add(handAnchor.transform.position);

        GameObject instructionsArrow = generateInstructions.InstantiateArrowText(
            a_button,
            "Move " + angleToCalibrateWith + " to the right",
            true
        );
        TextMeshPro instructionsArrowText = instructionsArrow.GetComponentInChildren<TextMeshPro>();

        while (Quaternion.Angle(handAnchor.transform.rotation, initialRotation) < angleToCalibrateWith)
        {
            float angleDiff = Quaternion.Angle(handAnchor.transform.rotation, initialRotation);
            instructionsArrowText.text = "Move " + Mathf.Round((float)(angleToCalibrateWith - Quaternion.Angle(handAnchor.transform.rotation, initialRotation))) + " to the right";

            if (angleDiff - lastAngleDiff > 0.5f)
            {
                pointsForCalibration.Add(handAnchor.transform.position);
                lastAngleDiff = angleDiff;
            }
            yield return null;
        }
        Destroy(instructionsArrow);
        savePointsForCalibrationComplete = true;
    }

    public void StartCalibration()
    {
        calibrationCoroutine = StartCoroutine(calibrateRotatingArmLocation());
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
        aveCenter = new Vector3(aveCenter.x, handAnchor.transform.position.y, aveCenter.z);
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

        // Set center of rotating armrest
        rotatingArm.transform.position = aveCenter;
        rotatingArm.transform.eulerAngles = new Vector3(0, 0, 0);
        // Set grip to correct distance
        gripModel.transform.localPosition = new Vector3(0, 0, -(radius+0.003f));
        positionProjected.transform.localPosition = new Vector3(0, 0, radius);
        positionOffsetProjected.transform.localPosition = new Vector3(0, 0, radius);


        positionToOffsetFrom.transform.position = new Vector3(positionToOffsetFrom.transform.position.x, rotatingArm.transform.position.y, positionToOffsetFrom.transform.position.z);
        angleStartToController = CalculateAngleDirectional(positionToOffsetFrom, handAnchor, rotatingArm);
        angleStartToOffsetController = angleStartToController * angleMultiplier;
        Vector3 dirToController = handAnchor.transform.position - rotatingArm.transform.position;
        Vector3 dirToStart = positionToOffsetFrom.transform.position - rotatingArm.transform.position;
        Vector3 dirToOffsetController = Quaternion.Euler(0, angleStartToOffsetController, 0) * dirToStart;
        rotatingArm.transform.forward = dirToOffsetController;

        offsetControllerAnchor.transform.rotation = handAnchor.transform.rotation;

    }

    private float CalculateAngleDirectional(GameObject At, GameObject Bt, GameObject Ct)
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
}