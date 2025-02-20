using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battlescape.UnitActions
{
    public class Move : Unit.UnitAction
    {
        private Level.Node          m_goal;
        private List<Level.Node>    m_path;

        const float                 MOVE_SPEED = 2.0f;

        #region Properties

        #endregion

        public Move(Unit unit, Level.Node goal) : base(unit)
        {
            m_goal = goal;
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);

            // find path to goal
            m_path = GraphAlgorithms.FindShortestPath_AStar<Level.Node>(Level.Instance, m_unit.Node, m_goal, MoveLinkEvaluator);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // do move!
            float fStep = Time.deltaTime * MOVE_SPEED;
            while (fStep > 0.0f)
            {
                if (m_path == null || m_path.Count == 0)
                {
                    return;
                }

                Level.Node nextNode = m_path[0];
                Vector3 vToNext = nextNode.WorldPosition - m_unit.transform.position;

                if (vToNext.magnitude > fStep)
                {
                    // move towards next node
                    m_unit.transform.position += vToNext.normalized * fStep;
                    return;
                }
                else
                {
                    // pass a node... move to next in list
                    m_path.RemoveAt(0);
                    m_unit.transform.position = nextNode.WorldPosition;
                    fStep -= vToNext.magnitude;
                    m_unit.Node = nextNode;
                }

                // update rotation
                if (vToNext.magnitude > 0.0001f)
                {
                    vToNext.y = 0.0f;
                    m_unit.transform.rotation = Quaternion.Slerp(m_unit.transform.rotation, Quaternion.LookRotation(vToNext), Time.deltaTime * 3.0f);
                }
            }
        }

        public override bool IsDone()
        {
            return m_path == null || m_path.Count == 0;
        }

        public bool MoveLinkEvaluator(ILink link)
        {
            Level.Node target = link.Target as Level.Node;
            return target.Unit == null;
        }
    }
}