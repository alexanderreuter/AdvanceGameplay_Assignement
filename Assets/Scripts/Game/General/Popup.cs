using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;

namespace Game.General
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Popup : EventHandler.GameEventBehaviour
    {
        private bool            m_bDone = false;
        private CanvasGroup     m_group = null;

        private void OnEnable()
        {
            m_group = GetComponent<CanvasGroup>();
            m_group.alpha = 0.0f;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // update alpha
            // changed delta time to unscaled so the fade in still triggers while pause
            m_group.alpha = Mathf.MoveTowards(m_group.alpha, m_bDone ? 0.0f : 1.0f, Time.unscaledDeltaTime);
        }

        public virtual void OnOkay()
        {
            m_bDone = true;
            m_group.interactable = false;

            // Add coroutine to not end the event until the fade out is complete (makes sure it dosen't break game logic)
            StartCoroutine(WaitForFadeOut());
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

        // Wait for fade out until ending the event
        private IEnumerator WaitForFadeOut()
        {
            while (m_group.alpha > 0.001f) 
            {
                yield return null; 
            }

            OnEnd(); 
        }
    }
}