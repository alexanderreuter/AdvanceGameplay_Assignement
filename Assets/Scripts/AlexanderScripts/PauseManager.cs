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
            if (EventHandler.Main.CurrentEvent is PauseMenu pauseMenu)
            {
                // Unpause
                pauseMenu.OnResume();
            }
            else
            {
                // Bring up pause menu 
                PauseMenu.Show();   
            }
        }
    }
}
