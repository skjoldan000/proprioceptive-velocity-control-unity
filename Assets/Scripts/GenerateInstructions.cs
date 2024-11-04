using UnityEngine;
using TMPro;


public class GenerateInstructions : MonoBehaviour
{
    public GameObject instructionArrowText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject InstantiateArrowText(GameObject targetLoc, string text = "", bool arrow = false)
    {
        Transform targetLocPos = targetLoc.transform;
        GameObject instantiatedObject = Instantiate(instructionArrowText);

        instantiatedObject.transform.SetParent(targetLocPos, false);

        instantiatedObject.transform.localPosition = Vector3.zero;

        TextMeshPro tmpComponent = instantiatedObject.GetComponentInChildren<TextMeshPro>();
        tmpComponent.text = text;

        Transform arrowTransform = instantiatedObject.transform.Find("arrow");

        arrowTransform.gameObject.SetActive(arrow);
        return instantiatedObject;
    }
}
