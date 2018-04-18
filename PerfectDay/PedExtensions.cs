using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Extensions
{
    public static class EntityExtensions
    {
        public static void PlaySound(this Entity entity, String audioFilePath)
        {
            GameFiber.StartNew(() => {
                var outputDevice = new WaveOutEvent();
                var audioFile = new AudioFileReader(audioFilePath);
                var volumeProvider = new VolumeSampleProvider(audioFile);

                var panner = new PanningSampleProvider(volumeProvider);
                panner.PanStrategy = new SquareRootPanStrategy();
                panner.Pan = 0f;

                outputDevice.Init(panner);
                outputDevice.Play();

                var player = Game.LocalPlayer.Character;

                while (entity && entity.Exists())
                {
                    var distance = player.DistanceTo(entity);
                    var volumeCalculation = (100.0f - (distance * 5)) / 100.0f;
                    volumeProvider.Volume = volumeCalculation <= 0 ? 0 : volumeCalculation;

                    var zombieVector = player.GetPositionOffset(entity.Position).ToNormalized();
                    panner.Pan = zombieVector.X;

                    if (Game.IsPaused)
                        outputDevice.Pause();
                    else
                        outputDevice.Play();

                    GameFiber.Yield();
                }

                outputDevice.Dispose();
                outputDevice = null;
                audioFile.Dispose();
                audioFile = null;
            });
        }
    }

    public static class PedExtentions
    {
        public static void LeaveAmbulanceAndPerformFirstAidOn(this Ped ped, Ped otherPed, Vehicle emergencyVehicle)
        {
            Vector3 targetPosition = otherPed.Position;
            ped.BlockPermanentEvents = true;
            ped.Tasks.Clear();            
            ped.Tasks.LeaveVehicle(emergencyVehicle, LeaveVehicleFlags.None);
            ped.Tasks.FollowNavigationMeshToPosition(targetPosition.Around(3.0f), 0.0f, 5.0f).WaitForCompletion();
            Rage.Native.NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(ped, otherPed, -1);            
            AnimationSet crouchDown = new AnimationSet("move_ped_crouched");
            crouchDown.LoadAndWait();
            ped.MovementAnimationSet = crouchDown;
            ped.BlockPermanentEvents = false;
        }

        public static void PreventAnyMovement(this Ped ped)
        {
            GameFiber.StartNew(() =>
            {
                while (ped && ped.IsValid())
                {
                    ped.CollisionIgnoredEntity = Game.LocalPlayer.Character;
                    ped.Tasks.ClearImmediately();                                        
                    GameFiber.Yield();
                }
            });
            
        }

        public static void DisableTalking(this Ped ped)
        {
            GameFiber.StartNew(() =>
            {
                while (ped && ped.IsValid())
                {
                    if (Rage.Native.NativeFunction.Natives.IS_AMBIENT_SPEECH_PLAYING<bool>(ped))
                        Rage.Native.NativeFunction.Natives.STOP_CURRENT_PLAYING_AMBIENT_SPEECH(ped);

                    GameFiber.Yield();
                }
            });
        }
    }
}
