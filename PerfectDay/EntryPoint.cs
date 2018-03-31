
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

[assembly: Rage.Attributes.Plugin("A Perfect Day", Description = "The Perfect Day Zombie Outbreak.", Author = "Smoke")]
namespace PerfectDay
{
    public class EntryPoint
    {
        private static RelationshipGroup ZombiesGroup = new RelationshipGroup("ZOMBIES");
        private static AnimationSet zombieWalk = new AnimationSet("move_m@drunk@verydrunk");
        private static AnimationSet zombieEating = new AnimationSet("move_ped_crouched");

        public static void Main()
        {
            Game.Console.Print("***** PerfectDay has been loaded.");
            GameFiber.StartNew(EntryPoint.SpawnZombie);
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
