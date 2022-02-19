using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class UndoChanges : MonoBehaviour
{
    public InputDevice leftController;
    public InputDevice rightController;
    private bool leftFound = false;
    private bool rightFound = false;
    [SerializeField] bool drawEnabled = false;

    void Start()
    {
        // FIND THE LEFT AND RIGHT CONTROLLERS.
        List<InputDevice> leftDevices = new List<InputDevice>();
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left;
        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, leftDevices);
        // If a device is found, store it within a local variable.
        if (leftDevices.Count > 0) {
            leftFound = true;
            leftController = leftDevices[0];
        }

        List<InputDevice> rightDevices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, rightDevices);
        // If a device is found, store it within a local variable.
        if (rightDevices.Count > 0) {
            rightFound = true;
            rightController = rightDevices[0];
        }

        Debug.Log("Left found: " + leftFound);
        Debug.Log("Right found: " + rightFound);
    }

    // Update is called once per frame
    void Update()
    {
        // Check the right controllers' primary button value to see
        // if a user has pressed it. If so, reset the sculpture.
        if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryIsPressed))
        {
            // If a button is pressed...
            if (primaryIsPressed)
            {
                // Set all meshes to visible.
                SetAllChildrenVisible();

                // Send a short haptic impulse.
                rightController.SendHapticImpulse(0u, 0.7f, 0.2f);
            }
        }
    }

    public void ToggleDrawing() {
        if (drawEnabled) {
            drawEnabled = false;
        } else {
            drawEnabled = true;
        }
    }

    public bool IsDrawEnabled() {
        if (drawEnabled) {
            return true;
        } else {
            return false;
        }
    }

    // Set all children objects to visible.
    // NOTE: There is currently an error in the naming of the blocks, such that 'Block Plane' is
    // conflated with 'Block Row'. In the future, those two should be flipped.
    private void SetAllChildrenVisible()
    {
        // Check each Block Plane
        foreach (Transform plane in transform)
        {
            // Check each Block Row
            foreach (Transform row in plane)
            {
                // Check each Block Part
                foreach (Transform block in row)
                {
                    MeshRenderer meshRenderer = block.gameObject.GetComponent("MeshRenderer") as MeshRenderer;
                    if (meshRenderer.enabled == false) {
                        meshRenderer.enabled = true;
                    }
                }
            }
        }
    }
}
