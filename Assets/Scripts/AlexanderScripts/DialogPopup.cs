using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.General
{
    public class DialogPopup : Popup
    {
        private float timeToDisplay;
        [SerializeField] private Text dialogText;
        
        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);

            timeToDisplay = 3f;
            dialogText.text = GetDialog();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            timeToDisplay -= Time.deltaTime;

            if (timeToDisplay <= 0)
            {
                OnOkay();
            }
        }
        
        public static void Show()
        {
            Create<DialogPopup>();
        }

        private String GetDialog()
        {
            String optionOne = "Pibble";
            String optionTwo = "Wow, epic move!";
            String optionThree = "I am Groot (very trustworthy)";

            int option = UnityEngine.Random.Range(0, 3);
            switch (option)
            {
                case 0:
                    return optionOne;
                case 1: 
                    return optionTwo;
                case 2:
                    return optionThree;
            }

            return optionOne;
        }
    }
}
