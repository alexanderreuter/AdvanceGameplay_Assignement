using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battlescape.UnitActions
{
    public class Shoot : Unit.UnitAction
    {
        private Unit                m_target;

        #region Properties

        public Quaternion TargetRotation => Quaternion.LookRotation(m_target.transform.position - m_unit.transform.position);

        #endregion

        public Shoot(Unit unit, Unit target) : base(unit)
        {
            m_target = target;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // face target
            m_unit.transform.rotation = Quaternion.Slerp(m_unit.transform.rotation, TargetRotation, Time.deltaTime);
        }

        public override bool IsDone()
        {
            return Quaternion.Angle(m_unit.transform.rotation, TargetRotation) < 4.0f;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            Bullet.Create(m_unit, m_target);
            m_unit.TriggerOverwatch();
        }
    }
}