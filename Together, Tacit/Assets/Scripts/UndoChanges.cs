using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class UndoChanges : MonoBehaviour
{
    public InputDeviceCharacteristics rightControllerCharacteristics;
    private InputDevice rightController;

    // Start is called before the first frame update
    void Start()
    {
        // Set the needed characteristics to those of the right VR controller.
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
        rightController = devices[0];
    }

    // Update is called once per frame
    void Update()
    {
        // Check both controllers' primary button values.
        if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
        {
            // If a button is pressed...
            if (isPressed)
            {
                // Set all meshes to visible.
                SetAllChildrenVisible();

                // Send a short haptic impulse.
                rightController.SendHapticImpulse(0u, 0.7f, 0.2f);
            }
        }
    }

    // Set all children objects to visible.
    // NOTE: There is currently an error in the naming of the blocks, such that 'Block Plane' is
    // conflated with 'Block Row'. In the future, those two should be flipped.
    void SetAllChildrenVisible()
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
