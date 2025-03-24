
using OWML.ModHelper;
using OWML.Common;
using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using NewHorizons.Utility;
using NewHorizons.Builder;
using NewHorizons.External.Modules;
using ShipAlarm;
using static UnityEngine.UI.Image;


namespace ShipAlarm.Components
{
    public class AlarmDieQuick : MonoBehaviour
    {
        private float timeAlive = 5;
        public void Start()
        {
            AudioSource AudioPlayer = this.gameObject.GetComponent<AudioSource>();
            AudioPlayer.loop = false;
            AudioPlayer.Play();
        }

        public void Update()
        {
            timeAlive = timeAlive - Time.deltaTime;
            if (timeAlive < 0)
            {
                Destroy(this.gameObject);
            }
        }

        
    }
}
