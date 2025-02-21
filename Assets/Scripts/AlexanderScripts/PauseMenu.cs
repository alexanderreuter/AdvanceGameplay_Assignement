using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;
using UnityEngine.SceneManagement;

namespace Game.General
{
    public class PauseMenu : Popup
    {
        public void OnResume()
        {
            // Trigger event completion - Used OnOkay since the logic already was setup in Popup
            OnOkay();
        }

        public void OnMainMenu()
        {
            Time.timeScale = 1f;
            EventHandler.Main.EventStack.Clear();
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        public void OnOptions()
        {
            Create<OptionsMenu>();
        }
        
        public void OnQuit()
        {
            // Quit
            Application.Quit();
        }

        public static void Show()
        {
            // Instantiate and add to EventHandler stack
            Create<PauseMenu>();
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);
            // Pause game
            Time.timeScale = 0.0f;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            // Unpause game
            Time.timeScale = 1f;
        }
    }
    
}
