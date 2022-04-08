using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameplayControllerType {
    OCULUS_TOUCH,
    HAPTIC_GLOVE
}

public class ControllerTypeManager : MonoBehaviour
{
    // Serial controller manager
    public GameObject serialController;
    // Local storage for the different types of controls loaded into each hand.
    public GameObject oculusQuestController;
    public GameObject gloveHandModel;
    public GameObject menuSelectionRay;
    // Variables for current state of hand.
    [SerializeField] private GameplayControllerType currentController = GameplayControllerType.OCULUS_TOUCH;
    [SerializeField] private Vector3 initialLocalEulerAngles;

    public void Start()
    {
        // Initially disable each controller type stored within the controller.
        oculusQuestController.SetActive(false);
        gloveHandModel.SetActive(false);
        menuSelectionRay.SetActive(false);

        // Set the starting controller type to visible.
        // By default, this is set to the Oculus controller, but that can be changed in the editor.
        UpdateControllerToCurrent();

        // Store the local euler angles of the controller to calibrate any haptic glove usage in the future.
        initialLocalEulerAngles = transform.localEulerAngles;
    }

    public void EnableMenuRay() {
        menuSelectionRay.SetActive(true);
    }

    public void DisableMenuRay() {
        menuSelectionRay.SetActive(false);
    }

    public void SetControllerToOculus() {
        currentController = GameplayControllerType.OCULUS_TOUCH;
        UpdateControllerToCurrent();
    }

    public void SetControllerToGlove() {
        currentController = GameplayControllerType.HAPTIC_GLOVE;
        UpdateControllerToCurrent();
    }

    public void CalibrateHandModelRotation() {
        // When called, updates the local rotation of the hand model to match that of the user's current hand.
    }

    public void UpdateControllerToCurrent() {
        // Set the game up to use the currently selected controller type.
        if (currentController == GameplayControllerType.OCULUS_TOUCH) {
            oculusQuestController.SetActive(true);
            gloveHandModel.SetActive(false);
            // Disable the serial controller for the given hand.
            serialController.SetActive(false);
        } else {
            oculusQuestController.SetActive(false);
            gloveHandModel.SetActive(true);
            // Enable the serial controller for the given hand.
            serialController.SetActive(true);
        }
    }

    public GameplayControllerType CurrentControllerType() {
        // Returns the current controller type set to be enabled locally.
        return currentController;
    }
}
