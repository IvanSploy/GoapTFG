using UnityEngine;
using UnityEngine.UI;

namespace Panda.Examples.Shooter
{
    public class HUD : MonoBehaviour
    {
        public Unit player;

        public Text hpText;
        public Text ammoText;
        public Text targetText;

        // Use this for initialization
        void Start()
        {

        }

        // SetParent is called once per frame
        void Update()
        {
            if (player != null)
                UpdateHUD();
        }

        float targetHP;
        float targetMax;
        private void UpdateHUD()
        {
            var hpStr = $"HP:{player.health:0}/{player.startHealth:0}";
            hpText.text = hpStr;

            var ammoStr = $"ammo:{player.ammo:0}/{player.startAmmo:0}";
            ammoText.text = ammoStr;

            if (player.lastHit)
            {
                targetHP = player.lastHit.health;
                targetMax = player.lastHit.startHealth;
            }else
            {
                targetHP = 0.0f;
            }

            if( Time.time - player.lastHitTime < 3.0f)
                targetText.text = $"HP:{targetHP:0}/{targetMax:0}";
            else
                targetText.text = "";


            if ((Time.time - player.lastHitTime) > 3.0f)
                targetText.text = "";


        }
    }
}
