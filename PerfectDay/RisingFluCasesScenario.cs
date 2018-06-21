using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

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
                while (true)
                {
                    if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                    {
                        GameFiber.Sleep(3000);
                        Vehicle vehicle = Game.LocalPlayer.Character.LastVehicle;
                        //Rage.Native.NativeFunction.Natives.SET_USER_RADIO_CONTROL_ENABLED(false);
                        //Rage.Native.NativeFunction.Natives.SET_VEHICLE_RADIO_ENABLED(vehicle, false);
                        ChangeStationsForEffect();
                        Rage.Native.NativeFunction.Natives.SET_VEHICLE_RADIO_ENABLED(vehicle, false);
                        Rage.Native.NativeFunction.Natives.SET_USER_RADIO_CONTROL_ENABLED(false);
                        GameFiber.Sleep(5000);
                        Rage.Native.NativeFunction.Natives.SET_VEHICLE_RADIO_ENABLED(vehicle, true);
                        Rage.Native.NativeFunction.Natives.SET_USER_RADIO_CONTROL_ENABLED(true);                        
                    }
                    GameFiber.Yield();
                }
            });
                //SET_USER_RADIO_CONTROL_ENABLED

                //SET_VEHICLE_RADIO_ENABLED
                //IS_RADIO_RETUNING
            }
        private void ChangeStationsForEffect()
        {           
            int radioStationIndex = Rage.Native.NativeFunction.Natives.GET_PLAYER_RADIO_STATION_INDEX<int>();
            //Rage.Native.NativeFunction.Natives.SET_RADIO_TO_STATION_INDEX(radioStationIndex + 1);
            Rage.Native.NativeFunction.Natives.SKIP_RADIO_FORWARD();
            GameFiber.Sleep(500);
            Rage.Native.NativeFunction.Natives.SKIP_RADIO_FORWARD();
            //Rage.Native.NativeFunction.Natives.SET_RADIO_TO_STATION_INDEX(radioStationIndex + 2);
            GameFiber.Sleep(1000);
            Rage.Native.NativeFunction.Natives.SKIP_RADIO_FORWARD();
            //Rage.Native.NativeFunction.Natives.SET_RADIO_TO_STATION_INDEX(radioStationIndex + 3);
            GameFiber.Sleep(800);
            Rage.Native.NativeFunction.Natives.SKIP_RADIO_FORWARD();
            GameFiber.Sleep(400);
            Rage.Native.NativeFunction.Natives.SET_RADIO_TO_STATION_INDEX(radioStationIndex);
        }
    }
}
