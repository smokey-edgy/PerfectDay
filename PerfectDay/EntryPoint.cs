
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



        
        public static void spawnZombieEmergencyIncident2()
        {
            
           GameFiber.StartNew(() =>
           {
               Vector3 zombiePosition = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.GetOffsetPositionFront(10.0f));
               Ped zombie = spawnZombieWaitingToHappen(zombiePosition);

               Vector3 spawnPosition = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.GetOffsetPositionFront(50.0f));
               Vector3 targetPosition = zombiePosition;

               Vehicle emergencyVehicle = new Vehicle(new Model(1171614426), spawnPosition);               
               //emergencyVehicle.IsSirenOn = true;

               Ped ped = Rage.Native.NativeFunction.Natives.CREATE_PED_INSIDE_VEHICLE<Ped>(emergencyVehicle, 20, -1286380898, -1, 0, 1);
               Ped partnerPed = Rage.Native.NativeFunction.Natives.CREATE_PED_INSIDE_VEHICLE<Ped>(emergencyVehicle, 20, -1286380898, 0, 0, 1);
               GameFiber.StartNew(() => ped.LeaveAmbulanceAndPerformFirstAidOn(zombie, emergencyVehicle));
               GameFiber.StartNew(() => partnerPed.LeaveAmbulanceAndPerformFirstAidOn(zombie, emergencyVehicle));
           });
        }

        
        public static Ped spawnZombieWaitingToHappen(Vector3 spawnPosition)
        {
            float z;
            if (Rage.Native.NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, out z, false))
            {
                spawnPosition.Z = z;
            }
            Ped ped = new Ped(spawnPosition);
            ped.BlockPermanentEvents = true;
            ped.Tasks.ClearImmediately();
            ped.DisableTalking();
            AnimationDictionary animationDictionary = new AnimationDictionary("amb@world_human_bum_slumped@male@laying_on_left_side@base");
            ped.Tasks.PlayAnimation(animationDictionary, "base", 1.0f, AnimationFlags.StayInEndFrame);            
            return ped;
        }

        [Rage.Attributes.ConsoleCommand(Description = "Spawn an object", Name = "SpawnObject")]
        public static void spawnObject(String objectName)
        {
            var spawnPosition = Game.LocalPlayer.Character.GetOffsetPositionFront(5.0f);
            float z;
            if (Rage.Native.NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, out z, false))
            {
                spawnPosition.Z = z;
            }

            Rage.Native.NativeFunction.Natives.CREATE_OBJECT_NO_OFFSET<Rage.Object>(Game.GetHashKey(objectName), spawnPosition.X, spawnPosition.Y, spawnPosition.Z, true, true, false);
            
        }


        public static void militaryFence(Vector3 spawnPosition, float heading)            
        {
            for (int i = 0; i < 10; i++)
            {               
                
                //if (!(Rage.Native.NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, new Vehicle())))
                //    return;

                float z;
                if (Rage.Native.NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, out z, false))
                {
                    spawnPosition.Z = z;
                }

                Rage.Object fence = Rage.Native.NativeFunction.Natives.CREATE_OBJECT_NO_OFFSET<Rage.Object>(Game.GetHashKey("prop_fnclink_03h"), spawnPosition.X, spawnPosition.Y, spawnPosition.Z, true, true, false);                
                fence.Heading = heading;
                Rage.Object barrier = Rage.Native.NativeFunction.Natives.CREATE_OBJECT_NO_OFFSET<Rage.Object>(Game.GetHashKey("prop_mp_barrier_01"), spawnPosition.X, spawnPosition.Y, spawnPosition.Z, true, true, false);
                barrier.Heading = heading;
                Vector3 leftVector = Vector3.Multiply(Vector3.Negate(barrier.RightVector), 5.9f);
                Vector3 nextBarrierPosition = Vector3.Add(barrier.RightPosition, leftVector);
                Rage.Object barrier2 = Rage.Native.NativeFunction.Natives.CREATE_OBJECT_NO_OFFSET<Rage.Object>(Game.GetHashKey("prop_mp_barrier_01"), nextBarrierPosition.X, nextBarrierPosition.Y, nextBarrierPosition.Z, true, true, false);
                barrier2.Heading = heading;
                leftVector = Vector3.Multiply(Vector3.Negate(fence.RightVector), 10.0f);
                spawnPosition = Vector3.Add(fence.RightPosition, leftVector);                
            }        
        }

        [Rage.Attributes.ConsoleCommand(Description = "Create a military blockade", Name = "Military")]
        public static void spawnMilitaryBlockade()
        {
            //Good candidates:
            /*
             * prop_mp_barrier_01
             * prop_facgate_01
             */
            Vector3 spawnPosition = Game.LocalPlayer.Character.GetOffsetPositionFront(20.0f);

            Vector3 closestVehicleNodeCoords;
            float roadHeading;
            Rage.Native.NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, out closestVehicleNodeCoords, out roadHeading, 1, 3, 0);                       

            militaryFence(spawnPosition, roadHeading);

        }

        [Rage.Attributes.ConsoleCommand(Description = "Nearby objects and model names", Name = "Models")]
        public static void nearbyModels()
        {
            
            foreach(Entity entity in World.GetEntities(Game.LocalPlayer.Character.Position, 10.0f, GetEntitiesFlags.ConsiderAllObjects))
            {
                Game.Console.Print("entity model = " + entity.Model.Name + " hash = " + entity.Model.Hash);
            }


        }

        [Rage.Attributes.ConsoleCommand(Description = "Create a zombie emergency incident", Name = "Zombie")]
        public static void spawnZombieEmergencyIncident()
        {
            if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
                return;

            Vector3 zombiePosition = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.GetOffsetPositionFront(1000.0f));
            Ped zombie = spawnZombieWaitingToHappen(zombiePosition);

            Model carModel = new Model(1171614426);
            Vehicle playerVehicle = Game.LocalPlayer.Character.CurrentVehicle;
            Vector3 backwardVector = Vector3.Negate(playerVehicle.ForwardVector);
            Vector3 behindVehicle = Vector3.Multiply(backwardVector, 30);
            Vector3 spawnPosition = Vector3.Add(playerVehicle.RearPosition, behindVehicle);

            Vehicle emergencyVehicle = new Vehicle(carModel, spawnPosition, playerVehicle.Heading);
            Rage.Native.NativeFunction.Natives.SET_VEHICLE_FORWARD_SPEED(emergencyVehicle, playerVehicle.Speed);
            emergencyVehicle.IsSirenOn = true;

            Ped ped = Rage.Native.NativeFunction.Natives.CREATE_PED_INSIDE_VEHICLE<Ped>(emergencyVehicle, 20, -1286380898, -1, 0, 1);
            Ped partnerPed = Rage.Native.NativeFunction.Natives.CREATE_PED_INSIDE_VEHICLE<Ped>(emergencyVehicle, 20, -1286380898, 0, 0, 1);
            
            ped.Tasks.DriveToPosition(emergencyVehicle, zombiePosition, emergencyVehicle.TopSpeed, VehicleDrivingFlags.Emergency, 10.0f).WaitForCompletion();            

            GameFiber.StartNew(() => ped.LeaveAmbulanceAndPerformFirstAidOn(zombie, emergencyVehicle));
            GameFiber.StartNew(() => partnerPed.LeaveAmbulanceAndPerformFirstAidOn(zombie, emergencyVehicle));
            reanimate(zombie, 30000);
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
                            
                            //taskSequence.Tasks.ReactAndFlee(Game.LocalPlayer.Character);
                            taskSequence.Execute();
                            Ped[] nearbyPeds = ped.GetNearbyPeds(1);
                            if(nearbyPeds.Length > 0)
                                Rage.Native.NativeFunction.Natives.TASK_CHAT_TO_PED(ped, nearbyPeds[0], 1, 0, 0, 0, 0, 0);
                            ped.Tasks.Wander();
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

        public static void reanimate(Ped deadPed, int howLongToReanimate)
        {
            GameFiber.StartNew(() => {                
                GameFiber.Sleep(howLongToReanimate);                
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

            zombie.Health = zombie.MaxHealth;
            zombieWalk.LoadAndWait();
            zombie.MovementAnimationSet = zombieWalk;     

            zombie.RelationshipGroup = ZombiesGroup;
            zombie.StaysInGroups = false;
            zombie.BlockPermanentEvents = true;
            zombie.IsCollisionEnabled = true;

            //zombie.PlaySound(@"Plugins\zombie.wav");            

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
                                            reanimate(closestHuman, 5000);
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
