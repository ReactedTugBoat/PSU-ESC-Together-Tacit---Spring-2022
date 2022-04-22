using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneOptionsManager : MonoBehaviour
{
    // A class dedicated to the different interactions available within the options menu.
    // Currently supports changing the currently set sculpture type, controller types
    // (either oculus touch or haptic glove), and resetting the origin of the 3D model.

    // Public objects.
    public ControllerTypeManager leftControllerManager;
    public ControllerTypeManager rightControllerManager;
    public SerialMessagesScript leftSerialManager;
    public SerialMessagesScript rightSerialManager;
    public VoxelManager voxelManager;
    public Toggle sculptureTypeToggle;
    public Toggle leftControllerOculusToggle;
    public Toggle rightControllerOculusToggle;
    public Text leftControllerConnecting;
    public Text leftControllerConnected;
    public Text rightControllerConnecting;
    public Text rightControllerConnected;
    // Private booleans.
    private bool isLeftSetToGlove;
    private bool isRightSetToGlove;
    
    void Start()
    {
        // At startup, update all of the various scripts to the current options menu.
        UpdateSculptureType();
        UpdateLeftControllerType();

        // Set all of the connection messages (left or right) to be disabled initially.
        leftControllerConnecting.enabled = false;
        leftControllerConnected.enabled = false;
        rightControllerConnecting.enabled = false;
        rightControllerConnected.enabled = false;
    }

    void Update()
    {
        // If either hand is set to Haptic Gloves, show messages based on whether the glove
        // is connected or not. Otherwise, hide those messages entirely.
        if (isLeftSetToGlove) {
            // Check if the left glove is connected.
            if (leftSerialManager.IsControllerConnected()) {
                // If connected, display messages to indicate connection.
                leftControllerConnected.enabled = true;
                leftControllerConnecting.enabled = false;
            } else {
                // If not connected, display messages to tell the user that connection is ongoing.
                leftControllerConnected.enabled = false;
                leftControllerConnecting.enabled = true;
            }
        } else {
            // If left is set to Oculus Touch, disable all messages.
            leftControllerConnected.enabled = false;
            leftControllerConnecting.enabled = false;
        }

        if (isRightSetToGlove) {
            // Check if the right glove is connected.
            if (rightSerialManager.IsControllerConnected()) {
                // If connected, display messages to indicate connection.
                rightControllerConnected.enabled = true;
                rightControllerConnecting.enabled = false;
            } else {
                // If not connected, display messages to tell the user that connection is ongoing.
                rightControllerConnected.enabled = false;
                rightControllerConnecting.enabled = true;
            }
        } else {
            // If right is set to Oculus Touch, disable all messages.
            rightControllerConnected.enabled = false;
            rightControllerConnecting.enabled = false;
        }
    }

    // MENU OPTION METHODS.
    public void UpdateSculptureType()
    {
        // Updates the currently set sculpture type to the setting in the menu.
        // This won't immediately change the sculpture, and will instead wait for the scene
        // to be reset before changing ingame.
        // In this setting, "On" refers to when the toggles are set to "Sphere".
        if (sculptureTypeToggle.isOn) {
            voxelManager.SetCurrentModelType(STARTING_MODEL.SPHERE);
        } else {
            voxelManager.SetCurrentModelType(STARTING_MODEL.CUBE);
        }
    }

    public void UpdateLeftControllerType()
    {
        // Updates the left controller to the currently set controller type, as set in the menu.

        // If toggle buttons are currently set to "Oculus Touch", instruct the left controller
        // manager to update accordingly. Otherwise, call the opposing method.
        // For simplicity, we only check the toggle button containing the 'toggle group' script, 
        // as one will always be able to detect the state of the other.
        if (leftControllerOculusToggle.isOn) {
            // Set to Oculus Touch and update local booleans.
            leftControllerManager.SetControllerToOculus();
            isLeftSetToGlove = false;
        } else {
            // Set to Haptic Glove and update local booleans.
            leftControllerManager.SetControllerToGlove();
            isLeftSetToGlove = true;
        }
    }

    public void UpdateRightControllerType()
    {
        // Updates the right controller to the currently set controller type, as set in the menu.

        // If toggle buttons are currently set to "Oculus Touch", instruct the left controller
        // manager to update accordingly. Otherwise, call the opposing method.
        // For simplicity, we only check the toggle button containing the 'toggle group' script, 
        // as one will always be able to detect the state of the other.
        if (rightControllerOculusToggle.isOn) {
            // Set to Oculus Touch and update local booleans.
            rightControllerManager.SetControllerToOculus();
            isRightSetToGlove = false;
        } else {
            // Set to Haptic Glove and update local booleans.
            rightControllerManager.SetControllerToGlove();
            isRightSetToGlove = true;
        }
    }
}
