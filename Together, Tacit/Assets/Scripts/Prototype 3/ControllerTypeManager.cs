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
    public Object oculusQuestController;
    public Object gloveHandModel;
    public GameObject menuSelectionRay;
    // Variables for current state of hand.
    [SerializeField] private GameplayControllerType currentController;

    public void Start()
    {
        // Initialize the 
    }
}
