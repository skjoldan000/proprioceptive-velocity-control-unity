using UnityEngine;

public class ControllerSphereRotatedFrame : MonoBehaviour
{
    public GameObject trialSpace;
    // public GameObject trialSpaceRotatedMovementSpace;
    public GameObject rightHandAnchor;
    public GameObject targetAnchor;

    void Update()
    {
        transform.localPosition = rightHandAnchor.transform.position;
        transform.localPosition = GetRelativePosition(trialSpace, rightHandAnchor);
    }
    private Vector3 GetRelativePosition(GameObject reference, GameObject target)
    {
        return reference.transform.InverseTransformPoint(target.transform.position);
    }
}
