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

    private void Start()
    {
        // LOCATE THE MESH RENDERER FOR THIS GAMEOBJECT.
        meshRenderer = GetComponent<MeshRenderer>();

        // FIND THE GREATEST PARENT ART BLOCK.
        parentArtBlock = GameObject.Find("/Art Blocks");

        // ADD A LISTENER FOR WHEN A HAND TOUCHES THIS BLOCK.
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

        // Track the largest parent to find whether drawing is enabled.
        parentScript = parentArtBlock.GetComponent<UndoChanges>();

        // If the parent isnt instantiated, log an error.
        if (parentScript == null) {
            Debug.LogError("parent not found.");
        }

        // If drawing is enabled, set the material to disabled.
        if (parentScript.IsDrawEnabled()) {
            meshRenderer.enabled = false;
        }
    }
}
