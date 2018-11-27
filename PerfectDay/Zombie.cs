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
                            LookForTargets();
                            break;
                        case ZombieState.Pursuing:
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
            ZombieWalk.LoadAndWait();
            Ped.MovementAnimationSet = ZombieWalk;
            Ped.Tasks.Wander();       
        }

        private void LookForTargets()
        {
        }

        private void StartAttackingWhenCloseToTarget()
        {

        }

        private void ZombifyTarget()
        {

        }
        private void Die()
        {

        }

        private bool NotDead()
        {
            return ZombieState.Dead != State;
        }

    }
}
