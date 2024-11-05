using UnityEngine;
using System.Collections;

public class ControlRotatingArmrest : MonoBehaviour
{
    public GameObject handAnchor;
    public GameObject controllerAnchor;
    public GameObject gripModel;
    public float angleMultiplier = 1f;
    public bool rotatingArmVisible = true;
    public bool calibrationComplete = false;
    public GenerateInstructions generateInstructions;
    public Transform startTransform;
    public Transform endTransform;
    public Transform circleCenter;
    [SerializeField] private float angleToCalibrateWith = 75f;
    public Coroutine calibrationCoroutine;
    private float radius;
    public float angleStartToController;
    public float angleStartToOffsetController;
    public GameObject leftController;



    void Start()
    {
        StartCalibration();
    }
    void Update()
    {
        if (calibrationComplete)
        {
            float angleStartToController = CalculateAngleDirectional(startTransform, handAnchor, circleCenter);
            
            if (rotatingArmVisible)
            {

            }
        }
    }
    IEnumerator calibrateRotatingArmLocation()
    {
        calibrationComplete = false;
        yield return new WaitUntil(() => (OVRInput.GetDown(OVRInput.Button.One)));
        startTransform = handAnchor.transform;
        while (Mathf.Abs(handAnchor.transform.eulerAngles.y - startTransform.eulerAngles.y) < angleToCalibrateWith)
        {
            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                startTransform = handAnchor.transform;
            }
            yield return null;
        }
        endTransform = handAnchor.transform;
        CalibrateCircleCenter(startTransform, endTransform);
        calibrationComplete = true;
    }

    public void StartCalibration()
    {
        calibrationCoroutine = StartCoroutine(calibrateRotatingArmLocation());
    }

    private void CalibrateCircleCenter(Transform A, Transform B)
    {
        float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(A.eulerAngles.y, B.eulerAngles.y));
        float ABdist = Vector3.Distance(A.position, B.position);
        float isoscelesAngle = (180 - deltaAngle);
        radius = ABdist * (Mathf.Sin(isoscelesAngle*Mathf.Deg2Rad)/Mathf.Sin(deltaAngle*Mathf.Deg2Rad));
        Vector3 ABmidpoint = (A.position + B.position)/2;
        Vector3 BtoMidpoint = (B.position - ABmidpoint).normalized;
        Vector3 directionMidpointToCenter = Quaternion.Euler(0, 90, 0) * BtoMidpoint;
        circleCenter.position = ABmidpoint + directionMidpointToCenter * Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(ABdist, 2));
        handAnchor.transform.position = circleCenter.position;
        gripModel.transform.localPosition = new Vector3(0, 0, -(radius+0.003f));
    }
    private float CalculateAngleDirectional(Transform At, Transform Bt, Transform Ct)
    {
        Vector3 A = At.position;
        Vector3 B = Bt.position;
        Vector3 C = Ct.position;

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