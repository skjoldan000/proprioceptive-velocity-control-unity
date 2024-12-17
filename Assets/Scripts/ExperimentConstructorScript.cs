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
        int numRepeatsPractice = uxfSession.settings.GetInt("numRepeatsPractice");
        string practiceInstructions = $"\nYou will start with {numRepeatsPractice} practice trials before moving on the data collection.";
        string realInstructions = $"\nActual data collection will now start.";

        blockNumber = 0;
        if (blockNumber == 0)
        {
            Block block = uxfSession.CreateBlock();
            Trial newTrial = block.CreateTrial();
            newTrial.settings.SetValue("blockNumber", blockNumber);
            newTrial.settings.SetValue("calibration", true);
            block.settings.SetValue("blockInstructions", "Calibration");
        }
        // 1D reaching tasks, rotating arm rest tasks
        if (uxfSession.settings.GetInt("nDims") == 1)
        {
            // 1D rotation task with currently no intervention.
            blockNumber = 1;
            if (runBlocks.Contains(blockNumber))
            {
                Block practiceblock = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeatsPractice; i++)
                {
                    Trial newTrial = practiceblock.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("practice", true);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", true);
                    newTrial.settings.SetValue("targetDegrees", 80f);
                }
                Block block = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeats; i++)
                {
                    Trial newTrial = block.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", true);
                    newTrial.settings.SetValue("targetDegrees", 80f);
                }
                string instructions = (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, to move to target and press A."
                );
                practiceblock.settings.SetValue("blockInstructions", instructions + practiceInstructions);
                block.settings.SetValue("blockInstructions", instructions + realInstructions);
                block.trials.Shuffle();
                Debug.Log("Block 1 Instructions: " + block.settings.GetString("blockInstructions"));
            }
            // 1D rotation pacer follower task. A pacer moves from start to end at constant speed, 
            // which subjects are asked to remain within. Randomized with vibrationTypes.
            blockNumber = 2;
            if (runBlocks.Contains(blockNumber))
            {
                Block practiceblock = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeatsPractice; i++)
                {
                    Trial newTrial = practiceblock.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("practice", true);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", true);
                    newTrial.settings.SetValue("targetDegrees", 80f);
                    newTrial.settings.SetValue("rotationPacer", true);
                    newTrial.settings.SetValue("vibration", "none");
                    newTrial.settings.SetValue("vibrationStartFraction", 0.0f);
                    newTrial.settings.SetValue("vibrationEndFraction", 1.0f);
                }
                Block block = uxfSession.CreateBlock();
                foreach (string vibration in uxfSession.settings.GetStringList("vibrationTypes"))
                {
                    foreach (float vibrationStartFraction in uxfSession.settings.GetFloatList("vibrationStartFractionTypes"))
                    {
                        for (int i = 0; i < numRepeats; i++)
                        {
                            {
                            Trial newTrial = block.CreateTrial();
                            newTrial.settings.SetValue("blockNumber", blockNumber);
                            newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                            newTrial.settings.SetValue("targetDegrees", 80f);
                            newTrial.settings.SetValue("rotationPacer", true);
                            newTrial.settings.SetValue("vibration", vibration);
                            newTrial.settings.SetValue("vibrationStartFraction", vibrationStartFraction);
                            newTrial.settings.SetValue("vibrationEndFraction", 1.0f);
                            }
                        }
                    }
                }

                string instructions = (
                    "Your task is to follow the pacer and press A when you reach the target. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for pacer then stay within it, until target is reached. "
                );
                practiceblock.settings.SetValue("blockInstructions", instructions + practiceInstructions);
                block.settings.SetValue("blockInstructions", instructions + realInstructions);
                block.trials.Shuffle();
                Debug.Log("Block 1 Instructions: " + block.settings.GetString("blockInstructions"));
            }
        }

        // 2D reaching tasks
        else if (uxfSession.settings.GetInt("nDims") == 2)
        {
            // Reaching tasks with visual rotation trial to trial. Most trials with full visual feedback, a subset with no visual feedback.
            blockNumber = 1;
            if (runBlocks.Contains(blockNumber))
            {
                Block practiceblock = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeatsPractice; i++)
                {
                    Trial newTrial = practiceblock.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("practice", true);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", true);
                    // newTrial.settings.SetValue("visualXrotation", visualXrotation);
                }
                Block block = uxfSession.CreateBlock();
                foreach (float visualXrotation in uxfSession.settings.GetFloatList("visualXrotations"))
                {
                    for (int i = 0; i < numRepeats; i++)
                    {
                        Trial newTrial = block.CreateTrial();
                        newTrial.settings.SetValue("blockNumber", blockNumber);
                        newTrial.settings.SetValue("controllerVisibleTrialStart", true);
                        newTrial.settings.SetValue("visualXrotation", visualXrotation);
                    }
                }
                for (int i = 0; i < numRepeats; i++)
                {
                    Trial newTrial = block.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                }
                string instructions = (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, to move to target and press A."+
                    "On some trials the controller sphere will be invisible."
                );
                practiceblock.settings.SetValue("blockInstructions", instructions + practiceInstructions);
                block.settings.SetValue("blockInstructions", instructions + realInstructions);
                block.trials.Shuffle();
                Debug.Log("Block 1 Instructions: " + block.settings.GetString("blockInstructions"));
            }
            // Reaching tasks with visual rotation trial to trial, with brief visual feedback during movement.
            blockNumber = 2;
            if (runBlocks.Contains(blockNumber))
            {
                Block practiceblock = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeatsPractice; i++)
                {
                    Trial newTrial = practiceblock.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                    newTrial.settings.SetValue("turnControllerVisibleMidpoint", true);
                    // newTrial.settings.SetValue("visualXrotation", visualXrotation);
                }
                Block block = uxfSession.CreateBlock();
                foreach (float visualXrotation in uxfSession.settings.GetFloatList("visualXrotations"))
                {
                    for (int i = 0; i < numRepeats; i++)
                    {
                        Trial newTrial = block.CreateTrial();
                        newTrial.settings.SetValue("blockNumber", blockNumber);
                        newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                        newTrial.settings.SetValue("turnControllerVisibleMidpoint", true);
                        newTrial.settings.SetValue("visualXrotation", visualXrotation);
                    }
                }
                string instructions = (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, to move to target and press A."+
                    "The controller sphere will be mostly invisible, except for a brief glimpse during movement."
                );
                practiceblock.settings.SetValue("blockInstructions", instructions + practiceInstructions);
                block.settings.SetValue("blockInstructions", instructions + realInstructions);
                block.trials.Shuffle();
                Debug.Log("Block 1 Instructions: " + block.settings.GetString("blockInstructions"));
            }

            // Reaching task without visual feedback and with vibration
            blockNumber = 3;
            if (runBlocks.Contains(blockNumber))
            {
                Block practiceblock = uxfSession.CreateBlock();
                for (int i = 0; i < numRepeatsPractice; i++)
                {
                    Trial newTrial = practiceblock.CreateTrial();
                    newTrial.settings.SetValue("blockNumber", blockNumber);
                    newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                    newTrial.settings.SetValue("vibration", "none");
                    newTrial.settings.SetValue("vibrationStartFraction", 0f);
                    newTrial.settings.SetValue("vibrationEndFraction", 1f);
                }
                Block block = uxfSession.CreateBlock();
                foreach (string vibration in uxfSession.settings.GetStringList("vibrationTypes"))
                {
                    for (int i = 0; i < numRepeats; i++)
                    {
                        Trial newTrial = block.CreateTrial();
                        newTrial.settings.SetValue("blockNumber", blockNumber);
                        newTrial.settings.SetValue("controllerVisibleTrialStart", false);
                        newTrial.settings.SetValue("vibration", vibration);
                        newTrial.settings.SetValue("vibrationStartFraction", 0f);
                        newTrial.settings.SetValue("vibrationEndFraction", 1f);
                    }
                }
                string instructions = (
                    "Your task is to indicate target location. "+
                    "Move controller within the start location (teal) "+
                    "and press B button to indicate readiness. "+
                    "Wait for target to turn green, to move to target and press A.\n"+
                    "Your controller sphere will be invisible during this task."
                );
                practiceblock.settings.SetValue("blockInstructions", instructions + practiceInstructions);
                block.settings.SetValue("blockInstructions", instructions + realInstructions);
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

            blockNumber = 5;
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
