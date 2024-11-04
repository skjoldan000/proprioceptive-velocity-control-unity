using UnityEngine;

public class PositionDesk : MonoBehaviour
{
    public GameObject desk;
    public GameObject anchor;
    public bool DeskPositioned;
    private float Quest3DeskAdjustment = 9.5f; //rotation adjustment after moving from Quest 2
    public GameObject button_x;
    public GameObject button_y;
    public GenerateInstructions generateInstructions;
    private GameObject x_instrcutionArrow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        x_instrcutionArrow = generateInstructions.InstantiateArrowText(
            button_x,
            "Press X to anchor table",
            true
        );
        x_instrcutionArrow.SetActive(true);
        Debug.Log("Arrow instanciated");
    }

    // Update is called once per frame
    void Update()
    {
        if (DeskPositioned == false)
        {

            desk.transform.position = anchor.transform.position;
            desk.transform.eulerAngles = new Vector3(0, anchor.transform.eulerAngles.y + Quest3DeskAdjustment, 0);
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
    }
}
