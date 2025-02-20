using Events;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.General
{
    public class MainMenu : EventHandler.GameEventBehaviour
    {
        private void OnEnable()
        {
            EventHandler.Main.PushEvent(this);
        }

        public override bool IsDone()
        {
            return false;
        }

        public void OnNewGame()
        {
            EventHandler.Main.RemoveEvent(this);
            SceneManager.LoadScene("Geoscape", LoadSceneMode.Single);
        }

        public void OnOptions()
        {
            Popup.Create<OptionsMenu>();
        }

        public void OnQuit()
        {
        }
    }
}