using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public MenuPanel currentPanel = null;
    private MenuPanel mainMenuPanel = null;
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

        // Store the first panel found to be the default options when opening the menu.
        // This will always be the first canvas in the heirarchy.
        mainMenuPanel = menuPanels[0];

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

        } else {
            // Otherwise, if the menu is closed, set the current panel to the main menu.
            SetCurrent(mainMenuPanel);

            // Set the menu to visible.
            Show();
        }
    }

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
