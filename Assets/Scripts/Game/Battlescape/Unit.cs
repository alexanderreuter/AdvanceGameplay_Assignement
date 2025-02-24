using Events;
using Game.Battlescape.UnitActions;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;

namespace Game.Battlescape
{
    public class Unit : EventHandler.GameEventBehaviour
    {
        public abstract class UnitAction : EventHandler.GameEvent
        {
            protected Unit m_unit;

            public UnitAction(Unit unit)
            {
                m_unit = unit;
            }
        }

        [SerializeField, Range(1, 10)]
        public int                  m_iHP = 2;

        private float               m_fTime = 1.0f;
        private Level.Node          m_node;
        private Unit                m_target;

        public static List<Unit>    AllUnits = new List<Unit>();

        public const float          SHOOT_RANGE = 8.1f;
        public const int            VISION_RANGE = 8;
        public const int            MOVEMENT_RANGE = 5;

        #region Properties

        public Level.Node Node
        {
            get => m_node;
            set
            {
                if (m_node != value)
                {
                    if (m_node != null)
                    {
                        m_node.Unit = null;
                    }

                    m_node = value;

                    if (m_node != null)
                    {
                        m_node.Unit = this;
                        Level.Instance?.UpdateSightTexture();
                        TriggerOverwatch();
                    }
                }
            }
        }

        public Vector3Int HeadVoxel => new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y + 1.5f), Mathf.RoundToInt(transform.position.z));

        public Team Team => GetComponentInParent<Team>();

        public List<Unit> Enemies => AllUnits.FindAll(u => IsEnemy(u));
        
        public List<Unit> EnemiesInRange => Enemies.FindAll(e => Vector3.Distance(transform.position, e.transform.position) <= SHOOT_RANGE);

        public HashSet<Level.Node> OverwatchNodes { get; set; }

        #endregion

        private void OnEnable()
        {
            AllUnits.Add(this);
        }

        private void OnDisable()
        {
            Node = null;
            AllUnits.Remove(this);
        }

        public void OnStartBattle()
        {
            Node = GraphAlgorithms.GetClosestNode<Level.Node>(Level.Instance, transform.position);
            transform.position = Node.WorldPosition;
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);                    

            m_fTime = 0.5f;

            if (bFirstTime)
            {
                // clear overwatch nodes
                OverwatchNodes = null;
                
                if (Team.m_bIsPlayerTeam)
                {
                    EventHandler.Main.PushEvent(new UnitActions.PlayerInput(this));
                }
                else
                {
                    DoAIMove();
                }
            }
        }

        protected void DoAIMove()
        {
            if (EnemiesInRange.Count > 0)
            {
                // Shoot enemy
                m_target = EnemiesInRange[Random.Range(0, EnemiesInRange.Count)];
                EventHandler.Main.PushEvent(new UnitActions.Shoot(this, m_target));
            }
            else if (Enemies.Count > 0)
            {
                // find closest enemy
                float fBestDistance = float.MaxValue;
                m_target = null;
                foreach (Unit enemy in Enemies)
                {
                    float fDistance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (fDistance < fBestDistance)
                    {
                        fBestDistance = fDistance;
                        m_target = enemy;
                    }
                }

                // get reachable (unoccupied) nodes 
                HashSet<Level.Node> reachableNodes = GraphAlgorithms.GetNodesInRange(Node, MOVEMENT_RANGE);
                reachableNodes.RemoveWhere(n => n.Unit != null);

                // remove nodes too close to enemies
                foreach (Unit enemy in Enemies)
                {
                    HashSet<Level.Node> enemyNodes = GraphAlgorithms.GetNodesInRange(enemy.Node, MOVEMENT_RANGE / 2);
                    reachableNodes.RemoveWhere(n => enemyNodes.Contains(n));
                }

                // find closest node to enemy
                fBestDistance = float.MaxValue;
                Level.Node bestNode = null;
                foreach (Level.Node node in reachableNodes)
                {
                    float fDistance = Vector3.Distance(node.WorldPosition, m_target.transform.position);
                    if (fDistance < fBestDistance)
                    {
                        fBestDistance = fDistance;
                        bestNode = node;
                    }
                }

                if (bestNode != null)
                {
                    EventHandler.Main.PushEvent(new UnitActions.Move(this, bestNode));
                }
            }
        }

        public void TakeDamage(int iDmg)
        {
            m_iHP -= iDmg;
            if (m_iHP <= 0)
            {
                EventHandler.Main.PushEvent(new UnitActions.Death(this));
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            m_fTime -= Time.deltaTime;
        }

        public override bool IsDone()
        {
            return m_fTime < 0.0f;
        }

        public bool IsEnemy(Unit unit)
        {
            return unit.Team != Team;
        }

        public void TriggerOverwatch()
        {
            // trigger enemy overwatch
            foreach (Unit enemy in Enemies)
            {
                if (enemy.OverwatchNodes != null && enemy.OverwatchNodes.Contains(Node))
                {
                    EventHandler.Main.PushEvent(new Shoot(enemy, this));
                    enemy.OverwatchNodes = null;
                }
            }
        }
    }
}