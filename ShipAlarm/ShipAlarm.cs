using System.Reflection;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

//using ShipAlarm.Components;
using ShipAlarm;


using ShipAlarm.Components;
using System;
using System.IO;

namespace ShipAlarm
{
    public class ShipAlarm : ModBehaviour
    {

        public static ShipAlarm ShipAlarmInstance;
        public INewHorizons newHorizonsAPI;
        public GameObject _alarmMaster;
        public AssetBundle _shipAlarmBundle;
        public AudioClip AudioTest;

        public void Awake()
        {
            ShipAlarmInstance = this;

            // You won't be able to access OWML's mod helper in Awake. Use Start() instead.
        }




        public void Start()
        {
            newHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            newHorizonsAPI.LoadConfigs(this);
            newHorizonsAPI.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);

            ModHelper.Console.WriteLine($"Look upon my Works, ye Mighty, and despair! {nameof(ShipAlarm)} is loaded...", MessageType.Success);

            // Get the New Horizons API and load configs

            _shipAlarmBundle = AssetBundle.LoadFromFile(Path.Combine(ModHelper.Manifest.ModFolderPath, "assets/alarmsaudio"));

            new Harmony("SpatialXX.ShipAlarm").PatchAll(Assembly.GetExecutingAssembly());



            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;

            AudioTest = LoadAudio("Assets/AlarmsAudio/MD/StallWarning.mp3");
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            ModHelper.Console.WriteLine("SPX Scene load! " + newScene, MessageType.Success);
            //if (newScene != OWScene.SolarSystem) return;
            SpawnOnStart();
        }



        private void OnStarSystemLoaded(string systemName)
        {
            //WriteUtil.WriteLine("SPX LOADED SYSTEM " + systemName);

            SpawnOnStart();
        }




        public void SpawnOnStart()
        {
            if (_alarmMaster == null)
            {
                _alarmMaster= new GameObject("AlarmMaster");
                _alarmMaster.AddComponent<AlarmInit>();
                ShipAlarmInstance.ModHelper.Events.Unity.FireInNUpdates(_alarmMaster.GetComponent<AlarmInit>().Start, 10);
            }
            else
            {
                _alarmMaster.GetComponent<AlarmInit>().InitializedAlarm = false;
            }


        }

        public void LogForStupids(string PrintIT)
        {
            ModHelper.Console.WriteLine(PrintIT, MessageType.Success);
        }

        public void ConfigureAlarmFirstStep()
        {
            var volumeAlarm = ModHelper.Config.GetSettingsValue<float>("Alarms Volume");
            var stallWarning = ModHelper.Config.GetSettingsValue<bool>("stallWarning");
            var overspeed = ModHelper.Config.GetSettingsValue<float>("Overspeed Sensibility");
            var altitudeWarning = ModHelper.Config.GetSettingsValue<float>("Altitude Warning Distance");
            var sunWarn = ModHelper.Config.GetSettingsValue<float>("Sun Warning Distance");
            var AlarmsVoice = ModHelper.Config.GetSettingsValue<string>("Alarms Voice");
            var ReadNumbers = ModHelper.Config.GetSettingsValue<string>("Read Numbers");

            if (_alarmMaster != null)
            {
                if (_alarmMaster.GetComponent<AlarmInit>() != null)
                {
                    _alarmMaster.GetComponent<AlarmInit>().ConfigureInTwoStep(volumeAlarm, stallWarning, overspeed, altitudeWarning, sunWarn, AlarmsVoice, ReadNumbers);
                }
            }
        }

        public override void Configure(IModConfig config)
        {
            ConfigureAlarmFirstStep();
            //var newFavorite = config.GetSettingsValue<string>("Favorite Food");
            //ModHelper.Console.WriteLine($"You changed your favorite food to: {newFavorite}!");
        }

        public AudioClip LoadAudio(string path)
        {
            return (AudioClip)_shipAlarmBundle.LoadAsset(path);
        }

        public object LoadAsset(string path)
        {
            return _shipAlarmBundle.LoadAsset(path);
        }


    }
}