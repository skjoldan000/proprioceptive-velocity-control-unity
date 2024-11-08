using UnityEngine;
using System.Collections.Generic;
using UXF;


public class ExperimentConstructorScript : MonoBehaviour
{
    private int blockNumber;
    public void Generate(Session uxfSession)
    {
        List<int> runBlocks = uxfSession.settings.GetIntList("runBlocks");
        int numRepeats = uxfSession.settings.GetInt("numRepeats");
        int blockDims = uxfSession.settings.GetInt("blockDims");

        blockNumber = 1;
        if (runBlocks.Contains(blockNumber))
        {
            Block block1 = uxfSession.CreateBlock();
            for (int i = 0; i < numRepeats; i++)
            {
                Trial newTrial = block1.CreateTrial();
            }
        }
    }
}
