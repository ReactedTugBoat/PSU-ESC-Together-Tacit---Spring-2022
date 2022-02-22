using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

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
    private InputDeviceCharacteristics controllerCharacteristics;
    private InputDevice controller;
    private int currentBlockCount = 0;
    private int prevBlockCount = 0;


    void Start()
    {
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

    void Update()
    {
        // COMPARE BLOCK COUNTS BETWEEN FRAMES.
        // Using these comparisons prevents a ton of unneccesary 'SendHapticImpulse' and
        // 'StopHaptics' calls from occuring, which would only slow runtime.
        // If the prev count was 0 and the current count is greater than 0, enable haptics.
        if (prevBlockCount == 0 && currentBlockCount > 0) {
            BeginHaptics();
        }
        // If the prev count was greater than 0 and the current count is 0, disable haptics.
        else if (prevBlockCount > 0 && currentBlockCount == 0) {
            EndHaptics();
        }

        // STORE CURRENTBLOCKCOUNT IN PREV.
        prevBlockCount = currentBlockCount;
    }

    public void IncreaseBlockCount() {
        ++currentBlockCount;
    }

    public void DecreaseBlockCount() {
        --currentBlockCount;
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

    private void BeginHaptics() {
        controller.SendHapticImpulse(0u, 0.2f, 9999f);
    }

    private void EndHaptics() {
        controller.StopHaptics();
    }

    
}
