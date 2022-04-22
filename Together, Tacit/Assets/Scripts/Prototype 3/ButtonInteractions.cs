using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonInteractions : MonoBehaviour
{
    private Collider foundCollider = null;
    public int layerMaskValue = 5;

    public void TryButton()
    {
        // When called, attempts to interact with a given menu element if found by the raycaster.
        if (IsLineRendHittingInteractable()) {
            if (foundCollider != null) {
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                foundCollider.gameObject.GetComponent<IPointerClickHandler>().OnPointerClick(pointerEventData);
            }
        }
    }


    public bool IsLineRendHittingInteractable()
    {
        Ray ray;
        bool isHittingInteractable = false;
        RaycastHit hit;

        // Cast a new ray from the ray interactor, checking to see if it is hitting a button.
        ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out hit, layerMaskValue)) {
            // If an interactable is hit, store it's associated collider
            foundCollider = hit.collider;
            if (foundCollider != null) {
                isHittingInteractable = true;
            }

            // If the dropdown is not of a recognized type, isHittingInteractable is left false.

        } else {
            // Otherwise, set 'is hitting button' to false.
            foundCollider = null;
            isHittingInteractable = false;
        }

        return isHittingInteractable;
    }
}
