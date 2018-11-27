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
        Attacking,
        Dead
    }  

    class Zombie
    {
        private Ped Ped;
        private Ped Target;
        private ZombieState State;

        private static AnimationSet ZombieWalk = new AnimationSet("move_m@drunk@verydrunk");
        private static RelationshipGroup ZombiesGroup = new RelationshipGroup("ZOMBIES");

        public Zombie(Ped ped)
        {
            Ped = ped;
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
                    GameFiber.Yield();
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
                }
            });
        }

        private void Wander()
        {
            GameFiber.Sleep(5000);
            Ped.Tasks.Clear();
            Ped.Tasks.Wander();
            State = ZombieState.Wandering;       
        }

        private void LookForClosestTarget()
        {
            Ped[] nearbyPeds = Ped.GetNearbyPeds(1);
            foreach (Ped nearbyPed in nearbyPeds)
            {
                if(nearbyPed.RelationshipGroup != ZombiesGroup)
                {
                    Ped.MovementAnimationSet = ZombieWalk;
                    Ped.Tasks.Clear();
                    Ped.Tasks.FollowToOffsetFromEntity(nearbyPed, Vector3.Zero);
                    Target = nearbyPed;
                    State = ZombieState.Pursuing;                    
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
            return ZombieState.Dead != State;
        }

    }
}
