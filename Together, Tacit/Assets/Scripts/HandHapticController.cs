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

public enum ControllerState {
    FarOutside,
    Outside,
    Inside,
    Entering,
    Leaving,
    Tooling
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
    private int prevBlockCount = 0;
    private bool controllerIsEntering = false;
    private bool controllerIsLeaving = false;
    private XRDirectInteractor interactor = null;
    private GameObject blockManager = null;
    // private GameObject[] blocks = null;
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

        // LOCATE THE BLOCK MANAGER.
        blockManager = GameObject.Find("Block Manager");
    }

    void Update()
    {
        // CALCULATE THE DISTANCE OF THE HAND FROM THE CLOSEST BLOCK IN THE SCENE.
        // This is done by calculating the distance from every block and saving the lowest value.
        // Calculate the distance between the each game object with an enabled mesh
        // renderer, storing the smallest value as it goes along.
        int enabledBlockCount = 0;
        currentDistance = 9999F;
        shortestDistance = 9999F;
        if (blockManager.GetComponent<BlockManager>().hasSculptureBeenGenerated == true) {
            foreach (Transform blockTransform in blockManager.transform) {
                // Only calculate the distance from a block if its mesh renderer is enabled.
                if (blockTransform.GetComponent<MeshRenderer>().enabled){
                    currentDistance = Vector3.Distance(transform.position, blockTransform.position);
                    if (currentDistance < shortestDistance) {
                        shortestDistance = currentDistance;
                        // Debug.Log("Shortest Distance Reset to: " + shortestDistance);
                    }
                    enabledBlockCount++;
                }
            }
        }
        // Debug.Log("Closest Block Position = " + closestBlock.position);
        // Debug.Log(side + "Hand Position = " + this.transform.position);
        // Debug.Log("Enabled block count = " + enabledBlockCount);
        // Debug.Log("Shortest distance for " + side + " hand = " + shortestDistance);

        // COMPARE BLOCK COUNTS BETWEEN FRAMES.
        // Using these comparisons prevents a ton of unneccesary 'SendHapticImpulse' and
        // 'StopHaptics' calls from occuring, which would only slow runtime.
        // If the prev count was 0 and the current count is greater than 0, enable haptics.
        if (prevBlockCount == 0 && currentBlockCount > 0) {
            BeginHaptics();
            controllerIsEntering = true;
        }
        // If the prev count was greater than 0 and the current count is 0, disable haptics.
        else if (prevBlockCount > 0 && currentBlockCount == 0) {
            EndHaptics();
            controllerIsLeaving = true;
        }

        // STORE CURRENTBLOCKCOUNT IN PREV.
        prevBlockCount = currentBlockCount;

        // STORE THE CURRENT STATE/POSITION OF THIS CONTROLLER.
        // On any given frame, a controller can have four states: far outside, outside, inside, entering, or leaving.
        // These five states are determined as follows:
        //   Far outside = controller is further than 10cm from the closest point on the sculpture.
        //   Outside = controller is within 10cm of the sculpture, but not touching it or within it.
        //   Inside = controller is within a block of the sculpture (i.e. touching at least 1 block).
        //   Entering = controller is passing from outside to inside
        //   Leaving = controller is passing from inside to outside.
        // Using the shortest distance block and the current block count, figure out what state the controller is in.

        // If the flags for either the controller entering or leaving are set, set the state accordingly.
        if (controllerIsEntering) {
            controllerState = ControllerState.Entering;
            controllerIsEntering = false;
        }
        else if (controllerIsLeaving) {
            controllerState = ControllerState.Leaving;
            controllerIsLeaving = false;
        }
        // If the current block count is greater than 0, we know that the state is currently inside.
        else if (currentBlockCount > 0) {
            controllerState = ControllerState.Inside;
        }
        // If the closest block is within 40cm (i.e. 0.4 units), the state is outside. Otherwise, it is far outside.
        else if (shortestDistance < 0.4) {
            controllerState = ControllerState.Outside;
        }
        else {
            controllerState = ControllerState.FarOutside;
        }
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

    public void SendAddPulse() {
        // Stop any currently running haptics.
        controller.StopHaptics();

        // Send a short haptic pulse, slightly stronger than those used to find the scuplture.
        controller.SendHapticImpulse(0u, 0.3f, 0.1f);

    }

    public void GenerateSculpture() {
        // In order to generate the sculpture using a button press, this wrapper
        // method is used to obtain the controller's current position.
        Vector3 currentPosition = transform.position;
        blockManager.GetComponent<BlockManager>().GenerateSculpture(currentPosition);
    }

    private void BeginHaptics() {
        controller.SendHapticImpulse(0u, 0.1f, 9999f);
    }

    private void EndHaptics() {
        controller.StopHaptics();
    }

}
