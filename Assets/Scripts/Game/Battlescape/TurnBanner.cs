using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;
using UnityEngine.UI;

namespace Game.Battlescape
{
    public class TurnBanner : EventHandler.GameEventBehaviour
    {
        CanvasGroup         m_group;
        bool                m_bDone;
        float               m_fTime;

        private void OnEnable()
        {
            m_group = GetComponent<CanvasGroup>();
            m_group.alpha = 0.0f;
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);
            m_bDone = false;
            m_fTime = 0.0f;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            m_group.alpha = Mathf.MoveTowards(m_group.alpha, m_bDone ? 0.0f : 1.0f, Time.deltaTime * 2.0f);
            m_fTime += Time.deltaTime;
            if (m_fTime > 2.0f)
            {
                m_bDone = true;
            }
        }

        public override bool IsDone()
        {
            return m_bDone && m_group.alpha < 0.001f;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            gameObject.SetActive(false);
        }


        public void Show(string turn)
        {
            transform.Find("Banner/Text").GetComponent<Text>().text = turn;
            gameObject.SetActive(true);
            EventHandler.Main.PushEvent(this);
        }
    }
}