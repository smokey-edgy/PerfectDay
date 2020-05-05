using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace PerfectDay
{
    enum ZombieState
    {
        StandingStill,
        Wandering,
        Pursuing,
        Attacking
    }  

    class Zombie
    {
        private Ped Ped;
        private static List<Zombie> Zombies = new List<Zombie>();
        private static Ped Leader;
        private Ped Target;
        private ZombieState State;

        private static AnimationSet ZombieWalk = new AnimationSet("move_m@drunk@verydrunk");
        private static RelationshipGroup ZombiesGroup = new RelationshipGroup("ZOMBIES");

        public Zombie(Ped ped)
        {
            Ped = ped;
            if (Leader == null)
                Leader = Ped;
            Zombies.Add(this);
        }

        public void Zombify()
        {
            Rage.Native.NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(Ped, "Fall", 100, 100);

            Ped.RelationshipGroup = ZombiesGroup;
            Ped.StaysInGroups = false;
            Ped.BlockPermanentEvents = true;
            Ped.IsCollisionEnabled = true;

            ZombieWalk.LoadAndWait();
            Ped.MovementAnimationSet = ZombieWalk;

            Ped.Tasks.Clear();
            Ped.Tasks.StandStill(-1);
            State = ZombieState.StandingStill;

            ZombieLoop();
        }

        private void ZombieLoop()
        {
            GameFiber.StartNew(() =>
            {
                while (NotDead())
                {                  
                    switch (State)
                    {
                        case ZombieState.StandingStill:
                            Wander();
                            break;
                        case ZombieState.Wandering:
                            LookForClosestTarget();
                            break;
                        case ZombieState.Pursuing:
                            LookForClosestTarget();
                            StartAttackingWhenCloseToTarget();
                            break;
                        case ZombieState.Attacking:
                            ZombifyTarget();
                            break;                        
                    }                    
                    GameFiber.Yield();
                }
            });
        }

        private void Wander()
        {
            Ped.Tasks.Clear();
            Ped.Tasks.Wander();
            State = ZombieState.Wandering;       
        }

        private void LookForClosestTarget()
        {
            if (Ped != Leader)
                return;

            Ped[] nearbyPeds = Ped.GetNearbyPeds(5);            
            foreach (Ped nearbyPed in nearbyPeds)
            {            
                if (nearbyPed.RelationshipGroup != ZombiesGroup)
                {
                    foreach (Zombie zombie in Zombies)
                    {
                        zombie.Ped.MovementAnimationSet = ZombieWalk;
                        zombie.Ped.Tasks.Clear();
                        zombie.Ped.Tasks.FollowToOffsetFromEntity(nearbyPed, Vector3.Zero);
                        zombie.Target = nearbyPed;
                        zombie.State = ZombieState.Pursuing;
                    }    
                    break;
                }
            }
        }

        private void StartAttackingWhenCloseToTarget()
        {
            if (Ped.DistanceTo(Target) <= 1)
            {
                State = ZombieState.Attacking;
            }       
        }

        private void ZombifyTarget()
        {
            new Zombie(Target).Zombify();
            State = ZombieState.Wandering;
        }
        
        private bool NotDead()
        {
            return Ped.IsValid() && !Ped.IsDead;
        }

    }
}
