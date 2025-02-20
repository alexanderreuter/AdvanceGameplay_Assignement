using Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battlescape
{
    public class Bullet : EventHandler.GameEventBehaviour
    {
        private Unit                m_target;

        private static GameObject   sm_bullet;

        #region Properties

        protected Vector3 Target => m_target.transform.position + Vector3.up;

        #endregion

        public override void OnUpdate()
        {
            base.OnUpdate();
            transform.position = Vector3.MoveTowards(transform.position, Target, Time.deltaTime * 40.0f);
        }

        public override bool IsDone()
        {
            return Vector3.Distance(transform.position, Target) < 0.1f;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            m_target.TakeDamage(1);
            StartCoroutine(DelayedDestroy());
        }

        IEnumerator DelayedDestroy()
        {
            // let trail renderer die out
            yield return new WaitForSeconds(1.0f);
            Destroy(gameObject);
        }

        public static void Create(Unit shooter, Unit target)
        {
            // spawn bullet
            if (sm_bullet == null)
            {
                sm_bullet = Resources.Load<GameObject>("Prefabs/Bullet");
            }

            GameObject go = Instantiate(sm_bullet, shooter.transform.position + Vector3.up, Quaternion.identity);
            go.name = "Bullet";
            Bullet bullet = go.AddComponent<Bullet>();
            bullet.m_target = target;
            EventHandler.Main.PushEvent(bullet);
        }
    }
}