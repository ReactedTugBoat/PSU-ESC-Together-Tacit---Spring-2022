using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    private Canvas canvas = null;
    private MenuManager menuManager = null;

    private void Awake()
    {
        // Upon startup, locate the canvas and store it locally.
        canvas = GetComponent<Canvas>();
    }

    public void Setup(MenuManager menuManager)
    {
        // Set the menu manager to the parameter passed into this function.
        this.menuManager = menuManager;

        // Initially, have the canvas hidden from the player.
        Hide();
    }

    public void Show()
    {
        canvas.enabled = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
    }


}
