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
        int nDims = uxfSession.settings.GetInt("nDims");
        List<float> visualXOffsets = uxfSession.settings.GetFloatList("visualXOffsets");

        // calibration 1d
        if (nDims == 1)
        {
        }

        // calibration 2d
        if (nDims == 2)
        {
            Block calibrationBlock = uxfSession.CreateBlock();
            Trial newTrial = calibrationBlock.CreateTrial();
            newTrial.settings.SetValue("blockNumber", 0);
            newTrial.settings.SetValue("calibration", true);
        }

        blockNumber = 1;
        if (runBlocks.Contains(blockNumber))
        {
            Block block1 = uxfSession.CreateBlock();
            for (int i = 0; i < numRepeats; i++)
            {
                Trial newTrial = block1.CreateTrial();
                newTrial.settings.SetValue("blockNumber", blockNumber);
            }
            block1.trials.Shuffle();
        }
        blockNumber = 2;
        if (runBlocks.Contains(blockNumber))
        {
            Block block2 = uxfSession.CreateBlock();
            for (int i = 0; i < numRepeats; i++)
            {
                Trial newTrial = block2.CreateTrial();
                newTrial.settings.SetValue("blockNumber", blockNumber);
                newTrial.settings.SetValue("controllerVisibleTrialStart", false);
            }
            block2.trials.Shuffle();
        }
        blockNumber = 3;
        if (runBlocks.Contains(blockNumber))
        {
            Block block3 = uxfSession.CreateBlock();
            for (int i = 0; i < numRepeats; i++)
            {
                foreach (float visualXOffset in visualXOffsets)
                {
                    Trial newTrial = block3.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                    newTrial.settings.SetValue("turnControllerVisibleMidpoint", true);
                    newTrial.settings.SetValue("visualXOffset", visualXOffset);
                }
            }
            block3.trials.Shuffle();
        }
    }
}
