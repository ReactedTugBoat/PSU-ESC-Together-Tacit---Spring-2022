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
    // In keep up with haptic feedback, functions are imported from
    // the HandHapticController components of each hand individually.
    // The names of these controllers will need to be updated accordingly, along with a leading '/' tag.
    // TODO: Figure out a better way of storing these names...
    private string leftControllerName = "LeftHand Controller";
    private HandHapticController leftControllerHaptics;
    private string rightControllerName = "RightHand Controller";
    private HandHapticController rightControllerHaptics;

    // LOCAL VARIABLES.
    // Game object components.
    private MeshRenderer meshRenderer = null;
    private XRBaseInteractable interactable = null;

    private void Start()
    {
        // LOCATE THE MESH RENDERER FOR THIS GAMEOBJECT.
        meshRenderer = GetComponent<MeshRenderer>();

        // FIND DERIVATIVES OF THE GREATEST PARENT OBJECT.
        parentArtBlock = GameObject.Find("/Block Manager");

        // Track the largest parent to find whether drawing is enabled.
        parentScript = parentArtBlock.GetComponent<UndoChanges>();

        // FIND THE HAPTIC SCRIPT OF BOTH VR CONTROLLERS.
        leftControllerHaptics = GameObject.Find(leftControllerName).GetComponent<HandHapticController>();
        rightControllerHaptics = GameObject.Find(rightControllerName).GetComponent<HandHapticController>();

        // ADD A LISTENERS FOR WHEN A HAND ENTERS/EXITS THIS BLOCK.
        interactable = GetComponent<XRBaseInteractable>();
        interactable.onHoverEnter.AddListener(Draw);
    }

    private void OnDestroy()
    {
        // Remove any listeners left on the object after it is destroyed.
        interactable.onHoverEnter.RemoveListener(Draw);
    }

    private void Draw(XRBaseInteractor interactor)
    {
        // If the parent isnt instantiated, log an error.
        if (parentScript == null) {
            Debug.LogError("parent not found.");
        }

        // If drawing is enabled, set the material to disabled.
        if (parentScript.IsDrawEnabled() && meshRenderer.enabled) {
            // Disable the material.
            meshRenderer.enabled = false;

            // Send a haptic pulse to show the user a block as been deleted.
            leftControllerHaptics.SendDeletePulse();
            rightControllerHaptics.SendDeletePulse();
        }
    }
}
