using Events;
using Game.General;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Geoscape
{
    public class Geoscape : EventHandler.GameEventBehaviour
    {
        private float               m_fUFOTime = 5.0f;
        private static Geoscape     sm_instance;

        #region Properties

        public static Geoscape Instance => sm_instance;

        #endregion

        private void OnEnable()
        {
            EventHandler.Main.PushEvent(this);
            DontDestroyOnLoad(gameObject);
            sm_instance = this;
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);

            if (!bFirstTime)
            {
                gameObject.SetActive(true);
                SceneManager.CreateScene("Empty");
                SceneManager.UnloadSceneAsync("Battlescape");
            }
        }

        public override bool IsDone()
        {
            return false;
        }

        public void HideGeoscape()
        {
            gameObject.SetActive(false);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            m_fUFOTime -= Time.deltaTime;
            if (m_fUFOTime < 0)
            {
                Popup.Create<UFOPopup>();
                m_fUFOTime = 5.0f;
            }
        }
    }
}