using Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battlescape
{
    public class Team : EventHandler.GameEventBehaviour
    {
        [SerializeField]
        public bool             m_bIsPlayerTeam = false;

        private Queue<Unit>     m_units = new Queue<Unit>();

        #region Properties

        public Battlescape Battlescape => GetComponentInParent<Battlescape>();

        public bool IsAlive => GetComponentInChildren<Unit>() != null;

        #endregion

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);

            if (bFirstTime)
            {
                m_units = new Queue<Unit>(GetComponentsInChildren<Unit>());
            }

            // grab current unit
            if (m_units.Count > 0)
            {
                Unit unit = m_units.Dequeue();
                EventHandler.Main.PushEvent(unit);
            }

            // show turn banner?
            if (bFirstTime)
            {
                Battlescape.TurnBanner.Show(name + " Turn");
            }
        }

        public override bool IsDone()
        {
            return m_units.Count == 0;
        }
    }
}