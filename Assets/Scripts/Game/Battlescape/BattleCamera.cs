using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;

namespace Game.Battlescape
{
    [RequireComponent(typeof(Camera))]
    public class BattleCamera : MonoBehaviour
    {
        static BattleCamera     sm_instance;

        #region Properties

        public Camera Camera => GetComponent<Camera>();

        public static BattleCamera Instance => sm_instance;

        #endregion

        public void OnEnable()
        {
            sm_instance = this;
        }

        private void OnDisable()
        {
            sm_instance = (sm_instance == this ? null : sm_instance);
        }

        private void Update()
        {
            // focus camera on current unit
            Unit unit = EventHandler.Main.EventStack.Find(e => e is Unit) as Unit;
            if (unit != null)
            {
                Vector3 vTarget = unit.transform.position - transform.forward * 20.0f;
                transform.position += (vTarget - transform.position) * Time.deltaTime;
            }
        }
    }
}