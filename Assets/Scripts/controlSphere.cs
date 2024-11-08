using UnityEngine;

public class controlSphere : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject obj;
    void Start()
    {
        obj = transform.Find("obj").gameObject;
    }

    public void PositionAnchor(Transform requestedPosition)
    {
        transform.position = requestedPosition.position;
    }
    public void ColorObj(Material requestedColor)
    {
        obj.GetComponent<Renderer>().material = requestedColor;
    }
    public void VisibleObj(bool targetVisibility)
    {
        obj.SetActive(targetVisibility);
    }
    public void visualOffsetFromReference(Vector3 requestedOffset, Transform referenceTransform)
    {
        transform.position = transform.position
         + referenceTransform.right * requestedOffset.x
         + referenceTransform.up * requestedOffset.y
         + referenceTransform.forward * requestedOffset.z;
    }
}