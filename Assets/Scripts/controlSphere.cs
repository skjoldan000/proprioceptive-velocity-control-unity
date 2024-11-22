using UnityEngine;

public class ControlSphere : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject obj;
    public GameObject rightHandAnchor;
    public bool visible = true;
    void Awake()
    {
        obj = transform.Find("Sphere")?.gameObject;
        if (obj == null)
        {
            Debug.LogError("Sphere child GameObject is missing.");
        }
    }

    public void PositionAnchor(Vector3 requestedPosition)
    {
        transform.localPosition = requestedPosition;
    }
    public void ColorObj(Material requestedColor)
    {
        obj.GetComponent<Renderer>().material = requestedColor;
    }
    public void Visible(bool targetVisibility)
    {
        visible = targetVisibility;
        obj.SetActive(visible);
    }
    public void visualOffsetFromReference(Vector3 requestedOffset, Transform referenceTransform)
    {
        transform.position = transform.position
         + referenceTransform.right.normalized * requestedOffset.x
         + referenceTransform.up.normalized * requestedOffset.y
         + referenceTransform.forward.normalized * requestedOffset.z;
    }
    public void ResetVisualOffset()
    {
        transform.position = rightHandAnchor.transform.position;
    }
}