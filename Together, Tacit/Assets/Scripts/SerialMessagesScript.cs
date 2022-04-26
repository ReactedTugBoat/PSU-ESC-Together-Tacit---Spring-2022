using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum ControllerSide {
    Left,
    Right
}

public class SerialMessagesScript : MonoBehaviour
{
    // PUBLIC GAMEOBJECT SCRIPTS.
    public SerialController serialController;
    public HandHapticController controller;
    public HandToolController toolController;
    // NOTE: Script defaults to left controller.
    public ControllerSide controllerSide = ControllerSide.Left;
    private bool isControllerConnected = false;
    // PRIVATE VARIABLES.
    // Current finger flex values.
    private float thumbFlex;
    private float indexFlex;
    private float middleFlex;
    // Max values for each finger's flex.
    private float maxThumbFlex;
    private float maxIndexFlex;
    private float maxMiddleFlex;
    private bool areMaxFlexValuesSet;
    // Min values for each finger's flex.
    private float minThumbFlex;
    private float minIndexFlex;
    private float minMiddleFlex;
    private bool areMinFlexValuesSet;
    // Current finger states (inside, outside, etc...)
    [SerializeField] private ControllerState thumbState;
    [SerializeField] private ControllerState indexState;
    [SerializeField] private ControllerState middleState;
    // Finger rotation and collision scripts.
    public FingerRotation thumbRotation;
    public FingerRotation indexRotation;
    public FingerRotation middleRotation;
    public SculptureCollisionBox thumbCollision;
    public SculptureCollisionBox indexCollision;
    public SculptureCollisionBox middleCollision;
    // Counters and stored string values.
    private int frameCounter;
    private string comPort;
    private bool isComPortFound = false;

    void Awake()
    {
        // Determine the COM port for the given controller from the PROPERTIES file.
        // This should be set prior to running the program, and is only used when this
        // hand is set to use a Haptic Glove, as opposed to an Oculus Touch controller.
        string propertiesPath = Application.dataPath + "/PROPERTIES.txt";

        // The properties are read using a StreamReader, then stored locally.
        StreamReader reader = new StreamReader(propertiesPath);
        string propertiesString = reader.ReadToEnd();

        // Once reading the PROPERTIES file has finished, close the stream reader.
        reader.Close();

        // Split the PROPERTIES file up line-by-line.
        string[] propertiesLines = propertiesString.Split('\n');

        // Each hand has a specific tag associated with its COM port. Determine which tag to use
        // based on the local value of controllerSide.
        string propertiesTag;
        if (controllerSide == ControllerSide.Left) {
            propertiesTag = "left-controller-com-port";
        } else {
            propertiesTag = "right-controller-com-port";
        }

        // Run through the lines of the PROPERTIES file and check for matches with the given tag.
        foreach (string line in propertiesLines) {
            // If the Com port has been found, break from the loop;
            if (isComPortFound) {
                break;
            }

            // If the line begins with a "#" (denoting a comment line), skip it.
            if (line[0] == '#') {
                continue;
            }

            // Otherwise, check if the line contains the associated tag.
            if (line.Contains(propertiesTag)) {
                // Split the line according to "=" characters.
                // All properties are formatted as follows, so this should be consistent:
                //   ex. "left-controller-com-port=COM8"
                string[] splitProperty = line.Split('=');

                // If the COM field is left as whitespace (output from .Split()), set the COM port to an empty string and break.
                if (String.IsNullOrWhiteSpace(splitProperty[1])) {
                    comPort = "";
                    isComPortFound = true;
                    continue;
                }

                // Store the second split field, containing the COM port, and check if it is equal to COM10 or above.
                // if so, add an additional "\\.\" string just before the port name:
                //   ex. "COM12" becomes "\\.\COM12"
                // This is needed for the Ardity plugin to function properly with these higher COM ports.
                comPort = splitProperty[1];
                string comPortNum = comPort.Replace("COM", "");
                if (Convert.ToInt32(comPortNum) > 9) 
                {
                    // A Verbatim string is used here to allow the writing of \\.\ in quotations.
                    comPort = @"\\.\" + comPort;
                }

                // Set the bool indicating the COM port has been found.
                isComPortFound = true;
            }
        }

        // Set the COM port of the Serial controller to the value read from the PROPERTIES file.
        if (isComPortFound) {
            serialController.portName = comPort;
        }
        // If a COM port was not found correctly, flag an error.
        else {
            Debug.LogError("ERROR: COM port name unrecognized for " + controllerSide + " hand controller.");
        }

        // Stagger the frame counters between the left and right hands.
        // This helps to prevent messages from becoming cluttered between the left and right
        // controllers, and lowers the amount of messages that are dropped as a result.
        if (controllerSide == ControllerSide.Left) {
            frameCounter = 0;
        } else {
            frameCounter = 1;
        }
    }
    
    void Update()
    {
        // Update finger states based on current collision states.
        thumbState = thumbCollision.collisionState;
        indexState = indexCollision.collisionState;
        middleState = middleCollision.collisionState;
        
        // If a max and min flex value have been set, map the current flex sensor values to a range
        // between 0 and 40 degrees, then set the finger's rotation to that value.
        if (areMaxFlexValuesSet && areMinFlexValuesSet) {
            thumbRotation.SetFingerFlexValue(map(thumbFlex, minThumbFlex, maxThumbFlex, 0, 40));
            indexRotation.SetFingerFlexValue(map(indexFlex, minIndexFlex, maxIndexFlex, 0, 40));
            middleRotation.SetFingerFlexValue(map(middleFlex, minMiddleFlex, maxMiddleFlex, 0, 40));

            // Whenever a user brings their fingers together into a fist, call the tooling command for the given hand.
            float toolRotationThreshold = 36f;
            bool isThumbFlexed = (thumbRotation.GetFingerFlexValue() > toolRotationThreshold);
            bool isIndexFlexed = (indexRotation.GetFingerFlexValue() > toolRotationThreshold);
            bool isMiddleFlexed = (middleRotation.GetFingerFlexValue() > toolRotationThreshold);
            if (isThumbFlexed && isIndexFlexed && isMiddleFlexed) {
                toolController.Tooling();
            }
        }

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

        if (frameCounter == 9) {
            // Send the completed haptic messages to the necessary serial port for parsing.
            serialController.SendSerialMessage(hapticString);
            // Debug.Log("Sending Serial Message: " + hapticString + " from " + controllerSide + " side");
            frameCounter = 0;
            return;
        }

        frameCounter++;

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
                // Debug.LogError("Recieved message was not of expected length: expected = 4, actual = " + splitMsg.Length);
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
        if (success) {
            // Debug.Log("Connection established");
            isControllerConnected = true;
        } else {
            // Debug.Log("Connection attempt failed or disconnection detected");
            isControllerConnected = false;
        }
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
                hapticString += "h";
            }
            // If current finger is inside, snd a haptic message according to the hand's current velocity.
            // TODO: Incorperate velocity. Currently, it just sets the haptics to a set value (100).
            else if (state == ControllerState.Inside) {
                hapticString += "i200";
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

    public bool IsControllerConnected() {
        // Returns whether the controller is currently found by the Serial Controller.
        if (isControllerConnected) {
            return true;
        } else {
            return false;
        }
    }

    // Method to map a float number within a range (a1 to a2) to a new range (b1 to b2).
    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
    }
}