
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
        private static RelationshipGroup MilitaryGroup = new RelationshipGroup("MILITARY");
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
        
        [Rage.Attributes.ConsoleCommand(Description = "Create a military blockade", Name = "Military")]
        public static void spawnMilitaryBlockade()
        {        
            MilitaryBlockadeScenario blockade = new MilitaryBlockadeScenario(Game.LocalPlayer.Character.GetOffsetPositionFront(20.0f));
            blockade.Start();
        }

        [Rage.Attributes.ConsoleCommand(Description = "Radio broadcast about rising flu cases", Name = "Flu")]
        public static void flu()
        {
            RisingFluCasesScenario risingFluCases = new RisingFluCasesScenario();
            risingFluCases.Start();
        }

        [Rage.Attributes.ConsoleCommand(Description = "Create a blackout", Name = "Blackout")]
        public static void Blackout()
        {            
            GameFiber.StartNew(() => {
                GameFiber.Sleep(5000);                
                Rage.Native.NativeFunction.CallByHash<uint>(0x1268615ACE24D504, true);
                GameFiber.Sleep(150);
                Rage.Native.NativeFunction.CallByHash<uint>(0x1268615ACE24D504, false);
                GameFiber.Sleep(150);
                Rage.Native.NativeFunction.CallByHash<uint>(0x1268615ACE24D504, true);                
            });            
        }

        [Rage.Attributes.ConsoleCommand(Description = "Start Fire", Name = "Fire")]
        public static void StartFire()
        {
            Fire[] fires = World.GetAllFires();
            foreach(Fire fire in fires)
            {
                fire.Delete();
            }

            for (int i = 0; i < 500; i++)
            {
                Vector3 somewhereAround = Game.LocalPlayer.Character.GetOffsetPositionFront(10.0f).Around(MathHelper.GetRandomSingle(5.0f, 10.0f));
                float z;
                if (Rage.Native.NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(somewhereAround.X, somewhereAround.Y, somewhereAround.Z, out z, false))
                {
                    somewhereAround.Z = z;
                }

                Rage.Native.NativeFunction.Natives.START_SCRIPT_FIRE(somewhereAround.X, somewhereAround.Y, somewhereAround.Z, 25, false);
                fires = World.GetAllFires();
                foreach (Fire fire in fires)
                {
                    fire.SpreadRadius = 100.0f;
                }
            }
            //for (int i = 0; i < 10; i++)
            //{
            //    vector3 somewherearound = game.localplayer.character.getoffsetpositionfront(10.0f);//.around(mathhelper.getrandomsingle(5.0f, 10.0f));
            //    rage.native.nativefunction.natives.start_script_fire(somewherearound.x, somewherearound.y, somewherearound.z, 25, false);
            //    gamefiber.sleep(300);
            //}
            //Fire[] fires = World.GetAllFires();
            //foreach(Fire fire in fires)
            //{
            //    fire.DesiredBurnDuration = 100000.0f;
            //    fire.SpreadRadius = 300.0f;                    
            //}

            //Rage.Native.NativeFunction.CallByHash<uint>(0xB80D8756B4668AB6, "scr_rcbarry2");
            //Rage.Native.NativeFunction.CallByHash<uint>(0x6C38AF3693A69A91, "scr_rcbarry2");
            //Rage.Native.NativeFunction.CallByHash<uint>(0x0D53A3B8DA0809D2, "scr_clown_appears", Game.LocalPlayer.Character, 0.0f, 0.0f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, false, false, false);

            //Rage.Native.NativeFunction.CallByHash<uint>(0xB80D8756B4668AB6, "core");
            //Rage.Native.NativeFunction.Natives.REQUEST_NAMED_PTFX_ASSET("core");            
            //Rage.Native.NativeFunction.Natives._USE_PARTICLE_FX_ASSET_NEXT_CALL("core");
            //Rage.Native.NativeFunction.CallByHash<uint>(0x6C38AF3693A69A91, "core");
            //Rage.Native.NativeFunction.CallByHash<int>(0x25129531F77B9ED3, "fire_wrecked_plane_cockpit", somewhereAround.X, somewhereAround.Y, somewhereAround.Z + 10.0f, 0.0f, 0.0f, 0.0f, 10.0f, false, false, false);
            //Rage.Native.NativeFunction.Natives.START_PARTICLE_FX_LOOPED_AT_COORD("fire_wrecked_plane_cockpit", somewhereAround.X, somewhereAround.Y, somewhereAround.Z, 0.0f, 0.0f, 0.0f, 1.0f, false, false, false);          

            //Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_rcbarry2");


            //Rage.Native.NativeFunction.Natives.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY();
            //for (int i = 0; i < 10; i++)
            //{
            //    Vector3 somewhereAround = Game.LocalPlayer.Character.GetOffsetPositionFront(10.0f);
            //    float z;
            //    if (Rage.Native.NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(somewhereAround.X, somewhereAround.Y, somewhereAround.Z, out z, false))
            //    {
            //        somewhereAround.Z = z;
            //    }

            //    //Vehicle vehicle = new Vehicle(new Model("ArmyTanker"), somewhereAround);
            //    //vehicle.IsVisible = false;
            //    //vehicle.Explode();

            //    Rage.Native.NativeFunction.Natives.START_PARTICLE_FX_LOOPED_AT_COORD("fire_wrecked_bus", somewhereAround.X, somewhereAround.Y, somewhereAround.Z + 4f, 0, 0, 0, 100.0f, 0, 0, 0, 0);

            //Rage.Native.NativeFunction.Natives.ADD_EXPLOSION(somewhereAround.X, somewhereAround.Y, somewhereAround.Z, 21, 500.0f, true, false, 0.0f);
            //Fire[] fires = World.GetAllFires();
            //foreach(Fire fire in fires)
            //{
            //    fire.DesiredBurnDuration = 100000.0f;
            //    fire.SpreadRadius = 300.0f;                    
            //}
            //    GameFiber.Sleep(3000);
            //}
            //foreach(Vehicle vehicle in Game.LocalPlayer.Character.GetNearbyVehicles(5))
            //{
            //    Rage.Native.NativeFunction.Natives.START_ENTITY_FIRE(vehicle);

            //}
        }


        [Rage.Attributes.ConsoleCommand(Description = "Bye Bye Planes", Name = "Planes")]
        public static void ByeByePlanes()
        {
            foreach(Vehicle vehicle in World.GetAllVehicles())
            {
                if(vehicle.IsInAir)
                {
                    vehicle.PunctureFuelTank();
                    //vehicle.SetRotationPitch()
                    vehicle.FuelLevel = 0.0f;
                    vehicle.TopSpeed = 0.0f;
                    vehicle.EngineHealth = 0.0f;
                }
            }
        }


        [Rage.Attributes.ConsoleCommand(Description = "Restore the lights", Name = "Lights")]
        public static void Lights()
        {
            //BlackoutScenario blackoutScenario = new BlackoutScenario();
            //blackoutScenario.Start();
            GameFiber.StartNew(() =>
            {
                Rage.Native.NativeFunction.CallByHash<uint>(0x1268615ACE24D504, false);
            });
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
                //Rage.Native.NativeFunction.Natives.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(0.0f);
                //Rage.Native.NativeFunction.Natives.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(0.0f);
                //Rage.Native.NativeFunction.Natives.SET_FAR_DRAW_VEHICLES(0);
                //Rage.Native.NativeFunction.Natives.POPULATE_NOW();
                GameFiber.Yield();
            }
        }

        [Rage.Attributes.ConsoleCommand(Description = "Zombify someone nearby", Name = "Zombify")]
        public static void Zombify()
        {
            Ped playerPed = Game.LocalPlayer.Character;
            Ped[] nearbyPeds = playerPed.GetNearbyPeds(1);
            foreach(Ped ped in nearbyPeds)
            {
                Zombie zombie = new Zombie(ped);
                zombie.Zombify();
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
