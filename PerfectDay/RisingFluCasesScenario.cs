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
    }
}
