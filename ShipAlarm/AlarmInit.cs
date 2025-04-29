
using OWML.ModHelper;
using OWML.Common;
using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using ShipAlarm;
using static UnityEngine.UI.Image;
using System.ComponentModel;
using Random = UnityEngine.Random;


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
        private OWRigidbody BrambleRB;
        private bool BrambleOnlyOnce;
        private OWRigidbody TwinRB;
        private bool TwinOnlyOnce;
        private OWRigidbody GiantRB;
        private bool GiantOnlyOnce;
        private OWRigidbody BrittleRB;
        private bool BrittleOnlyOnce;
        private OWRigidbody LanternRB;
        private bool LanternOnlyOnce;
        private OWRigidbody QuantumRB;
        private bool QuantumOnlyOnce;
        private OWRigidbody RingRB;
        private bool RingOnlyOnce;
        private bool StartUpOnce;
        private bool PreviousPlayerInShip;
        private bool AuthorizeWelcome;

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

        private HatchController hatchControll;

        public void Start()
        {

        }

        public void Update()
        {
            if (InitializedAlarm == false)
            {
                if (GameObject.Find("/Ship_Body/Module_Cockpit") == null)
                {
                    return;
                }

                GameObject Ship_Cockpit = GameObject.Find("/Ship_Body/Module_Cockpit");
                GameObject PrefabVerboseAlarm = GameObject.Find("/Ship_Body/Audio_Ship/ShipInteriorAudio/AlarmAudio");
                ShipReferenceFrame = GameObject.Find("/ShipScreenSpaceUI").transform.GetChild(0).GetComponent<ReferenceFrameGUI>();
                ShipDamaged = GameObject.Find("/Ship_Body/Module_Cockpit/Systems_Cockpit/ShipCockpitUI/DamageScreen/HUD_ShipDamageDisplay").GetComponent<ShipDamageDisplayV2>()._shipComponents;
                if (GameObject.Find("/Ship_Body/Module_Cabin/Systems_Cabin/Hatch/HatchControls") != null)
                {
                    hatchControll = GameObject.Find("/Ship_Body/Module_Cabin/Systems_Cabin/Hatch/HatchControls").GetComponent<HatchController>();
                }
                
                //GameObject Source = GameObject.Find("/AlarmBank_Body/Sector/BAltitudeAlarm");
                GameObject MainMasterLight = GameObject.Find("/Ship_Body/Module_Cabin/Lights_Cabin/PointLight_HEA_MasterAlarm");



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



                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/MD/eng1fire.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/MD/eng2fire.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/MD/evacuate.mp3"));

                ConditionsForAlarm = new List<bool> { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                //ConditionsForAlarm.Add(false);  //0 Left Engine
                //ConditionsForAlarm.Add(false);  //1 Right Engine
                //ConditionsForAlarm.Add(false);  //2 Boilers
                //                                //3 Ecological Dead zone
                //                                //4 Motivation 8




                if (GameObject.Find("/Sun_Body") != null)
                {
                    GameObject SunBody = GameObject.Find("/Sun_Body");
                    if (SunBody.activeSelf == true)
                    {
                        SunRB = SunBody.GetComponent<OWRigidbody>();
                    }
                }

                if (GameObject.Find("/DarkBramble_Body") != null)
                {
                    GameObject BrambleBody = GameObject.Find("/DarkBramble_Body");
                    if (BrambleBody.activeSelf == true)
                    {
                        BrambleRB = BrambleBody.GetComponent<OWRigidbody>();
                    }
                }

                if (GameObject.Find("/CaveTwin_Body") != null)
                {
                    GameObject TwinBody = GameObject.Find("/CaveTwin_Body");
                    if (TwinBody.activeSelf == true)
                    {
                        TwinRB = TwinBody.GetComponent<OWRigidbody>();
                    }
                }

                if (GameObject.Find("/GiantsDeep_Body") != null)
                {
                    GameObject GiantBody = GameObject.Find("/GiantsDeep_Body");
                    if (GiantBody.activeSelf == true)
                    {
                        GiantRB = GiantBody.GetComponent<OWRigidbody>();
                    }
                }

                if (GameObject.Find("/BrittleHollow_Body") != null)
                {
                    GameObject BrittleBody = GameObject.Find("/BrittleHollow_Body");
                    if (BrittleBody.activeSelf == true)
                    {
                        BrittleRB = BrittleBody.GetComponent<OWRigidbody>();
                    }
                }

                if (GameObject.Find("/VolcanicMoon_Body") != null)
                {
                    GameObject LanternBody = GameObject.Find("/VolcanicMoon_Body");
                    if (LanternBody.activeSelf == true)
                    {
                        LanternRB = LanternBody.GetComponent<OWRigidbody>();
                    }
                }

                if (GameObject.Find("/QuantumMoon_Body") != null)
                {
                    GameObject QuantomBody = GameObject.Find("/QuantumMoon_Body");
                    if (QuantomBody.activeSelf == true)
                    {
                        QuantumRB = QuantomBody.GetComponent<OWRigidbody>();
                    }
                }

                if (GameObject.Find("/RingWorld_Body") != null)
                {
                    GameObject RingBody = GameObject.Find("/RingWorld_Body");
                    if (RingBody.activeSelf == true)
                    {
                        RingRB = RingBody.GetComponent<OWRigidbody>();
                    }
                }

                

                Autopilot = GameObject.Find("/Ship_Body").GetComponent<Autopilot>();


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
                    
                    if (!StartUpOnce && ShipReferenceFrame.gameObject.activeSelf && hatchControll!=null)
                    {
                        if (hatchControll._isPlayerInShip)
                        {
                            ClipToRead.Add(Random.Range(20, 35));
                            StartUpOnce = true;
                        }
                    }

                    if (hatchControll != null && AuthorizeWelcome)
                    {
                        if (hatchControll._isPlayerInShip && PreviousPlayerInShip != hatchControll._isPlayerInShip)
                        {
                            ClipToRead.Add(36);
                        }
                        PreviousPlayerInShip = hatchControll._isPlayerInShip;
                    }

                    

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

                    if (BrambleRB != null && !BrambleOnlyOnce)
                    {
                        float BrambleDist = Vector3.Distance(BrambleRB._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (BrambleDist < 200)
                        {
                            BrambleOnlyOnce = true;
                            ClipToRead.Add(Random.Range(3, 5));
                        }
                    }

                    if (TwinRB != null && !TwinOnlyOnce)
                    {
                        float BrambleDist = Vector3.Distance(TwinRB._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (BrambleDist < 600)
                        {
                            TwinOnlyOnce = true;
                            ClipToRead.Add(Random.Range(6, 8));
                        }
                    }

                    if (GiantRB != null && !GiantOnlyOnce)
                    {
                        float BrambleDist = Vector3.Distance(GiantRB._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (BrambleDist < 850)
                        {
                            GiantOnlyOnce = true;
                            ClipToRead.Add(Random.Range(9, 11));
                        }
                    }

                    if (BrittleRB != null && !BrittleOnlyOnce)
                    {
                        float BrambleDist = Vector3.Distance(BrittleRB._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (BrambleDist < 280)
                        {
                            BrittleOnlyOnce = true;
                            ClipToRead.Add(Random.Range(12, 14));
                        }
                    }

                    if (LanternRB != null && !LanternOnlyOnce)
                    {
                        float BrambleDist = Vector3.Distance(LanternRB._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (BrambleDist < 600)
                        {
                            LanternOnlyOnce = true;
                            ClipToRead.Add(15);
                        }
                    }

                    if (QuantumRB != null && !QuantumOnlyOnce)
                    {
                        float BrambleDist = Vector3.Distance(QuantumRB._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (BrambleDist < 450)
                        {
                            QuantumOnlyOnce = true;
                            ClipToRead.Add(16);
                        }
                    }

                    if (RingRB != null && !RingOnlyOnce)
                    {
                        float BrambleDist = Vector3.Distance(RingRB._lastPosition, ShipAlarmAudio.gameObject.transform.position);

                        if (BrambleDist < 590)
                        {
                            RingOnlyOnce = true;
                            ClipToRead.Add(Random.Range(16, 19));
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
                            if (OverSpeedAlarm >= overspeedSensibility && ShipReferenceFrame._orientedRelativeVelocity.z > 250 && dist < altitudeWarning)
                            {
                                PlayPriority(13);
                            }
                            else if (OverSpeedAlarm >= overspeedSensibility && ShipReferenceFrame._orientedRelativeVelocity.z > 250)
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

        private void CheckAltitude()
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
                        if (readOutLoud == 2 && i != 0)
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


        public void ConfigureInTwoStep(float SoundVolume, bool stallWarning, float overspeed, float altitudeWarn, float sunWarn, string AlarmsVoice, string ReadNumbers, bool subnauticaPDA, string Welcome)
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

                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/ecologicalDeadZone.mp3")); //3 DB
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/predatorycave.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/terror.mp3")); //5 DB
                
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/cavedivers.mp3")); //6 CT
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/cavesystem.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/fossils.mp3")); //8 CT


                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/aquatic.mp3")); //9 GD
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/plankton.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/seamoth.mp3")); //11 GD


                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/cautionatdeep.mp3")); //12 BH
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/energysignature.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/hugeenergy.mp3")); //14 BH

                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/volcanicactivity.mp3")); //15 HL

                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/unidentifiable.mp3")); //16 QM RW

                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/artificial.mp3")); //17 RW
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/largealien.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/orbitdestroy.mp3")); //19 RW


                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/alienressources.mp3")); //20
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/aquarium.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/diet.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/drink.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/explorationrisk.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/flywell.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/homesnothome.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/mediate.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/minorheadtrauma.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/motivation.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/pet.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/photojournal.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/prawnOp.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/problems.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/radioactive.mp3"));
                AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/motivation/settings.mp3")); //35

                AuthorizeWelcome = true;
                if (Welcome == "Cyclops")
                { AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/welcome/welcomeM1.mp3")); } //36
                else if (Welcome == "Prawn")
                { AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/welcome/welcomeM2.mp3")); }
                else if (Welcome == "Sea Moth")
                { AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/welcome/welcomeF1.mp3")); }
                else if (Welcome == "Sea Base")
                { AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/welcome/welcomeF2.mp3")); }
                else
                { AudioSecondary.Add(ShipAlarm.ShipAlarmInstance.LoadAudio("Assets/AlarmsAudio/Subnautica/welcome/welcomeM1.mp3"));
                    AuthorizeWelcome = false;
                } 
                



                BrambleOnlyOnce = subnauticaPDA;
                TwinOnlyOnce = subnauticaPDA;
                GiantOnlyOnce = subnauticaPDA;
                BrittleOnlyOnce = subnauticaPDA;
                LanternOnlyOnce = subnauticaPDA;
                QuantumOnlyOnce = subnauticaPDA;
                RingOnlyOnce = subnauticaPDA;
                StartUpOnce = subnauticaPDA;



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
