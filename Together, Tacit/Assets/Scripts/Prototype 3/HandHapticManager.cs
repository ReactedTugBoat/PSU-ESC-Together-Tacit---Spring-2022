using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public enum ControllerSideType {
    None,
    Left,
    Right
}

public enum ControllerState {
    Outside,
    Inside,
    Entering,
    Leaving,
    Tooling
}

public class HandHapticManager : MonoBehaviour
{
    // Controller state scripts.
    public ControllerTypeManager controllerTypeManager;
    public SerialMessagesScript serialMessagesScript;
    // Storage for controller properties.
    public SculptureCollisionBox thumb;
    public SculptureCollisionBox index;
    public SculptureCollisionBox middle;
    public SculptureCollisionBox oculus;
    public ControllerSideType side = ControllerSideType.None;
    private InputDeviceCharacteristics controllerCharacteristics;
    private InputDevice oculusController;
    private bool oculusControllerFound;
    private int pulseFrameCounter;

    public void Start()
    {
        // Based on the given controller side, store the input device characteristics.
        if (side == ControllerSideType.Left) {
            controllerCharacteristics = InputDeviceCharacteristics.Left;
        }
        else if (side == ControllerSideType.Right) {
            controllerCharacteristics = InputDeviceCharacteristics.Right;
        }
        else {
            // If a controller was not selected, an error is logged and the start functions stops.
            Debug.LogError("Input device not selected - could not instantiate.");
            return;
        }
    }

    public void Update()
    {
        // If the controller is currently not found, attempt to find it.
        if (!oculusControllerFound) {
            List<InputDevice> controllerDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, controllerDevices);
            if (controllerDevices.Count > 0) {
                // Once a controller has been found, store it and set controllerFound to true.
                oculusController = controllerDevices[0];
                oculusControllerFound = true;
            }
        }

        // Update the stored states based on what kind of controller is enabled.
        if (controllerTypeManager.CurrentControllerType() == GameplayControllerType.OCULUS_TOUCH) {
            // With an oculus controller is enabled, use just the state of the oculus collision box.

            if (oculus.collisionState == ControllerState.Entering || oculus.collisionState == ControllerState.Leaving) { 
                // If the controller is either entering or leaving, send a slightly stronger pulse.
                oculusController.SendHapticImpulse(0u, 0.5f, 0.1f);
            }
            else if (oculus.collisionState == ControllerState.Inside) {
                // If the controller is inside, send a constant weaker haptic.
                oculusController.SendHapticImpulse(0u, 0.3f, 0.5f);
            }
            else {
                // Otherwise the controller is outside, so disable all haptics.
                oculusController.StopHaptics();
            }
        }

        else {
            // With a haptic glove enabled, update each finger's state individually.
            // Pass these values to the appropriate serial messenger. All haptics involving the custom
            // gloves are dealt with within this script, and are just passed along from here.
            serialMessagesScript.SetFingerStates(thumb.collisionState, index.collisionState, middle.collisionState);
        }

        if (pulseFrameCounter > 0) {
            if (pulseFrameCounter % 10 == 1) {
                oculusController.SendHapticImpulse(0u, 1.0f, 0.3f);
            } else {
                oculusController.StopHaptics();
            }
            pulseFrameCounter--;
        }
    }

    public void SendCarvingHaptics() {
        // Send a haptic pulse to indicate that the tool is set to carving.
        pulseFrameCounter = 1;
    }

    public void SendAddingHaptics() {
        // Send a haptic pulse to indicate that the tool is set to adding.
        pulseFrameCounter = 11;
    }
}
