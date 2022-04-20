using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public enum ControllerType {
    None,
    Left,
    Right
}

// Public class to store the haptic behavior of the VR hands. Designed as a general script, which
// can be adjusted to fit either hand as needed.
public class HandHapticController : MonoBehaviour
{
    // Before this script can be run, the type of controller must be selected within Unity.
    public ControllerType side = ControllerType.None;
    public ControllerState controllerState;
    private InputDeviceCharacteristics controllerCharacteristics;
    private InputDevice controller;
    private int currentBlockCount = 0;
    private XRDirectInteractor interactor = null;
    // Values for the hand's distance to the nearest block are initialized to high values, beyond the range
    // of the Unity scene, to help recognize errors when they occur.
    private float currentDistance;
    public float shortestDistance;

    void Start()
    {
        // LOCATE THE HAND'S INTERACTOR.
        interactor = GetComponent<XRDirectInteractor>();

        // FIND CONTROLLER CHARACTERISTICS.
        if (side == ControllerType.Left) {
            controllerCharacteristics = InputDeviceCharacteristics.Left;
        }
        else if (side == ControllerType.Right) {
            controllerCharacteristics = InputDeviceCharacteristics.Right;
        }
        else {
            // If a controller was not selected, an error is logged and the start functions stops.
            Debug.LogError("Input device not selected - could not instantiate.");
            return;
        }

        // LOCATE THE CONTROLLER DEVICE.
        List<InputDevice> controllerDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, controllerDevices);
        if (controllerDevices.Count > 0) {
            controller = controllerDevices[0];
        } else {
            Debug.LogError("Device " + side + " could not be found.");
        }
    }

    public void IncreaseBlockCount() {
        // Debug.Log("Increasing block count");
        currentBlockCount++;
    }

    public void DecreaseBlockCount() {
        currentBlockCount--;
        if (currentBlockCount < 0) {
            Debug.LogError("Error: Number of blocks interacting with hand fell below 0.");
        }
    }
    
    // A short haptic pulse, to be used when an art block is carved away.
    public void SendDeletePulse() {
        // Stop any currently running haptics.
        controller.StopHaptics();

        // Send a short haptic pulse, slightly stronger than those used to find the scuplture.
        controller.SendHapticImpulse(0u, 0.3f, 0.1f);
    }

    public void SendAddPulse() {
        // Stop any currently running haptics.
        controller.StopHaptics();

        // Send a short haptic pulse, slightly stronger than those used to find the scuplture.
        controller.SendHapticImpulse(0u, 0.3f, 0.1f);

    }

    private void BeginHaptics() {
        controller.SendHapticImpulse(0u, 0.1f, 9999f);
    }

    private void EndHaptics() {
        controller.StopHaptics();
    }

}
