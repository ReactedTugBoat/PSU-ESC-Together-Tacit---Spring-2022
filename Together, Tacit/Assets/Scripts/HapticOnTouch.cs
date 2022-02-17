using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticOnTouch : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristics;
    private InputDevice activeController;
    private MeshRenderer meshRenderer = null;
    private XRBaseInteractable interactable = null;

    // Start is called before the first frame update
    void Start()
    {
        // Set the needed characteristics to those of the right VR controllers.
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);
        activeController = devices[0];

        // Store the meshrenderer and interactable.
        meshRenderer = GetComponent<MeshRenderer>();
        interactable = GetComponent<XRBaseInteractable>();

        // Set up events for when collision enters/exits the material.
        interactable.onHoverEnter.AddListener(StartHaptic);
        interactable.onHoverExit.AddListener(StopHaptic);
    }

    private void StartHaptic(XRBaseInteractor interactor)
    {
        // Start vibration in given controller.
        activeController.SendHapticImpulse(0u, 0.2f, 999999f);

    }

    private void StopHaptic(XRBaseInteractor interactor)
    {
        // Stop vibration in given controller.
        activeController.StopHaptics();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
