﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Extensions;

[assembly: Rage.Attributes.Plugin("A Perfect Day", Description = "The Perfect Day Zombie Outbreak.", Author = "Smoke")]
namespace PerfectDay
{
    public class EntryPoint
    {
        private static RelationshipGroup ZombiesGroup = new RelationshipGroup("ZOMBIES");
        private static AnimationSet zombieWalk = new AnimationSet("move_m@drunk@verydrunk");
        private static AnimationSet zombieEating = new AnimationSet("move_ped_crouched");
        
        public static bool IsGameReady()
        {
            return !Game.IsLoading;          
        }

        public static void Main()
        {
            Game.Console.Print("***** PerfectDay has been loaded.");

            GameFiber.WaitUntil(IsGameReady);
            //GameFiber.StartNew(SpawnZombie);
            GameFiber.StartNew(IncreaseTraffic);
            //GameFiber.StartNew(CreateMayhemAndPanic);
        }

        [Rage.Attributes.ConsoleCommand(Description = "Create an emergency incident", Name = "Emergency")]
        public static void CreateEmergencyIncident()
        {

            
            GameFiber.StartNew(() =>
            {
                SpawnEmergencyVehicle(new Model("Police"), 6, 1581098148, 20.0f);
                SpawnEmergencyVehicle(new Model(1171614426), 20, -1286380898, 30.0f);          
            });
        }


        [Rage.Attributes.ConsoleCommand(Description = "Create a zombie waiting to happen", Name = "Zombie")]
        public static void spawnZombieWaitingToHappen()
        {
            GameFiber.StartNew(() =>
            {
                Ped ped = new Ped(Game.LocalPlayer.Character.GetOffsetPositionFront(10.0f));
                Rage.Native.NativeFunction.Natives.SET_PED_TO_RAGDOLL(ped, 5000, 5000, 0, 1, 1, 0);
                Rage.Native.NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(ped, "Fall", 100, 100);
                ped.BlockPermanentEvents = true;
                ped.Tasks.ClearImmediately();                
                ped.Health = 4;                
            });
        }

        public static void SpawnEmergencyVehicle(Model carModel, int pedType, int pedModel, float metresBehind)
        {
            if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
                return;
            
            Vehicle playerVehicle = Game.LocalPlayer.Character.CurrentVehicle;
            Vector3 backwardVector = Vector3.Negate(playerVehicle.ForwardVector);
            Vector3 behindVehicle = Vector3.Multiply(backwardVector, metresBehind);
            Vector3 spawnPosition = Vector3.Add(playerVehicle.RearPosition, behindVehicle);

            Vehicle emergencyVehicle = new Vehicle(carModel, spawnPosition, playerVehicle.Heading);
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_FORWARD_SPEED(emergencyVehicle, playerVehicle.Speed);
            emergencyVehicle.IsSirenOn = true;

            Ped ped = Rage.Native.NativeFunction.Natives.CREATE_PED_INSIDE_VEHICLE<Ped>(emergencyVehicle, pedType, pedModel, -1, 0, 1);
                        
            emergencyVehicle.TopSpeed = 200.0f;
            TaskSequence taskSequence = new TaskSequence(ped);            
            taskSequence.Tasks.DriveToPosition(emergencyVehicle, World.GetNextPositionOnStreet(emergencyVehicle.GetOffsetPositionFront(1000.0f)), 200.0f, VehicleDrivingFlags.Emergency, 10.0f);
            taskSequence.Execute();        
        }

        [Rage.Attributes.ConsoleCommand(Description = "Create mayhem and panic", Name = "Mayhem")]
        public static void CreateMayhemAndPanic()
        {
            while (true)
            {
                Entity[] nearbyCars = World.GetEntities(Game.LocalPlayer.Character.GetOffsetPositionFront(200.0f), 150.0f, GetEntitiesFlags.ConsiderCars);
                
                foreach (Vehicle vehicle in nearbyCars)
                {
                    if (vehicle && vehicle.IsValid())
                    {                        
                        Ped ped = vehicle.Driver;
                        if (ped && ped.IsValid() && !ped.IsLocalPlayer)
                        {
                            ped.BlockPermanentEvents = true;
                            ped.Tasks.Clear();
                            TaskSequence taskSequence = new TaskSequence(ped);
                            taskSequence.Tasks.LeaveVehicle(vehicle, LeaveVehicleFlags.LeaveDoorOpen);
                            taskSequence.Tasks.ReactAndFlee(Game.LocalPlayer.Character);
                            taskSequence.Execute();
                            Rage.Native.NativeFunction.Natives.ADD_SHOCKING_EVENT_FOR_ENTITY<uint>(86, vehicle, 0);
                        }
                    }
                }
                GameFiber.Sleep(1500);
            }
        }

        public static void IncreaseTraffic()
        {           
            while (true)
            {
                Rage.Native.NativeFunction.Natives.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(100.0f);
                Rage.Native.NativeFunction.Natives.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(100.0f);
                Rage.Native.NativeFunction.Natives.SET_FAR_DRAW_VEHICLES(1);
                Rage.Native.NativeFunction.Natives.POPULATE_NOW();
                GameFiber.Yield();
            }
        }
        
        [Rage.Attributes.ConsoleCommand(Description = "Spawns a zombie", Name = "SpawnZombie")]
        public static void Command_SpawnZombie(int howMany)
        {
            Game.Console.Print("***** Spawning Zombie.");
            for(int i = 0; i < howMany; i++)
                GameFiber.StartNew(EntryPoint.SpawnZombie);
        }   

        public static Ped GetClosestLivingHumanNear(Ped ped)
        {
            // This is an expensive check to do every tick so let's ease off
            GameFiber.Sleep(2000);
            if (ped)
            {
                Ped[] nearbyPeds = ped.GetNearbyPeds(6);
                for (int i = 0; i < nearbyPeds.Length; i++)
                {
                    if (nearbyPeds[i].Exists() && !nearbyPeds[i].RelationshipGroup.Equals(ZombiesGroup) &&
                        nearbyPeds[i].IsAlive)
                        return nearbyPeds[i];
                }
            }
            return null;
        }

        public static Ped GetClosestLivingHumanNearThatIsNot(Ped notThisPed, Ped ped)
        {
            // This is an expensive check to do every tick so let's ease off
            GameFiber.Sleep(2000);
            if (notThisPed && ped)
            {                         
                Ped[] nearbyPeds = ped.GetNearbyPeds(6);
                for (int i = 0; i < nearbyPeds.Length; i++)
                {
                    if (nearbyPeds[i].Equals(notThisPed))
                        continue;

                    if (nearbyPeds[i].Exists() && !nearbyPeds[i].RelationshipGroup.Equals(ZombiesGroup) &&
                        nearbyPeds[i].IsAlive)
                        return nearbyPeds[i];
                }
            }
            return null;
        }

        public static void reanimate(Ped deadPed)
        {
            GameFiber.StartNew(() => {                
                GameFiber.Sleep(5000);                
                zombify(deadPed);
            });           
        }

        public static void zombify(Ped zombie)
        {            
            Rage.Native.NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(zombie, "Fall", 100, 100);            

            if (zombie == null)
            {
                return;
            }
            
            if(zombie.Exists() && zombie.IsValid())
                zombie.Tasks.Clear();            

            zombieWalk.LoadAndWait();
            zombie.MovementAnimationSet = zombieWalk;     

            zombie.RelationshipGroup = ZombiesGroup;
            zombie.StaysInGroups = false;
            zombie.BlockPermanentEvents = true;
            zombie.IsCollisionEnabled = true;

            zombie.PlaySound(@"Plugins\zombie.wav");            

            try
            {
                while (true)
                {
                    if (!zombie.Exists())
                    {
                        break;
                    }

                    if (zombie.IsAlive)
                    {
                        if(zombie && zombie.Exists() && zombie.IsAlive) zombie.Tasks.Wander();
                        if (!zombie.IsInMeleeCombat || zombie.IsStill)
                        {
                            Ped closestHuman = GetClosestLivingHumanNear(zombie);
                            if (closestHuman)
                            {
                                FollowHuman:
                                zombie.MovementAnimationSet = zombieWalk;
                                zombie.Tasks.FollowToOffsetFromEntity(closestHuman, Vector3.Zero);

                                while (true)
                                {
                                    /**
                                     *  Keep checking how close we are to the target and once we're are near,
                                     *  make them fall and reanimate 
                                     */
                                    if (zombie && closestHuman)
                                    {
                                        if (zombie.DistanceTo(closestHuman) <= 1) {
                                            
                                            Rage.Native.NativeFunction.Natives.SET_PED_TO_RAGDOLL(closestHuman, 5000, 5000, 0, 1, 1, 0);                                            
                                            zombieEating.LoadAndWait();
                                            zombie.MovementAnimationSet = zombieEating;
                                            Vector3 zombiePosition = zombie.GetOffsetPositionFront(0);
                                            Rage.Native.NativeFunction.Natives.ADD_SHOCKING_EVENT_AT_POSITION<uint>(114, zombiePosition.X, zombiePosition.Y, zombiePosition.Z, 0);                                            
                                            GameFiber.Sleep(2000);                                            
                                            reanimate(closestHuman);
                                            break;
                                        }
                                    }
                                    /**
                                     *  Keep checking for any human that is closer than the one we are pursuing, and start following
                                     *  them instead                                    
                                     */
                                    Ped nextClosestHuman = GetClosestLivingHumanNearThatIsNot(closestHuman, zombie);
                                    if (nextClosestHuman && zombie)
                                    {
                                        if (nextClosestHuman.DistanceTo(zombie) < closestHuman.DistanceTo(zombie))
                                        {
                                            closestHuman = nextClosestHuman;
                                            goto FollowHuman;
                                        }
                                    }
                                    GameFiber.Yield();
                                }
                            }
                            else
                            {
                                if (zombie && zombie.Exists() && zombie.IsAlive)
                                    zombie.Tasks.Wander();
                            }  
                        }
                    }
                    GameFiber.Yield();
                }
            }
            finally
            {
                if (zombie.Exists())
                {
                    zombie.Delete();
                    zombie = null;
                }
            }
        }

        public static void SpawnZombie()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            if(playerPed)
                zombify(GetClosestLivingHumanNear(playerPed));
        }
            
    }        
}
