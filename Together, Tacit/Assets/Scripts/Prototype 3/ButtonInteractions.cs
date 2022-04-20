using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractions : MonoBehaviour
{
    private Button foundButton = null;
    private Dropdown foundDropdown = null;
    public int layerMaskValue = 5;

    public void TryButton()
    {
        // When called, attempts to start a click event for a button or dropdown, if found by the raycaster.
        if (IsLineRendHittingInteractable()) {
            if (foundButton != null) {
                foundButton.onClick.Invoke();
            }
            if (foundDropdown != null) {
                foundDropdown.Select();
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
            // If an interactable is hit, check if it is of a type recognized currently.
            // At the moment, this script recognizes dropdown menus and buttons.
            // Check if the interactable is a button.
            foundButton = hit.collider.gameObject.GetComponent<Button>();
            if (foundButton != null) {
                isHittingInteractable = true;
                Debug.Log("Hit button!");
            }
            // Check if the interactable is a dropdown.
            foundDropdown = hit.collider.gameObject.GetComponent<Dropdown>();
            if (foundDropdown != null) {
                isHittingInteractable = true;
                Debug.Log("Hit dropdown!");
            }

            // If the dropdown is not of a recognized type, isHittingInteractable is left false.

        } else {
            // Otherwise, set 'is hitting button' to false.
            foundButton = null;
            foundDropdown = null;
            isHittingInteractable = false;
            Debug.Log("No inter found");
        }

        return isHittingInteractable;
    }
}
