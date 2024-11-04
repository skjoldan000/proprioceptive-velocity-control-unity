using UnityEngine;

public class FaceCameraY : MonoBehaviour
{
    private Transform centerEyeAnchor;

    void Start()
    {
        // Find the CenterEyeAnchor from your VR camera rig
        // Assuming you're using OVRPlayerController or a similar setup
        centerEyeAnchor = GameObject.Find("CenterEyeAnchor").transform;
    }

    void Update()
    {
        if (centerEyeAnchor == null)
        {
            Debug.LogError("CenterEyeAnchor not found");
            return;
        }

        // Look at the camera along the y-axis only.
        Vector3 targetPosition = new Vector3(centerEyeAnchor.position.x, transform.position.y, centerEyeAnchor.position.z);
        transform.LookAt(targetPosition);
    }
}
