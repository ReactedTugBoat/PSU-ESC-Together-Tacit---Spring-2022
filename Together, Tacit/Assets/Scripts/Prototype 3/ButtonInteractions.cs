using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractions : MonoBehaviour
{
    private Button foundButton = null;
    public int layerMaskValue = 5;

    public void TryButton()
    {
        // When called, attempts to start a click event for a button, if found by the raycaster.
        if (IsLineRendHittingButton()) {
            Debug.Log("Click evoked");
            foundButton.onClick.Invoke();
        }
    }


    public bool IsLineRendHittingButton()
    {
        Ray ray;
        bool isHittingButton = false;
        RaycastHit hit;

        // Cast a new ray from the ray interactor, checking to see if it is hitting a button.
        ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out hit, layerMaskValue)) {
            // If a button is hit, store it locally and set 'is hitting button' to true.
            foundButton = hit.collider.gameObject.GetComponent<Button>();
            isHittingButton = true;
        } else {
            // Otherwise, set 'is hitting button' to false.
            isHittingButton = false;
        }

        return isHittingButton;
    }
}
