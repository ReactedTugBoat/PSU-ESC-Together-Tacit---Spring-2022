using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public ControllerTypeManager leftControllerManager;
    public ControllerTypeManager rightControllerManager;
    public VoxelManager voxelManager;
    private MenuPanel currentPanel = null;
    private MenuPanel mainMenuPanel = null;
    private MenuPanel optionsPanel = null;
    private MenuPanel leftCalibratePanel = null;
    private MenuPanel rightCalibratePanel = null;
    private Canvas canvas = null;
    private bool menuIsOpen;

    public void Start()
    {
        // Setup the panels within the menu
        SetupPanels();
    }

    public void Update()
    {
        // When a user depresses the menu button on the left controller, toggle the menu's visibility.
        
    }

    private void SetupPanels()
    {
        // Locate and initialize each menu panel within the manager's children.
        MenuPanel[] menuPanels = GetComponentsInChildren<MenuPanel>();
        foreach (MenuPanel panel in menuPanels) {
            panel.Setup(this);
        }

        // Store the panels found.
        // The main menu will always be the first canvas in the heirarchy.
        mainMenuPanel = menuPanels[0];
        optionsPanel = menuPanels[1];
        leftCalibratePanel = menuPanels[2];
        rightCalibratePanel = menuPanels[3];

        // Save the panel locally to toggle visibility later.
        canvas = GetComponent<Canvas>();

        // To begin, set the menu to closed.
        Hide();
    }

    public void ToggleMenuOpen()
    {
        // When called, toggles whether the menus are currently visible.
        // Typically called when a user either pushes the menu button on the vr controller
        // or when the user closes the menu using the 'Resume' button.
        // Check if the menu is open prior to being called.
        if (menuIsOpen) {
            // If it is already open, change the menu visiblity to hidden.
            Hide();

            // Disable the selection rays for both controllers.
            leftControllerManager.DisableMenuRay();
            rightControllerManager.DisableMenuRay();

            // If the sculpture is hidden, set it to visible again.
            voxelManager.ShowSculpture();

        } else {
            // Otherwise, if the menu is closed, set the current panel to the main menu.
            SetCurrent(mainMenuPanel);

            // Set the menu to visible.
            Show();

            // Enable the selection rays for both controllers.
            leftControllerManager.EnableMenuRay();
            rightControllerManager.EnableMenuRay();

            // Hide the sculpture temporarily and disable any interactions with it.
            voxelManager.HideSculpture();
        }
    }

    // MENU BUTTON FUNCTIONS.

    public void RegenSculptureFromMenu()
    {
        // Toggle the menu's visiblilty.
        ToggleMenuOpen();

        // Regererate the sculpture.
        voxelManager.RegenerateSculpture();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenCalibrationMenus() {
        // When called, opens calibration menus equivalent to which controllers (left or right) are
        // currently set to be controlled as haptic gloves.
        currentPanel.Hide();

        // Once swapped to the calibration menu, enable the controls of the needed controller panels.
        // if (leftControllerManager.CurrentControllerType() == GameplayControllerType.HAPTIC_GLOVE) {
        //     leftCalibratePanel.Show();
        //     Debug.Log("Left show");
        // } else {
        //     leftCalibratePanel.Hide();
        // }

        // if (rightControllerManager.CurrentControllerType() == GameplayControllerType.HAPTIC_GLOVE) {
        //     rightCalibratePanel.Show();
        //     Debug.Log("Right show");
        // } else {
        //     rightCalibratePanel.Show();
        // }
        leftCalibratePanel.Show();
        rightCalibratePanel.Show();
    }

    // HELPER FUNCTIONS.

    public void SetCurrent(MenuPanel newPanel)
    {

        // If the current panel is not null, hide the current panel.
        if (currentPanel != null) {
            currentPanel.Hide();
        }

        // Set the new panel as the current panel and make it visible.
        currentPanel = newPanel;
        currentPanel.Show();
    }

    public void Show()
    {
        canvas.enabled = true;
        menuIsOpen = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
        menuIsOpen = false;
    }
}
