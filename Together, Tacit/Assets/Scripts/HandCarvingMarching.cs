using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandCarvingMarching : MonoBehaviour
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
        interactable.onHoverEnter.AddListener(RaiseBlockCount);
        interactable.onHoverExit.AddListener(LowerBlockCount);
    }

    private void OnDestroy()
    {
        // Remove any listeners left on the object after it is destroyed.
        interactable.onHoverEnter.RemoveListener(RaiseBlockCount);
        interactable.onHoverExit.RemoveListener(LowerBlockCount);
    }

    private void RaiseBlockCount(XRBaseInteractor interactor) {
        Debug.Log("Entered");
        // Upon entering an interactable, raise the block count of the specific controller.
        if (meshRenderer.enabled == true) {
            interactor.GetComponentInParent<HandHapticController>().IncreaseBlockCount();
        }
    }

    private void LowerBlockCount(XRBaseInteractor interactor) {
        Debug.Log("Exited");
        // Upon leaving an interactable, lower the block count of the specific controller.
        if (meshRenderer.enabled == true) {
            interactor.GetComponentInParent<HandHapticController>().DecreaseBlockCount();
        }
    }

    // private void CarveOrAdd(XRBaseInteractor interactor)
    // {
    //     // Upon entering any interactable, check to see if its mesh renderer is enabled. If so,
    //     // increase the block count of the interactor element.
    //     if (meshRenderer.enabled == true) {
    //         interactor.GetComponentInParent<HandHapticController>().IncreaseBlockCount();
    //     }

    //     // This method controls both carving and additive behaviors, depending on the user's selection.
    //     // If the parent isnt instantiated, log an error.
    //     if (parentScript == null) {
    //         Debug.LogError("parent not found.");
    //     }

    //     // If the tool is set to carving away material...
    //     if (!parentScript.IsAddEnabled()) {
    //         // If drawing is enabled, set the material to disabled.
    //         if (parentScript.IsDrawEnabled() && meshRenderer.enabled == true) {
    //             // Disable the material.
    //             meshRenderer.enabled = false;

    //             // Decrease the block count to stay consistent with the state of the mesh renderer.
    //             interactor.GetComponentInParent<HandHapticController>().DecreaseBlockCount();

    //             // Send a haptic pulse to show the user a block as been deleted.
    //             interactor.GetComponentInParent<HandHapticController>().SendDeletePulse();
    //         }
    //     }
    //     // Otherwise...
    //     else {
    //         // If drawing is enabled, set any disabled materials to enabled.
    //         if (parentScript.IsDrawEnabled() && meshRenderer.enabled == false) {
    //             // Enabled the material
    //             meshRenderer.enabled = true;

    //             // Increase the block count to stay consistent with the state of the mesh renderer.
    //             interactor.GetComponentInParent<HandHapticController>().IncreaseBlockCount();

    //             // Send a haptic pulse top show the user a block has been added back.
    //             interactor.GetComponentInParent<HandHapticController>().SendAddPulse();

    //         }
    //     }
    // }

    public void EnterMesh() {
        Debug.Log("Mesh Entered");
    }
}
