using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;


namespace Game.General
{
    public class PauseMenu : Popup
    {
        private void Start()
        {
            enabled = false;
            enabled = true;
        }

        private void OnEnable()
        {
            EventHandler.Main.PushEvent(this);
        }

        public void OnResume()
        {
            base.m_bDone = true;
            base.m_group.interactable = false;
        }
    }
    
}
