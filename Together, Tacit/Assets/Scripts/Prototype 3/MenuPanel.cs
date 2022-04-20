using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    private Canvas canvas = null;
    private MenuManager menuManager = null;
    private Collider[] interactionColliders;

    private void Awake()
    {
        // Upon startup, locate the canvas and store it locally.
        canvas = GetComponent<Canvas>();
    }

    public void Setup(MenuManager menuManager)
    {
        // Set the menu manager to the parameter passed into this function.
        this.menuManager = menuManager;

        // Store all of the colliders for menu options locally.
        interactionColliders = GetComponentsInChildren<BoxCollider>();

        // Initially, have the canvas hidden from the player.
        Hide();
    }

    public void Show()
    {
        canvas.enabled = true;

        // Enable all child colliders.
        foreach (BoxCollider collider in interactionColliders) {
            collider.enabled = true;
        }
    }

    public void Hide()
    {
        canvas.enabled = false;

        // Disable all child colliders.
        foreach (BoxCollider collider in interactionColliders) {
            collider.enabled = false;
        }
    }
}
