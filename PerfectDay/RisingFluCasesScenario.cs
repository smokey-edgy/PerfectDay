using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using NAudio.Wave;

namespace PerfectDay
{
    class RisingFluCasesScenario
    {

        public RisingFluCasesScenario()
        {
        
        }

        public void Start()
        {
            GameFiber.StartNew(() => {
                Vehicle playerVehicle = Game.LocalPlayer.Character.CurrentVehicle;
                if (playerVehicle != null)
                {
                    GameFiber.Sleep(3000);                    
                    ChangeStationsForEffect();
                    PreventPlayerFromChangingRadioIn(playerVehicle);
                    PlayNewsBroadcast(playerVehicle);
                    RestoreRadioControlToPlayerIn(playerVehicle);
                    ChangeStationsForEffect();
                    SpawnAmbulancesAtHospitals();
                }                
            });                
        }

        private void PreventPlayerFromChangingRadioIn(Vehicle vehicle)
        {
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_RADIO_ENABLED(vehicle, false);
            Rage.Native.NativeFunction.Natives.SET_USER_RADIO_CONTROL_ENABLED(false);
        }

        private void RestoreRadioControlToPlayerIn(Vehicle vehicle)
        {
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_RADIO_ENABLED(vehicle, true);
            Rage.Native.NativeFunction.Natives.SET_USER_RADIO_CONTROL_ENABLED(true);
        }

        private void PlayNewsBroadcast(Vehicle vehicle)
        {
            var reader = new Mp3FileReader(@"Plugins\initial_radio_broadcast.mp3");
            var waveOut = new WaveOut(); // or WaveOutEvent()
            waveOut.Init(reader);
            waveOut.Play();
            
            while (waveOut.PlaybackState == PlaybackState.Playing && vehicle.IsEngineOn)
                GameFiber.Yield();

            waveOut.Stop();
            waveOut.Dispose();
            waveOut = null;
            reader.Dispose();
            reader = null;
        }

        private void ChangeStationsForEffect()
        {           
            int radioStationIndex = Rage.Native.NativeFunction.Natives.GET_PLAYER_RADIO_STATION_INDEX<int>();            
            Rage.Native.NativeFunction.Natives.SKIP_RADIO_FORWARD();
            GameFiber.Sleep(500);
            Rage.Native.NativeFunction.Natives.SKIP_RADIO_FORWARD();            
            GameFiber.Sleep(1000);
            Rage.Native.NativeFunction.Natives.SKIP_RADIO_FORWARD();            
            GameFiber.Sleep(800);
            Rage.Native.NativeFunction.Natives.SKIP_RADIO_FORWARD();
            GameFiber.Sleep(400);
            Rage.Native.NativeFunction.Natives.SET_RADIO_TO_STATION_INDEX(radioStationIndex);
        }

        private void SpawnAmbulancesAtHospitals()
        {
            Vehicle hash1171614426_636656536913000753 = new Vehicle(new Model(1171614426), new Vector3(-453.3583f, -338.3976f, 34.36348f), 350.2608f);
            Vehicle hash1171614426_636656537081210374 = new Vehicle(new Model(1171614426), new Vector3(-453.4534f, -345.7689f, 34.4999f), 346.9436f);
            Vehicle hash1171614426_636656537199397134 = new Vehicle(new Model(1171614426), new Vector3(-459.58f, -346.3934f, 34.37057f), 289.6459f);
            Vehicle hash1171614426_636656537287442170 = new Vehicle(new Model(1171614426), new Vector3(-466.2082f, -351.8208f, 34.0503f), 322.7789f);
            Vehicle hash1171614426_636656537505004613 = new Vehicle(new Model(1171614426), new Vector3(-469.0224f, -358.9347f, 33.94617f), 343.1626f);
            Vehicle hash1171614426_636656537654683175 = new Vehicle(new Model(1171614426), new Vector3(-473.5845f, -356.2509f, 33.94907f), 354.3062f);
            Vehicle hash1171614426_636656537696055541 = new Vehicle(new Model(1171614426), new Vector3(-480.731f, -354.9664f, 34.09108f), 44.50783f);
            Vehicle hash1171614426_636656537751778728 = new Vehicle(new Model(1171614426), new Vector3(-485.6405f, -345.0836f, 34.37884f), 34.5263f);
            Vehicle hash1171614426_636656537822252759 = new Vehicle(new Model(1171614426), new Vector3(-490.8056f, -336.86f, 34.36238f), 16.50317f);
            Vehicle hash1171614426_636656537991922464 = new Vehicle(new Model(1171614426), new Vector3(-489.0233f, -329.9831f, 34.37708f), 99.03619f);
            Vehicle hash1171614426_636656538173402844 = new Vehicle(new Model(1171614426), new Vector3(-480.7259f, -328.5074f, 34.50066f), 173.5973f);
            Vehicle hash1171614426_636656538234376331 = new Vehicle(new Model(1171614426), new Vector3(-456.1938f, -330.8273f, 34.50074f), 238.9657f);
        }
    }
}
