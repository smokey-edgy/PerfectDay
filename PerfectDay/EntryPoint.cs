
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
                
        public static void SpawnZombie()
        {

            Vector3 spawnPosition = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.GetOffsetPositionFront(100f));
            Model zombieModel = new Model("u_m_y_zombie_01");

            Ped zombie = new Ped(zombieModel, spawnPosition, 0);

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
                            if (closestHuman != null)
                                zombie.Tasks.FightAgainst(closestHuman).WaitForCompletion();
                            else
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
    }        
}
