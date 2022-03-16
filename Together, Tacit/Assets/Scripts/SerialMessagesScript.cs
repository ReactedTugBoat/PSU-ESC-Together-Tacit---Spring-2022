using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerSide {
    Left,
    Right
}

public class SerialMessagesScript : MonoBehaviour
{
    // PUBLIC GAMEOBJECTS.
    public SerialController serialController;
    public BlockManager blockManager;
    public HandHapticController controller;
    // Script defaults to left controller.
    public ControllerSide controllerSide = ControllerSide.Left;
    // PRIVATE VARIABLES.
    // Current finger flex values.
    private float thumbFlex;
    private float indexFlex;
    private float middleFlex;
    // Max values for each finger's flex.
    private float maxThumbFlex;
    private float maxIndexFlex;
    private float maxMiddleFlex;
    // Min values for each finger's flex.
    private float minThumbFlex;
    private float minIndexFlex;
    private float minMiddleFlex;
    // Current finger states (inside, outside, etc...)
    [SerializeField] private ControllerState thumbState;
    [SerializeField] private ControllerState indexState;
    [SerializeField] private ControllerState middleState;

    void Start() {
        // Initialize the serial controller.
        serialController = GameObject.Find(controllerSide + "SerialController").GetComponent<SerialController>();

        // BEGIN CONTROLLER INITIALIZATION.
        // Before the program is run, the serial connection needs to initialize a number of values
        // for each controller - communicated to and from the Arduino code.
        // In order to prevent blocking issues, both controllers will have their initializations run
        // as a part of coroutines, using keyboard inputs to help with moving through the steps.

    }
    
    void Update() {
        // SENDING MESSAGES.
        // Every frame, send a message containing data for each finger.
        // The data is sent as a string in the following format...
        //
        // "d,o124,i120,c000"
        // 1) Each string begins with a "d" character, to designate it as a haptic call.
        // 2) Each finger stores its new haptic command as a string of four characters:
        //    starting with a letter and followed by a three digit number, with the following...
        //      o*** -> Outside (*** is an integer value between 000 and 255)
        //      i*** -> Inside  (*** is an integer value between 000 and 255)
        //      c000 -> Collide/Entering
        //      e000 -> Exit/Leaving
        //      s000 -> Sculpt/Tooling
        // 3) The code for each finger is comma-separated, in the order: Thumb, Index, Middle.
        //
        // To start, gather the states of each finger at the current frame.
        // Todo: Calibrate finger states and store them. For now, these values are simply hard coded.
        thumbState = ControllerState.Inside;
        indexState = ControllerState.Inside;
        middleState = ControllerState.Inside;
        // Once calculated, use the finger states to generate haptic strings for both fingers.
        string hapticString;
        BuildHapticMessage(controller, thumbState, indexState, middleState, out hapticString);
        
        // Send the completed haptic messages to the necessary serial port for parsing.
        serialController.SendSerialMessage(hapticString);
    }

    // Whenever a message arrives from the Adruino (Ideally, once a frame, though actual timings may vary),
    // parse the message for flex sensor values and store those locally.
    void OnMessageArrived(string msg)
    {
        // Debug.Log("Message Arrived: " + msg);
        // Check to see if the message begins with a "f" character, indicating a message including flex sensor values.
        if (msg[0] == 'f') {
            // The format of flex sensor messages is as follows...
            // 
            // "f,120,255,052"
            // 1) Each string begins with an 'f' character
            // 2) The remaining fields are comma separated, and represent the flex sensors for
            //    the thumb, index, and middle fingers, in that order.
            // Once a message is found, split it by the commas into its individual parts.
            string[] splitMsg = msg.Split(',');
            if (splitMsg.Length != 4) {
                // If a recieved message is incorrectly parsed, log an error and return.
                Debug.LogError("Recieved message was not of expected length: expected = 4, actual = " + splitMsg.Length);
                return;
            }

            // Set the new current values for the flex sensors of each finger.
            thumbFlex = float.Parse(splitMsg[1]);
            indexFlex = float.Parse(splitMsg[2]);
            middleFlex = float.Parse(splitMsg[3]);

            // Debug.Log(controllerSide + " Controller Flex Values: thumb = " + thumbFlex + ", index = " + indexFlex + ", middle = " + middleFlex);
        }
    }

    private void BuildHapticMessage(HandHapticController controller,
                                        ControllerState thumb, 
                                        ControllerState index, 
                                        ControllerState middle, 
                                        out string hapticString) 
    {
        // Create an introductory string, beginning with a "d" character.
        hapticString = "h";

        // Store the provided finger states in an array
        ControllerState[] fingerArray = new ControllerState[] {thumb, index, middle};

        // For each finger, build a 4 character representation based on the state and distance from sculpture.
        // Once each rep is build, append it to the haptic message.
        foreach (ControllerState state in fingerArray) {
            // Add a "," character to separate each part of the message.
            hapticString += ",";

            // If the current finger is far outside, send a zeroed haptic message (i.e. no haptics).
            if (state == ControllerState.FarOutside) {
                hapticString += "o000";
            }
            // If the current finger is outside, send a haptic message according to current distance from blocks.
            else if (state == ControllerState.Outside) {
                // Adjust the current distance to be an integer value between 0 and 255
                int adjustedBlockDistance = (int)Math.Floor(controller.shortestDistance * 25500);
                if (adjustedBlockDistance < 0) {
                    adjustedBlockDistance = 0;
                }
                if (adjustedBlockDistance > 255) {
                    adjustedBlockDistance = 255;
                }

                // Add any leading zeros needed to the frequency value (must be three digits long).
                string leadingZeros = "";
                int numLength = adjustedBlockDistance.ToString().Length;
                if (numLength == 1) {
                    leadingZeros = "00";
                } else if (numLength == 2) {
                    leadingZeros = "0";
                }

                // Add all calculated parts to the haptic string.
                hapticString += String.Format("o{0}{1}", leadingZeros, adjustedBlockDistance);
            }
            // If current finger is inside, snd a haptic message according to the hand's current velocity.
            // TODO: Incorperate velocity. Currently, it just sets the haptics to a set value (100).
            else if (state == ControllerState.Inside) {
                hapticString += "i100";
            }
            // If current finger is entering, exiting, or tooling, send a haptic message accordingly.
            else if (state == ControllerState.Entering) {
                hapticString += "c000";
            }
            else if (state == ControllerState.Leaving) {
                hapticString += "e000";
            }
            else if (state == ControllerState.Tooling) {
                hapticString += "s000";
            }
            // In any other case, set the haptic message to "Error" and return.
            else {
                hapticString = "Error";
                return;
            }
        }

    }

}
