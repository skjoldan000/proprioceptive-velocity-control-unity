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

        blockNumber = 0;
        if (blockNumber == 0)
        {
            Block block = uxfSession.CreateBlock();
            Trial newTrial = block.CreateTrial();
            newTrial.settings.SetValue("blockNumber", blockNumber);
            newTrial.settings.SetValue("calibration", true);
            block.settings.SetValue("blockInstructions", "Calibration");
        }

        if (uxfSession.settings.GetInt("nDims") == 1)
        {
            blockNumber = 1;
            if (runBlocks.Contains(blockNumber))
            {
                Block block = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeats; i++)
                {
                    Trial newTrial = block.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", true);
                    newTrial.settings.SetValue("targetDegrees", 80f);
                }
                block.settings.SetValue("blockInstructions", (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, to move to target and press A."
                ));
                block.trials.Shuffle();
                Debug.Log("Block 1 Instructions: " + block.settings.GetString("blockInstructions"));
            }
        }


        else if (uxfSession.settings.GetInt("nDims") == 2)
        {
            blockNumber = 1;
            if (runBlocks.Contains(blockNumber))
            {
                Block block = uxfSession.CreateBlock();
                block.settings.SetValue("blockInstructions", "testing text");
                for (int i = 0; i < numRepeats; i++)
                {
                    Trial newTrial = block.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", true);
                }
                block.settings.SetValue("blockInstructions", (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, to move to target and press A."
                ));
                block.trials.Shuffle();
                Debug.Log("Block 1 Instructions: " + block.settings.GetString("blockInstructions"));
            }

            blockNumber = 2;
            if (runBlocks.Contains(blockNumber))
            {
                Block block = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeats; i++)
                {
                    Trial newTrial = block.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                }
                block.settings.SetValue("blockInstructions", (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, to move to target and press A.\n"+
                    "Your controller sphere will be invisible during this task."
                ));
                block.trials.Shuffle();
            }

            blockNumber = 3;
            if (runBlocks.Contains(blockNumber))
            {
                Block block = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeats; i++)
                {
                    foreach (float visualXOffset in uxfSession.settings.GetFloatList("visualXOffsets"))
                    {
                        foreach (float visualZOffset in uxfSession.settings.GetFloatList("visualZOffsets"))
                        {
                            Trial newTrial = block.CreateTrial();
                            newTrial.settings.SetValue("blockNumber", blockNumber);
                            newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                            newTrial.settings.SetValue("turnControllerVisibleMidpoint", true);
                            newTrial.settings.SetValue("visualXOffset", visualXOffset);
                            newTrial.settings.SetValue("visualZOffset", visualZOffset);
                        }
                    }
                }
                block.settings.SetValue("blockInstructions", (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, then move to target and press A."+
                    "\nYour controller sphere will be mostly invisible during this task, "+
                    "but you will be briefly shown your controller location during movement."
                ));
                block.trials.Shuffle();
            }

            blockNumber = 4;
            if (runBlocks.Contains(blockNumber))
            {
                Block block = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeats; i++)
                {
                    foreach (float visualXOffset in uxfSession.settings.GetFloatList("visualXOffsets"))
                    {
                        Trial newTrial = block.CreateTrial();
                        newTrial.settings.SetValue("blockNumber", blockNumber);
                        newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                        newTrial.settings.SetValue("nTargets", uxfSession.settings.GetInt("nTargetsb4"));
                        newTrial.settings.SetValue("visualXOffset", visualXOffset);
                        newTrial.settings.SetValue("turnControllerVisibleMidpoint", true);
                    }
                    foreach (float visualZOffset in uxfSession.settings.GetFloatList("visualZOffsets"))
                    {
                        Trial newTrial = block.CreateTrial();
                        newTrial.settings.SetValue("blockNumber", blockNumber);
                        newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                        newTrial.settings.SetValue("nTargets", uxfSession.settings.GetInt("nTargetsb4"));
                        newTrial.settings.SetValue("visualZOffset", visualZOffset);
                        newTrial.settings.SetValue("turnControllerVisibleMidpoint", true);
                    }
                }
                block.settings.SetValue("blockInstructions", (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, then move to target and press A."+
                    "\nYour controller sphere will be mostly invisible during this task, "+
                    "but you will be briefly shown your controller location during movement."
                ));
                block.trials.Shuffle();
            }
        }
        else 
        {
            Debug.LogError("nDims must be 1 or 2");
        }

    }
}
