
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
            GameFiber.StartNew(CreateTrafficJams3);
            GameFiber.StartNew(() => {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if (vehicle && vehicle.IsValid())
                    {
                        Ped ped = vehicle.Driver;
                        if (ped && ped.IsValid() && !ped.IsLocalPlayer)
                        {
                            vehicle.EngineHealth = 0.0f;
                            ped.Tasks.Clear();
                            Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(ped, 1074528293);
                            Rage.Native.NativeFunction.Natives.ADD_SHOCKING_EVENT_AT_POSITION<uint>(116, ped.Position, 0);
                        }
                    }
                }

                GameFiber.Sleep(10000);

            });
        }

        [Rage.Attributes.ConsoleCommand(Description = "Start a traffic jam", Name = "TrafficJam")]
        public static void CreateTrafficJamsCommand()
        {
            GameFiber.StartNew(CreateTrafficJams3);
        }

        public static void CreateTrafficJams3()
        {
            //Game.LocalPlayer.Character.Tasks.Clear();
           
            //Rage.Native.NativeFunction.Natives.ADD_SHOCKING_EVENT_AT_POSITION<uint>(86, Game.LocalPlayer.Character.Position, 0);

            while (true)
            {
                Rage.Native.NativeFunction.Natives.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(100.0f);
                Rage.Native.NativeFunction.Natives.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(100.0f);
                //Rage.Native.NativeFunction.Natives._SET_SOME_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(2);
                //Rage.Native.NativeFunction.Natives.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(2);
                Rage.Native.NativeFunction.Natives.SET_FAR_DRAW_VEHICLES(1);
                Rage.Native.NativeFunction.Natives.POPULATE_NOW();
                //foreach (Vehicle vehicle in World.GetAllVehicles())
                //{
                //    vehicle.EngineHealth = 0.0f;
                //}
               
                GameFiber.Yield();
            }

        }

        public static void CreateTrafficJams2()
        {
            Game.Console.Print("****** About to create Traffic jams");

            int numberOfVehiclesToSpawn = 10;
            
            while (numberOfVehiclesToSpawn > 0)
            {
                Vector3 somewhereAroundThePlayer = Game.LocalPlayer.Character.Position.Around(MathHelper.GetRandomSingle(10f, 20f));
                float z;
                if (Rage.Native.NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(somewhereAroundThePlayer.X, somewhereAroundThePlayer.Y, somewhereAroundThePlayer.Z, out z, false))
                {
                    somewhereAroundThePlayer.Z = z;
                }
                bool pointIsOnRoad = Rage.Native.NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(somewhereAroundThePlayer.X, somewhereAroundThePlayer.Y, somewhereAroundThePlayer.Z);

                if (!pointIsOnRoad)
                    continue;

                Entity[] entitiesInPossibleSpawnLocation = World.GetEntities(somewhereAroundThePlayer, 10, GetEntitiesFlags.ConsiderAllObjects);
                if (entitiesInPossibleSpawnLocation.Length > 0)
                    continue;

                Vehicle vehicle = new Vehicle((Model model) => { return model.IsCar && !model.IsBigVehicle; }, somewhereAroundThePlayer);
                Ped ped = vehicle.CreateRandomDriver();
                vehicle.IsEngineOn = true;
                ped.Tasks.CruiseWithVehicle(20.0f);
                numberOfVehiclesToSpawn--;                
            }
        }

        public static void CreateTrafficJams()
        {
            Game.Console.Print("****** About to create Traffic jams");            

            int numberOfVehiclesToSpawn = 100;

            Vehicle[] nearbyVehicles = Game.LocalPlayer.Character.GetNearbyVehicles(1);
            if (nearbyVehicles.Length == 0)
                return;

            Vehicle nearbyVehicle = nearbyVehicles[0];

            while (numberOfVehiclesToSpawn > 0)
            {
                if (nearbyVehicle.IsValid())
                {
                    Vector3 possibleSpawnLocation = nearbyVehicle.GetOffsetPositionFront(3);
                    Entity[] entitiesInPossibleSpawnLocation = World.GetEntities(possibleSpawnLocation, 2, GetEntitiesFlags.ConsiderAllObjects);

                    if (entitiesInPossibleSpawnLocation.Length > 0)
                    {
                        possibleSpawnLocation = nearbyVehicle.GetOffsetPositionRight(3);
                        entitiesInPossibleSpawnLocation = World.GetEntities(possibleSpawnLocation, 2, GetEntitiesFlags.ConsiderAllObjects);
                    }

                    bool pointIsOnRoad = Rage.Native.NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(possibleSpawnLocation.X, possibleSpawnLocation.Y, possibleSpawnLocation.Z);
                    if (entitiesInPossibleSpawnLocation.Length == 0 && pointIsOnRoad)
                    {
                        Vehicle vehicle = new Vehicle((Model model) => { return model.IsCar && !model.IsBigVehicle; }, possibleSpawnLocation);
                        Ped ped = vehicle.CreateRandomDriver();

                        if (nearbyVehicle.IsValid())
                        {
                            vehicle.Heading = nearbyVehicle.Heading;
                            vehicle.IsEngineOn = true;
                            ped.Tasks.CruiseWithVehicle(nearbyVehicle.Speed);
                            numberOfVehiclesToSpawn--;
                            nearbyVehicle = vehicle;
                        }
                        else
                        {
                            //vehicle.Delete();
                            ped.Delete();
                        }
                    }
                    else
                    {
                        nearbyVehicles = Game.LocalPlayer.Character.GetNearbyVehicles(1);
                        if (nearbyVehicles.Length == 0)
                            return;

                        nearbyVehicle = nearbyVehicles[0];
                    }
                }
                GameFiber.Yield();
            }
          
                
            }


            //Vector3 playerPos = Game.LocalPlayer.Character.GetOffsetPositionFront(0);
            
            //int numberOfVehiclesToSpawn = 20;

            //while (numberOfVehiclesToSpawn > 0)
            //{
            //    Vehicle[] nearestVehicles = Game.LocalPlayer.Character.GetNearbyVehicles(10);
            //    for (int i = 0; i < nearestVehicles.Length; i++)
            //    {
            //        Vehicle nearestVehicle = nearestVehicles[i];
            //        for (int j = 0; j < 20; j++)
            //        {
            //            if (nearestVehicle && !nearestVehicle.IsValid())
            //                break;
            //            Vector3 nextPositionOnStreetNotOccupied = World.GetNextPositionOnStreet(nearestVehicle.GetOffsetPositionFront(20.0f * j));// NextPositionOnStreet(nearestVehicle.GetOffsetPositionFront(20.0f));
            //            Entity[] entitiesNearPosition = World.GetEntities(nextPositionOnStreetNotOccupied, 5.0f, GetEntitiesFlags.ConsiderAllObjects);
            //            if (entitiesNearPosition.Length > 0)
            //                continue;

            //            Vehicle vehicle = new Vehicle((Model model) => { return model.IsCar; }, nextPositionOnStreetNotOccupied);
            //            vehicle.Heading = nearestVehicle.Heading;
            //            vehicle.IsEngineOn = true;
            //            Ped ped = vehicle.CreateRandomDriver();
            //            ped.Tasks.CruiseWithVehicle(nearestVehicle.Speed);
            //            numberOfVehiclesToSpawn--;
            //            if (numberOfVehiclesToSpawn == 0)
            //                break;
            //            nearestVehicle = vehicle;
            //            GameFiber.Sleep(500);
            //        }
            //    }
            //}
            //Vector3 lastVehiclePosition = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.GetOffsetPositionFront(10.0f));
            //for (int i = 0; i<2; i++)
            //{                                
            //    Vehicle vehicle = new Vehicle((Model model) => { return model.IsCar; }, lastVehiclePosition);
            //    vehicle.IsEngineOn = true;
            //    vehicle.Velocity = vehicle.ForwardVector * 2;               
            //    Ped ped = vehicle.CreateRandomDriver();
            //    ped.Tasks.CruiseWithVehicle(40);

            //    Vehicle vehicle2 = new Vehicle((Model model) => { return model.IsCar; }, vehicle.GetOffsetPositionRight(5.0f));
            //    vehicle2.IsEngineOn = true;
            //    vehicle2.Velocity = vehicle2.ForwardVector * 2;
            //    Ped ped2 = vehicle2.CreateRandomDriver();
            //    ped2.Tasks.CruiseWithVehicle(40);

            //    lastVehiclePosition = vehicle.GetOffsetPositionFront(5.0f);
            //}


            //while (true)
            //{
            //    Rage.Native.NativeFunction.Natives.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(1.3f);
            //    Rage.Native.NativeFunction.Natives.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(1.3f);
            //    Rage.Native.NativeFunction.Natives.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(1.3f);
            //    //World.AddSpeedZone(Game.LocalPlayer.Character.GetOffsetPositionFront(1000.0f), 100.0f, 0.0f);
            //    Entity[] entities = World.GetEntities(Game.LocalPlayer.Character.Position, 500.0f, GetEntitiesFlags.ConsiderCars);
            //    for (int i = 0; i < entities.Length; i++)
            //    {
            //        Vehicle car = (Vehicle)entities[i];

            //        if (car.Driver && !car.Driver.Equals(Game.LocalPlayer.Character))
            //        {
            //            car.EngineHealth = 0.0f;                                                
            //        }

            //    }
            //    GameFiber.Sleep(1000);        

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
