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
    public HandHapticController controller;
    // Script defaults to left controller.
    public ControllerSide controllerSide = ControllerSide.Left;
    // PRIVATE VARIABLES.
    // Current finger flex values.
    [SerializeField] private float thumbFlex;
    [SerializeField] private float indexFlex;
    [SerializeField] private float middleFlex;
    // Max values for each finger's flex.
    private float maxThumbFlex;
    private float maxIndexFlex;
    private float maxMiddleFlex;
    [SerializeField] private bool areMaxFlexValuesSet;
    // Min values for each finger's flex.
    private float minThumbFlex;
    private float minIndexFlex;
    private float minMiddleFlex;
    [SerializeField] private bool areMinFlexValuesSet;
    // Current finger states (inside, outside, etc...)
    [SerializeField] private ControllerState thumbState;
    [SerializeField] private ControllerState indexState;
    [SerializeField] private ControllerState middleState;
    // Finger rotation scripts.
    public FingerRotation thumbRotation;
    public FingerRotation indexRotation;
    public FingerRotation middleRotation;

    private int count = 0;
    
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
        // Once calculated, use the finger states to generate haptic strings for both fingers.
        string hapticString;
        BuildHapticMessage(controller, thumbState, indexState, middleState, out hapticString);
        
        if (count == 1) {
            // Send the completed haptic messages to the necessary serial port for parsing.
            serialController.SendSerialMessage(hapticString);
            // Debug.Log("Sending Serial Message: " + hapticString);
            count = 0;
            return;
        }

        count++;

        // If a max and min flex value have been set, map the current flex sensor values to a range
        // between 0 and 40 degrees, then set the finger's rotation to that value.
        if (areMaxFlexValuesSet && areMinFlexValuesSet) {
            thumbRotation.SetFingerFlexValue(map(thumbFlex, minThumbFlex, maxThumbFlex, 0, 40));
            indexRotation.SetFingerFlexValue(map(indexFlex, minIndexFlex, maxIndexFlex, 0, 40));
            middleRotation.SetFingerFlexValue(map(middleFlex, minMiddleFlex, maxMiddleFlex, 0, 40));
        }
    }

    // Whenever a message arrives from the Adruino (Ideally, once a frame, though actual timings may vary),
    // parse the message for flex sensor values and store those locally.
    void OnMessageArrived(string msg)
    {
        if (msg == "") {
            return;
        }
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

    void OnConnectionEvent(bool success)
    {
        if (success)
            Debug.Log("Connection established");
        else
            Debug.Log("Connection attempt failed or disconnection detected");
    }

    private void BuildHapticMessage(HandHapticController controller,
                                        ControllerState thumb, 
                                        ControllerState index, 
                                        ControllerState middle, 
                                        out string hapticString) 
    {
        // Create an introductory string, beginning with a "d" character.
        hapticString = "d";

        // Store the provided finger states in an array
        ControllerState[] fingerArray = new ControllerState[] {thumb, index, middle};

        // For each finger, build a 4 character representation based on the state and distance from sculpture.
        // Once each rep is build, append it to the haptic message.
        foreach (ControllerState state in fingerArray) {
            // Add a "," character to separate each part of the message.
            hapticString += ",";

            // If the current finger is far outside, send a zeroed haptic message (i.e. no haptics).
            if (state == ControllerState.Outside) {
                // hapticString += "o000";
                hapticString = "h";
                return;
            }
            // If current finger is inside, snd a haptic message according to the hand's current velocity.
            // TODO: Incorperate velocity. Currently, it just sets the haptics to a set value (100).
            else if (state == ControllerState.Inside) {
                hapticString += "i255";
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

    // Private method to initialize the rotation of the controllers and the max/min value of each flex sensor.
    // Utilizes a coroutine to block for button inputs.
    IEnumerator Initaliztion() {
        Debug.Log(controllerSide + " Initalization!\n" + "Calibrating " + controllerSide + " controller, press A to continue");

        // Wait for a keyboard press.
        while(!Input.GetKeyDown(KeyCode.A)) {
            yield return null;
        }

        // Set the rotation of the hand model to the current rotation of the controller.
        GameObject hand = GameObject.Find(controllerSide + "Hand Controller");
        hand.GetComponentInChildren<Transform>().transform.rotation = hand.transform.rotation;

        Debug.Log(controllerSide + " Rotation Set!");

    }

    public void SetFingerStates(ControllerState thumb, ControllerState index, ControllerState middle)
    {
        // Public method to update the states of each finger on a given hand.
        thumbState = thumb;
        indexState = index;
        middleState = middle;
    }

    public void SetMaxFlexValues()
    {
        // When called, stores the current flex values as the maximum flex sensor values.
        maxThumbFlex = thumbFlex;
        maxIndexFlex = indexFlex;
        maxMiddleFlex = middleFlex;
        areMaxFlexValuesSet = true;
    }

    public void SetMinFlexValues()
    {
        // When called, stores the current flex values as the maximum flex sensor values.
        minThumbFlex = thumbFlex;
        minIndexFlex = indexFlex;
        minMiddleFlex = middleFlex;
        areMinFlexValuesSet = true;
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
    }
}