using Events;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battlescape
{
    public class Battlescape : EventHandler.GameEventBehaviour
    {
        //private Node[,]                 m_nodes;
        private Queue<Team>             m_teams = new Queue<Team>();
        private static Battlescape      sm_instance;

        const int                       SIZE = 8;

        #region Properties

        public TurnBanner TurnBanner => GetComponentInChildren<TurnBanner>(true);

        public static Battlescape Instance => sm_instance;

        #endregion

        private void OnEnable()
        {
            EventHandler.Main.PushEvent(this);
            sm_instance = this;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                Time.timeScale = 4.0f;
            }
            else if (Input.GetKeyUp(KeyCode.X))
            {
                Time.timeScale = 1.0f;
            }
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);

            if (bFirstTime)
            {
                m_teams = new Queue<Team>(GetComponentsInChildren<Team>());

                foreach (Unit unit in GetComponentsInChildren<Unit>())
                {
                    unit.OnStartBattle();
                }
            }

            // grab next team
            if (!IsDone())
            {
                Team currentTeam = m_teams.Dequeue();
                EventHandler.Main.PushEvent(currentTeam);

                // is team alive?
                if (currentTeam.IsAlive)
                {
                    m_teams.Enqueue(currentTeam);
                }
            }
        }

        public override bool IsDone()
        {
            return m_teams.Count == 1;
        }

        public override void OnEnd()
        {
            base.OnEnd();

            if (m_teams.Count > 0)
            {
                Team winner = m_teams.Peek();
                TurnBanner.Show("Winner: " + winner.name);
            }

            sm_instance = null;
        }
    }
}