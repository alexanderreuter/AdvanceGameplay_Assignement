using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battlescape.UnitActions
{
    public class Death : Unit.UnitAction
    {
        bool m_bDone = false;

        public Death(Unit unit) : base(unit)
        {
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);
            m_unit.StartCoroutine(DeathLogic());
        }

        IEnumerator DeathLogic()
        {
            // fall over
            Vector3 vRot = m_unit.transform.localEulerAngles;
            for (float f = 0.0f; f < 1.0f; f += Time.deltaTime * 3.0f)
            {
                vRot.x = f * 90.0f;
                m_unit.transform.localEulerAngles = vRot;
                yield return null;
            }

            // sink down
            for (float f = 0.0f; f < 1.0f; f += Time.deltaTime)
            {
                m_unit.transform.position += Vector3.down * Time.deltaTime * 0.5f;
                yield return null;
            }

            // done!
            m_bDone = true;
        }

        public override bool IsDone()
        {
            return m_bDone;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            Object.Destroy(m_unit.gameObject);
        }
    }
}