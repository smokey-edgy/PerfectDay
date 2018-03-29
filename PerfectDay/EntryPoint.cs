
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
            Ped[] nearbyPeds = ped.GetNearbyPeds(16);
            for(int i = 0; i<nearbyPeds.Length; i++)
            {
                if (nearbyPeds[i].Exists() && !nearbyPeds[i].RelationshipGroup.Equals(ZombiesGroup) &&
                    nearbyPeds[i].IsAlive)
                    return nearbyPeds[i];
            }
            return null;
        }

        public static Ped GetClosestLivingHumanNearThatIsNot(Ped notThisPed, Ped ped)
        {
            if (notThisPed && ped)
            {
                Ped[] nearbyPeds = ped.GetNearbyPeds(16);
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

        public static void zombify(Ped zombie)
        {            
            Rage.Native.NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(zombie, "Fall", 100, 100);
            
            if (zombie == null)
            {
                return;
            }

            AnimationSet animationSet = new AnimationSet("move_m@drunk@verydrunk");
            animationSet.LoadAndWait();
            zombie.MovementAnimationSet = animationSet;

            zombie.RelationshipGroup = ZombiesGroup;
            zombie.StaysInGroups = false;
            zombie.BlockPermanentEvents = true;

            zombie.Tasks.Wander();

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
                        if (!zombie.IsInMeleeCombat || zombie.IsStill)
                        {
                            Ped closestHuman = GetClosestLivingHumanNear(zombie);
                            if (closestHuman)
                            {
                                FollowHuman:
                                zombie.Tasks.FollowToOffsetFromEntity(closestHuman, Vector3.Zero);

                                while (true)
                                {
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
                            } else
                                zombie.Tasks.Wander();
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
            zombify(GetClosestLivingHumanNear(Game.LocalPlayer.Character));
        }
            
    }        
}
