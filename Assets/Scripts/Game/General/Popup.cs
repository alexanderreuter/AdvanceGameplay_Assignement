using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;

namespace Game.General
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Popup : EventHandler.GameEventBehaviour
    {
        protected bool            m_bDone = false;
        protected CanvasGroup     m_group = null;

        private void OnEnable()
        {
            m_group = GetComponent<CanvasGroup>();
            m_group.alpha = 0.0f;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // update alpha
            m_group.alpha = Mathf.MoveTowards(m_group.alpha, m_bDone ? 0.0f : 1.0f, Time.deltaTime);
        }

        public virtual void OnOkay()
        {
            m_bDone = true;
            m_group.interactable = false;
        }

        public virtual void OnCancel()
        {
            m_bDone = true;
            m_group.interactable = false;
        }

        public override bool IsDone()
        {
            return m_bDone && m_group.alpha < 0.001f;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            Destroy(gameObject);
        }

        public static void Create<T>() where T : EventHandler.GameEventBehaviour
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + typeof(T).Name);
            GameObject go = Instantiate(prefab);
            T om = go.GetComponent<T>();
            EventHandler.Main.PushEvent(om);
        }
    }
}