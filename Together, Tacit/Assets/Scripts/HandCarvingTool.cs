using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandCarvingTool : MonoBehaviour
{
    // CLASS INSTANCES.
    // To track whether drawing is enabled or not, this class takes an instance
    // of the UndoChanges class from its greatest parent object.
    private GameObject parentArtBlock;
    private UndoChanges parentScript;

    // LOCAL VARIABLES.
    // Game object components.
    private MeshRenderer meshRenderer = null;
    private XRBaseInteractable interactable = null;
    // VR Input Device objects.
    private InputDevice leftController;
    private InputDevice rightController;

    private void Start()
    {
        // LOCATE THE MESH RENDERER FOR THIS GAMEOBJECT.
        meshRenderer = GetComponent<MeshRenderer>();

        // FIND DERIVATIVES OF THE GREATEST PARENT OBJECT.
        parentArtBlock = GameObject.Find("/Art Blocks");

        // Track the largest parent to find whether drawing is enabled.
        parentScript = parentArtBlock.GetComponent<UndoChanges>();

        // Store the controller objects for the VR controllers.
        leftController = parentScript.leftController;
        rightController = parentScript.rightController;

        // ADD A LISTENERS FOR WHEN A HAND ENTERS/EXITS THIS BLOCK.
        interactable = GetComponent<XRBaseInteractable>();
        interactable.onHoverEnter.AddListener(Draw);
        interactable.onHoverEnter.AddListener(StartHaptic);
        interactable.onHoverExit.AddListener(StopHaptic);
    }

    private void OnDestroy()
    {
        // Remove any listeners left on the object after it is destroyed.
        interactable.onHoverEnter.RemoveListener(Draw);
        interactable.onHoverEnter.RemoveListener(StartHaptic);
        interactable.onHoverExit.RemoveListener(StopHaptic);
    }

    private void Draw(XRBaseInteractor interactor)
    {
        // If the parent isnt instantiated, log an error.
        if (parentScript == null) {
            Debug.LogError("parent not found.");
        }

        // If drawing is enabled, set the material to disabled.
        if (parentScript.IsDrawEnabled()) {
            meshRenderer.enabled = false;
        }
    }

    private void StartHaptic(XRBaseInteractor interactor)
    {
        if (meshRenderer.enabled) {
            leftController.SendHapticImpulse(0u, 0.2f, 9999f);
        }
    }

    private void StopHaptic(XRBaseInteractor interactor)
    {
        leftController.StopHaptics();
    }
}
