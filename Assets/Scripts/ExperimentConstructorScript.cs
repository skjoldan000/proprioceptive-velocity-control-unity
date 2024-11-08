using UnityEngine;
using System.Collections.Generic;
using UXF;


public class ExperimentConstructorScript : MonoBehaviour
{
    private int blockNumber;
    public void Generate(Session uxfSession)
    {
        Debug.LogWarning("Experiment constructor started");
        List<int> runBlocks = uxfSession.settings.GetIntList("runBlocks");
        int numRepeats = uxfSession.settings.GetInt("numRepeats");
        bool oneDim = uxfSession.settings.GetBool("oneDim");

        blockNumber = 1;
        if (runBlocks.Contains(blockNumber))
        {
            Block block1 = uxfSession.CreateBlock();
            for (int i = 0; i < numRepeats; i++)
            {
                Trial newTrial = block1.CreateTrial();
                Debug.LogWarning("Trial constructed " + i);
            }
        }
    }
}
