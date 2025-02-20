using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;
using UnityEngine.SceneManagement;
using Game.General;

namespace Game.Geoscape
{
    public class UFOPopup : Popup
    {
        bool m_bStartMission = false;

        public override void OnOkay()
        {
            base.OnOkay();
            m_bStartMission = true;
        }

        public override void OnEnd()
        {            
            base.OnEnd();

            if (m_bStartMission)
            {
                Geoscape.Instance.HideGeoscape();
                SceneManager.LoadScene("Battlescape", LoadSceneMode.Single);
            }
        }
    }
}