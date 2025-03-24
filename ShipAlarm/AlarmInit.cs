
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
    public class AlarmInit : MonoBehaviour
    {
        public AudioClip AudioJungle;
        public bool InitializedAlarm = false;
        public float LaterInitialize = 0;
        public bool LateBool = false;
        public ReferenceFrameGUI ShipReferenceFrame;
        public AudioSource ShipAlarmAudio;
        public AudioSource ShipAlarmSecondaryAudio;
        public AudioSource ShipAlarmStall;
        public List<AudioClip> AudioAlti = new List<AudioClip>();
        public List<AudioClip> AudioSecondary = new List<AudioClip>();

        public int LastAltitude = 5000;
        public float dist = 0;
        public OWRigidbody SunRB;

        public ShipDamageDisplayV2 ShipDamages;
        public ShipComponent[] ShipDamaged;
        public Autopilot Autopilot;
        public float SunDist = 10000;

        public bool MasterAlarm = false;

        public List<int> ClipToRead = new List<int>();
        public List<bool> ConditionsForAlarm = new List<bool>();

        public float TimeSinceSecondaryAlarm;
        public Vector3 PreviousRelativeVelocity;

        private bool AllowStallWarning = false;
        public float overspeedSensibility;
        public float altitudeWarning;
        public float sunDangerDist;
        public string prefix = "MB";
        public string prefixVerbose = "M";

        public int readOutLoud = 0;
        public GameObject woody;

        public void Start()
        {
            ShipAlarm.ShipAlarmInstance.LogForStupids("Ah");
            ShipAlarm.ShipAlarmInstance.LogForStupids("Start print I guess");



        }

        public void Update()
        {
            if (InitializedAlarm == false)
            {
                if (SearchUtilities.Find("Ship_Body/Module_Cockpit") == null)
                {
                    return;
                }

                GameObject Ship_Cockpit = SearchUtilities.Find("Ship_Body/Module_Cockpit");
                GameObject PrefabVerboseAlarm = SearchUtilities.Find("Ship_Body/Audio_Ship/ShipInteriorAudio/AlarmAudio");
                ShipReferenceFrame = SearchUtilities.Find("ShipScreenSpaceUI/CockpitLockOnCanvas").GetComponent<ReferenceFrameGUI>();
                ShipDamaged = SearchUtilities.Find("Ship_Body/Module_Cockpit/Systems_Cockpit/ShipCockpitUI/DamageScreen/HUD_ShipDamageDisplay").GetComponent<ShipDamageDisplayV2>()._shipComponents;

                //GameObject Source = SearchUtilities.Find("AlarmBank_Body/Sector/BAltitudeAlarm");
                GameObject MainMasterLight = SearchUtilities.Find("Ship_Body/Module_Cabin/Lights_Cabin/PointLight_HEA_MasterAlarm");


                LaterInitialize = 0;
                LateBool = false;

                GameObject VerboseAlarm = Instantiate(PrefabVerboseAlarm, Ship_Cockpit.transform);
                PlaceCorrectlyLocal(VerboseAlarm.transform, new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                VerboseAlarm.name = "VerboseAlarm";

                ShipAlarmAudio = VerboseAlarm.GetComponent<AudioSource>();

                //ShipAlarmAudio.clip = Source.GetComponent<AudioSource>().clip;


                GameObject SecondaryAlarm = Instantiate(PrefabVerboseAlarm, Ship_Cockpit.transform);
                PlaceCorrectlyLocal(SecondaryAlarm.transform, new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                SecondaryAlarm.name = "SecondaryAlarm";
                ShipAlarmSecondaryAudio = SecondaryAlarm.GetComponent<AudioSource>();
                //ShipAlarmSecondaryAudio.clip = Source.GetComponent<AudioSource>().clip;
                ShipAlarmSecondaryAudio.loop = false;


                GameObject StallingAlarm = Instantiate(VerboseAlarm, Ship_Cockpit.transform);
                PlaceCorrectlyLocal(StallingAlarm.transform, new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                StallingAlarm.name = "StallingAlarm";
                ShipAlarmStall = StallingAlarm.GetComponent<AudioSource>();
                ShipAlarmStall.loop = true;
                ShipAlarmStall.Play();
                ShipAlarmStall.Pause();

                //woody = SearchUtilities.Find("AlarmBank_Body/Sector/SPXWoodLogs");
                //woody = Instantiate(woody, Ship_Cockpit.transform);
                //PlaceCorrectlyLocal(woody.transform, new Vector3(-2, -2.7f, 1.7f), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                //woody.name = "WOODY";
                //ShipAlarmAudio.Play();



                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/MD/eng1fire.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/MD/eng2fire.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/MD/evacuate.mp3"));

                ConditionsForAlarm = new List<bool> { false, false, false, false, false };
                //ConditionsForAlarm.Add(false);  //0 Left Engine
                //ConditionsForAlarm.Add(false);  //1 Right Engine
                //ConditionsForAlarm.Add(false);  //2 Boilers
                //ConditionsForAlarm.Add(false);  //3 Copilot
                //ConditionsForAlarm.Add(false);  //4 Electrical


                
                if (SearchUtilities.Find("Sun_Body") != null)
                {
                    GameObject SunBody = SearchUtilities.Find("Sun_Body");
                    if (SunBody.activeSelf == true)
                    {
                        SunRB = SunBody.GetComponent<OWRigidbody>();
                    }  
                }

                Autopilot = SearchUtilities.Find("Ship_Body").GetComponent<Autopilot>();


                //Ship_Cockpit.GetComponent<SimplePlayerTemperature>()._currentFuel = 0;


                //GameObject NewIntera = Instantiate(GrappingCol, PlayerBody.transform);
                //PlaceCorrectlyLocal(NewIntera.transform, new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
                //NewIntera.AddComponent<StopRotation>();


                InitializedAlarm = true;

                ShipAlarm.ShipAlarmInstance.ConfigureAlarmFirstStep();

                ShipAlarm.ShipAlarmInstance.LogForStupids("Alarm Initialized");
            }

            else
            {
                if (ShipDamaged[3]._damaged == false)
                {

                    if (SunRB != null)
                    {
                        SunDist = Vector3.Distance(SunRB._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (SunDist < sunDangerDist)
                        {
                            PlayPriority(13);
                            ShipAlarm.ShipAlarmInstance.LogForStupids("I'm stupid and I think that it is " + (SunDist < sunDangerDist) + " that " + SunDist + " is < than 4000");
                        }
                        else
                        {
                            MasterAlarm = false;
                        }
                    }

                    

                    if (ShipReferenceFrame._currentReferenceFrame != null)
                    {
                        
                        //AlarmDieQuick


                        //if (ShipReferenceFrame._orientedRelativeVelocity.z > 50)
                        //{
                        //    if (ShipAlarmAudio.isPlaying == false)
                        //    {
                        //        ShipAlarmAudio.Play();
                        //    }
                        //}
                        dist = Vector3.Distance(ShipReferenceFrame._currentReferenceFrame._attachedOWRigidbody._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (ShipReferenceFrame._orientedRelativeVelocity.z > 1 && dist > 1)
                        {
                            float OverSpeedAlarm = ShipReferenceFrame._orientedRelativeVelocity.z / dist;
                            if (OverSpeedAlarm >= overspeedSensibility && ShipReferenceFrame._orientedRelativeVelocity.z > 200 && dist < altitudeWarning)
                            {
                                PlayPriority(13);
                            }
                            else if (OverSpeedAlarm >= overspeedSensibility && ShipReferenceFrame._orientedRelativeVelocity.z > 200)
                            {
                                PlayPriority(14);
                            }
                        }

                        if (ShipReferenceFrame._orientedRelativeVelocity.z < 15 && ShipReferenceFrame._orientedRelativeVelocity.z > 0.5f && dist < 1000 && PreviousRelativeVelocity.z < ShipReferenceFrame._orientedRelativeVelocity.z && AllowStallWarning)
                        {
                            if (ShipAlarmStall.clip != AudioAlti[16])
                            {
                                ShipAlarmStall.clip = AudioAlti[16];
                                ShipAlarmStall.Play(); 
                            }
                            else if (ShipAlarmStall.isPlaying == false)
                            {
                                ShipAlarmStall.clip = AudioAlti[16];
                                ShipAlarmStall.UnPause();
                            }
                        }
                        else
                        {
                            ShipAlarmStall.Pause();
                        }


                        if (ShipAlarmAudio.loop == false && MasterAlarm == false && readOutLoud != 0)
                        {
                            CheckAltitude();
                        }


                        PreviousRelativeVelocity = ShipReferenceFrame._orientedRelativeVelocity;
                    }
                }

                if (MasterAlarm == false)
                {
                    AlarmStop();
                }
                 

                
                //if (ShipDamaged[0]._damaged)
                //{
                //    if (ConditionsForAlarm[3] == false)
                //    {
                //        ConditionsForAlarm[3] = true;
                //        ClipToRead.Add(3);
                //    }
                //}
                //else
                //{
                //    ConditionsForAlarm[3] = false;
                //}



                CheckIfDamaged(0, 12);  //Eng1
                CheckIfDamaged(1, 13);  //Eng2
                CheckIfDamaged(2, 5);  //Boilers


                PlayOneByOne();

                

                MasterAlarm = false;
            }
        }

        private void PlayPriority(int CueToPlay)
        {
            if (MasterAlarm)
            { return; }
            MasterAlarm = true;

            if ((CueToPlay == 13 || CueToPlay == 14) && Autopilot.enabled)
            {
                CueToPlay = 15;
            }

            if (ShipAlarmAudio.clip != AudioAlti[CueToPlay] || ShipAlarmAudio.isPlaying == false)
            {
                if (ShipAlarmAudio.isPlaying == true)
                {
                    ShipAlarmAudio.loop = false;
                    return;
                }
                ShipAlarmAudio.clip = AudioAlti[CueToPlay];
                ShipAlarmAudio.loop = true;
                if (ShipAlarmAudio.isPlaying == false)
                {
                    ShipAlarmAudio.Play();
                }
            }
        }


        private void AlarmStop()
        {
            if (MasterAlarm)
            { return; }
            ShipAlarmAudio.loop = false;
        }

        private bool PlayAlarm(int CueToPlay)
        {
            bool WasPlayed = false;
            if (ShipAlarmAudio.isPlaying == false)
            {
                ShipAlarmAudio.clip = AudioAlti[CueToPlay];
                ShipAlarmAudio.loop = false;
                ShipAlarmAudio.Play();
                WasPlayed = true;
            }
            return WasPlayed;
        }

        private void CheckAltitude ()
        {
            if (readOutLoud == 1)
            {
                dist = Vector3.Distance(ShipReferenceFrame._currentReferenceFrame._attachedOWRigidbody._lastPosition, ShipAlarmAudio.gameObject.transform.position);
            }
            else if (readOutLoud == 2)
            {
                
                dist = Mathf.Abs(ShipReferenceFrame._orientedRelativeVelocity.z);
            }
            else if (readOutLoud == 3)
            {
                RaycastHit hit;
                float dist1 = 0;
                float dist2 = 0;
                if (Physics.Raycast(woody.transform.position, -Vector3.up, out hit, 3000.0f))
                {
                    dist = hit.distance;
                }
                else
                {
                    dist = LastAltitude;
                }

            }

            int[] Altitudes = { 10, 20, 30, 40, 50, 100, 200, 300, 400, 500, 1000, 1500, 2500, 5000 };
            //int[] Altitudes = {3000, 2500, 1500, 1000, 500, 400, 300, 200, 100, 50, 40, 30, 20, 10 };
            int i = 0;
            foreach (int Altitude in Altitudes)
            {
                if (i == 13)
                {
                    LastAltitude = Altitude;
                    return;
                }
                if (dist < Altitude)
                {
                    if ((LastAltitude > Altitude && (readOutLoud == 1 || readOutLoud == 3)) || (LastAltitude < Altitude && readOutLoud == 2))
                    {
                        if(readOutLoud == 2 && i != 0)
                        { i = i - 1; }
                        PlayAlarm(i);
                    }
                    LastAltitude = Altitude;
                    return;
                }
                i = i + 1;
            }
        }

        private void PlayOneByOne()
        {
            TimeSinceSecondaryAlarm = TimeSinceSecondaryAlarm + Time.deltaTime;

            if (TimeSinceSecondaryAlarm > 5)
            {
                if (ClipToRead.Count != 0)
                {
                    ShipAlarmSecondaryAudio.clip = AudioSecondary[ClipToRead[0]];
                    ShipAlarmSecondaryAudio.Play();
                    ClipToRead.RemoveAt(0);
                    TimeSinceSecondaryAlarm = 0;
                }
            }
        }

        private void CheckIfDamaged(int ComponentID, int ComponentListedID)
        {
            if (ShipDamaged[ComponentListedID]._damaged)
            {
                if (ConditionsForAlarm[ComponentID] == false)
                {
                    ConditionsForAlarm[ComponentID] = true;
                    ClipToRead.Add(ComponentID);
                    
                }
            }
            else
            {
                ConditionsForAlarm[ComponentID] = false;
            }
        }

       

        public void PlaceCorrectlyLocal(Transform ToChange, Vector3 pos, Vector3 rot, Vector3 scal)
        {
            ToChange.localPosition = pos;
            ToChange.localEulerAngles = rot;
            ToChange.localScale = scal;
        }


        public void ConfigureInTwoStep(float SoundVolume, bool stallWarning, float overspeed, float altitudeWarn, float sunWarn, string AlarmsVoice, string ReadNumbers)
        {
            if (InitializedAlarm)
            {
                ShipAlarmStall.volume = SoundVolume / 500;
                ShipAlarmSecondaryAudio.volume = SoundVolume / 100;
                ShipAlarmAudio.volume = SoundVolume / 100;

                AllowStallWarning = stallWarning;

                overspeedSensibility = (100 + overspeed) * 0.001f;

                altitudeWarning = (100 + (altitudeWarn * 4)) * 10;

                sunDangerDist = (300 + (sunWarn * 3)) * 10;

                prefixVerbose = AlarmsVoice;

                if (AlarmsVoice == "MD-80")
                {
                    prefix = "MD";
                }
                else
                {
                    prefix = "BOEING";
                }

                if (ReadNumbers == "Altitude")
                {
                    readOutLoud = 1;
                }
                else if (ReadNumbers == "Speed")
                {
                    readOutLoud = 2;
                }
                else if (ReadNumbers == "Real Altitude")
                {
                    readOutLoud = 3;
                }
                else
                {
                    readOutLoud = 0;
                }

                AudioAlti = new List<AudioClip>();
                AudioSecondary = new List<AudioClip>();

                AudioJungle = ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/MD/StallWarning.mp3");
                
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/10.mp3")); //0
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/20.mp3"));
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/30.mp3"));
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/40.mp3"));
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/50.mp3"));

                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/100.mp3")); //5
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/200.mp3"));
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/300.mp3"));
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/400.mp3"));
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/500.mp3"));

                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/1000.mp3")); //10
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/1500.mp3"));
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/2500.mp3")); //12

                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/AltitudeAlarm.mp3")); //13
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/Overspeed.mp3")); //14
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/AutopilotAlarm.mp3")); //15
                AudioAlti.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/StallWarning.mp3")); //16

                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/eng1fire.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/eng2fire.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/" + prefix + "/evacuate.mp3"));


                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "10").GetComponent<AudioSource>().clip);   //0
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "20").GetComponent<AudioSource>().clip);
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "30").GetComponent<AudioSource>().clip);
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "40").GetComponent<AudioSource>().clip);
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "50").GetComponent<AudioSource>().clip);

                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "100").GetComponent<AudioSource>().clip);  //5
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "200").GetComponent<AudioSource>().clip);
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "300").GetComponent<AudioSource>().clip);
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "400").GetComponent<AudioSource>().clip);
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "500").GetComponent<AudioSource>().clip);

                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "1000").GetComponent<AudioSource>().clip); //10
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "1500").GetComponent<AudioSource>().clip);
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "2500").GetComponent<AudioSource>().clip); //12

                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "AltitudeAlarm").GetComponent<AudioSource>().clip); //13
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "Overspeed").GetComponent<AudioSource>().clip); //14
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "AutopilotAlarm").GetComponent<AudioSource>().clip); //15
                //AudioAlti.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "StallWarning").GetComponent<AudioSource>().clip); //16

                //AudioSecondary.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "eng1fire").GetComponent<AudioSource>().clip);
                //AudioSecondary.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "eng2fire").GetComponent<AudioSource>().clip);
                //AudioSecondary.Add(SearchUtilities.Find("AlarmBank_Body/Sector/" + prefix + "evacuate").GetComponent<AudioSource>().clip);
            }
        }

    }
}
