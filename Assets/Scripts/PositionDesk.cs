using UnityEngine;
using System.Collections;


public class PositionDesk : MonoBehaviour
{
    public GameObject desk;
    public GameObject leftHandDeskAnchor;
    public bool DeskPositioned = false;
    private float Quest3DeskAdjustment = 9.5f; //rotation adjustment after moving from Quest 2
    public GameObject button_x;
    public GameObject button_y;
    public GenerateInstructions generateInstructions;
    private GameObject x_instrcutionArrow;
    private Coroutine CalibrateDeskCR;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void StartCalibrateDesk()
    {
        CalibrateDeskCR = StartCoroutine(CalibrateDesk());
    }

    IEnumerator CalibrateDesk()
    {
        x_instrcutionArrow = generateInstructions.InstantiateArrowText(
            button_x,
            "Press X to anchor table",
            true
        );
        x_instrcutionArrow.SetActive(true);
        while (true)
        {
            if (DeskPositioned == false)
            {

                desk.transform.position = leftHandDeskAnchor.transform.position;
                desk.transform.eulerAngles = new Vector3(0, leftHandDeskAnchor.transform.eulerAngles.y + Quest3DeskAdjustment, 0);
                if (OVRInput.GetDown(OVRInput.Button.Three))
                {
                    DeskPositioned = true;
                    x_instrcutionArrow.SetActive(false);
                }
            }
            if (OVRInput.GetDown(OVRInput.Button.Four))
            {
                DeskPositioned = false;
                x_instrcutionArrow.SetActive(true);
            }
            yield return null;
        }
    }
}
