using System;
using System.Collections;
using System.Collections.Generic;
using Game.General;
using UnityEngine;
using EventHandler = Events.EventHandler;

public class PauseManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (EventHandler.Main.CurrentEvent is PauseMenu)
            {
                // Unpause
                EventHandler.Main.RemoveEvent(EventHandler.Main.CurrentEvent);
            }
            else
            {
                // Bring up pause menu 
                PauseMenu.Show();   
            }
        }
    }
}
