using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameplayControllerType {
    OCULUS_TOUCH,
    HAPTIC_GLOVE
}

public class ControllerTypeManager : MonoBehaviour
{
    // Local storage for the different types of controls loaded into each hand.
    public GameObject oculusQuestController;
    public GameObject gloveHandModel;
    public GameObject menuSelectionRay;
    // Variables for current state of hand.
    [SerializeField] private GameplayControllerType currentController = GameplayControllerType.OCULUS_TOUCH;

    public void Start()
    {
        // Initially disable each controller type stored within the controller.
        oculusQuestController.SetActive(false);
        gloveHandModel.SetActive(false);
        menuSelectionRay.SetActive(false);

        // Set the starting controller type to visible.
        // By default, this is set to the Oculus controller, but that can be changed in the editor.
        if (currentController == GameplayControllerType.OCULUS_TOUCH) {
            oculusQuestController.SetActive(true);
        } else {
            gloveHandModel.SetActive(true);
        }
    }

    public void EnableMenuRay() {
        menuSelectionRay.SetActive(true);
    }

    public void DisableMenuRay() {
        menuSelectionRay.SetActive(false);
    }

    public void SetControllerToOculus() {
        currentController = GameplayControllerType.OCULUS_TOUCH;
    }

    public void SetControllerToGlove() {
        currentController = GameplayControllerType.HAPTIC_GLOVE;
    }

    public GameplayControllerType CurrentControllerType() {
        // Returns the current controller type set to be enabled locally.
        return currentController;
    }
}
