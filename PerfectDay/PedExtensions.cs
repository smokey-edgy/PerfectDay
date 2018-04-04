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
}
