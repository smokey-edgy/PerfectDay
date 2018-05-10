using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace PerfectDay
{
    class MilitaryBlockadeScenario
    {
        private static int _numberOfGuardingMarines = 4;
        private static int _numberOfChattingMarines = 4;
        private static int _numberOfWanderingMarines = 2;

        private Vector3 Position;
        private float Direction;
        private float MarinesDirection;
        private Vector3 MarinesSpawnPosition;
        private Rage.Object InitialFenceSegment;
        private Ped[] GuardingMarines = new Ped[_numberOfGuardingMarines];
        private Ped[] ChattingMarines = new Ped[_numberOfChattingMarines];
        private Ped[] WanderingMarines = new Ped[_numberOfWanderingMarines];

        public MilitaryBlockadeScenario(Vector3 blockadePosition)
        {
            Position = blockadePosition;
            DetermineBlockadeDirection();
        }

        public void Start()
        {
            SpawnBlockadeFence();
            SpawnMarines();
            HaveMarinesWarnAnyoneThatComesCloseToTheFence();
            HaveMarinesAimTheirWeaponsIfWarningsAreIgnored();
            HaveMarinesShootIfAllWarningsAreIgnored();
        }

        private Ped GetNearestNonMilitaryPed(Ped ped)
        {
            Ped[] nearbyPeds = ped.GetNearbyPeds(10);

            foreach (Ped nearbyPed in nearbyPeds)
            {
                if (!nearbyPed.RelationshipGroup.Equals(RelationshipGroup.Army))
                {
                    return nearbyPed;
                }
            }

            return null;
        }

        private void HaveMarinesWarnAnyoneThatComesCloseToTheFence()
        {
            int randomMarine = MathHelper.GetRandomInteger(0, _numberOfChattingMarines - 1);
            Ped enforcerMarine = ChattingMarines[randomMarine];

            if (IsNightTime())
                enforcerMarine.Inventory.Weapons.Remove(new WeaponDescriptor(new WeaponAsset("WEAPON_ASSAULTRIFLE")));            

            GameFiber.StartNew(() =>
            {
                uint warnings = 0;

                while (true)
                {
                    if (!enforcerMarine.IsValid())
                        break;

                    Ped nearestPed = GetNearestNonMilitaryPed(enforcerMarine);

                    if (nearestPed != null)
                    {
                        if (nearestPed.DistanceTo(InitialFenceSegment) <= 5.0f)
                        {
                            Vector3 originalPosition = enforcerMarine.Position;

                            DoNotPursueIfPedWalksAway(enforcerMarine, nearestPed, originalPosition);

                            enforcerMarine.Tasks.GoToOffsetFromEntity(nearestPed, 3.0f, 0.0f, 0.1f).WaitForCompletion();                            
                            Rage.Native.NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(enforcerMarine, nearestPed, -1);

                            if (IsNightTime())
                                enforcerMarine.Tasks.AimWeaponAt(nearestPed, 10000);

                            enforcerMarine.PlayAmbientSpeech(null, "PROVOKE_TRESPASS", 1, SpeechModifier.ForceShoutedCritical);
                            warnings++;
                            if (warnings == 2)
                            {
                                enforcerMarine.Tasks.GoToWhileAiming(nearestPed, 10.0f, 100.0f);
                            }
                            if (warnings == 3)
                            {
                                enforcerMarine.Tasks.GoToWhileAiming(nearestPed, 10.0f, 100.0f);                                
                                foreach (Ped marine in GuardingMarines)
                                {
                                    marine.Tasks.GoToWhileAiming(nearestPed, 10.0f, 100.0f);                                    
                                }
                            }
                            if (warnings >= 4)
                            {
                                enforcerMarine.Tasks.FireWeaponAt(nearestPed, -1, FiringPattern.BurstFireRifle);
                                foreach (Ped marine in GuardingMarines)
                                {
                                    marine.Tasks.FireWeaponAt(nearestPed, -1, FiringPattern.BurstFireRifle);
                                }

                            }
                            GameFiber.Sleep(10000);
                        }
                    }
                    GameFiber.Yield();
                }
            });
        }

        private bool IsNightTime()
        {
            int hour = World.DateTime.Hour;
            return hour >= 21 && hour <= 23 && hour >= 0 && hour <= 6;
        }

        private void DoNotPursueIfPedWalksAway(Ped militaryPed, Ped nearestPed, Vector3 originalMilitaryPedPosition)
        {
            GameFiber.StartNew(() =>
            {
                while (true)
                {
                    if (nearestPed.DistanceTo(InitialFenceSegment) > 5.0f)
                    {
                        militaryPed.Tasks.Clear();
                        militaryPed.Tasks.GoStraightToPosition(originalMilitaryPedPosition, 1.0f, 0.0f, 0.1f, 20000);
                        break;
                    }
                    GameFiber.Yield();
                }
            });
        }
        private void HaveMarinesAimTheirWeaponsIfWarningsAreIgnored()
        {

        }

        private void HaveMarinesShootIfAllWarningsAreIgnored()
        {

        }

        private void SpawnMarines()
        {
            DetermineMarinesDirectionAndSpawnPosition();
            SpawnMarinesGuardingFence();
            SpawnChattingMarines();
            SpawnWanderingMarines();
        }

        private void SpawnWanderingMarines()
        {
            Vector3 spawnPosition = InitialFenceSegment.GetOffsetPositionFront(6.0f);

            for (int i = 0; i < _numberOfWanderingMarines; i++)
            {
                Vector3 marinePosition = spawnPosition.Around2D(0.0f, 3.0f);
                Ped wanderingMarine = EquippedMarine(marinePosition, MarinesDirection);
                WanderingMarines[i] = wanderingMarine;
                wanderingMarine.Tasks.Wander();
            }
        }

        private void SpawnChattingMarines()
        {
            Vector3 spawnPosition = InitialFenceSegment.GetOffsetPositionFront(6.0f);

            for (int i = 0; i < _numberOfChattingMarines; i++)
            {
                Vector3 marinePosition = spawnPosition.Around2D(0.0f, 3.0f);
                Ped marine = EquippedMarine(marinePosition, MarinesDirection);
                ChattingMarines[i] = marine;

                if (i > 0)
                    Rage.Native.NativeFunction.Natives.TASK_CHAT_TO_PED(ChattingMarines[i - 1], ChattingMarines[i], 1, 0, 0, 0, 0, 0);
            }
        }
        private void SpawnMarinesGuardingFence()
        {
            Vector3 spawnPosition = MarinesSpawnPosition;

            for (int i = 0; i < _numberOfGuardingMarines; i++)
            {
                Vector3 marinePosition = spawnPosition;
                spawnPosition = Vector3.Add(marinePosition, Vector3.Multiply(Vector3.Negate(InitialFenceSegment.RightVector), 3.0f));
                Ped marine = EquippedMarine(marinePosition, MarinesDirection);
                GuardingMarines[i] = marine;
                Rage.Native.NativeFunction.Natives.TASK_STAND_GUARD(marine, marine.Position.X, marine.Position.Y, marine.Position.Z, MarinesDirection, "WORLD_HUMAN_GUARD_STAND_ARMY");
            }
        }

        public Ped EquippedMarine(Vector3 marinePosition, float marineHeading)
        {
            Ped marine = new Ped(new Model("S_M_Y_Marine_03"), marinePosition, marineHeading);

            marine.RelationshipGroup = RelationshipGroup.Army;
            marine.StaysInGroups = false;
            marine.BlockPermanentEvents = true;
            marine.Tasks.ClearImmediately();
            marine.Inventory.GiveFlashlight();

            WeaponAsset assaultRifle = new WeaponAsset("WEAPON_ASSAULTRIFLE");
            marine.Inventory.GiveNewWeapon(assaultRifle, 100, true);
            marine.Inventory.AddComponentToWeapon(assaultRifle, "COMPONENT_AT_AR_FLSH");
            marine.Inventory.AddComponentToWeapon(assaultRifle, "COMPONENT_AT_SCOPE_MACRO");
            marine.Inventory.AddComponentToWeapon(assaultRifle, "COMPONENT_AT_AR_AFGRIP");

            return marine;
        }

        private void SpawnBlockadeFence()
        {
            InitialFenceSegment = SpawnFenceAt(Position);

            ExtendFenceToTheEdgeOfTheRoadHeadingRight(InitialFenceSegment.RightPosition);
            ExtendFenceToTheEdgeOfTheRoadHeadingLeft(InitialFenceSegment.LeftPosition);
        }

        private void ExtendFenceToTheEdgeOfTheRoadHeadingRight(Vector3 position)
        {
            ExtendFenceToTheEdgeOfTheRoad(true, position);
        }

        private void ExtendFenceToTheEdgeOfTheRoadHeadingLeft(Vector3 position)
        {
            ExtendFenceToTheEdgeOfTheRoad(false, position);
        }

        private void ExtendFenceToTheEdgeOfTheRoad(bool headingRight, Vector3 position)
        {
            bool justOffTheRoad = false;
            int segmentTick = 1;
            Vector3 nextSegmentPosition = position;

            while (segmentTick != 0)
            {
                if (!justOffTheRoad)
                {
                    if (!isPointOnRoad(nextSegmentPosition))
                    {
                        justOffTheRoad = true;
                    }
                }
                else
                {
                    segmentTick--;
                }
                Rage.Object segment = SpawnFenceAt(nextSegmentPosition);

                if (headingRight)
                    nextSegmentPosition = segment.RightPosition;
                else
                {
                    Vector3 leftVector = Vector3.Negate(segment.RightVector);
                    nextSegmentPosition = Vector3.Add(segment.LeftPosition, Vector3.Multiply(leftVector, segment.Width));
                }
            }
        }

        private bool isPointOnRoad(Vector3 position)
        {
            return Rage.Native.NativeFunction.Natives.IS_POINT_ON_ROAD<bool>(position.X, position.Y, position.Z, new Vehicle());
        }

        private void DetermineBlockadeDirection()
        {
            Vector3 ignore;
            Rage.Native.NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(Position.X, Position.Y, Position.Z, out ignore, out Direction, 1, 3, 0);
        }

        private void DetermineMarinesDirectionAndSpawnPosition()
        {
            Vector3 playerPosition = Game.LocalPlayer.Character.Position;
            MarinesSpawnPosition = InitialFenceSegment.GetOffsetPositionFront(1.0f);
            MarinesDirection = MathHelper.ConvertDirectionToHeading(InitialFenceSegment.ForwardVector);

            if (playerPosition.DistanceTo(InitialFenceSegment.RearPosition) < playerPosition.DistanceTo(InitialFenceSegment.FrontPosition))
            {
                MarinesSpawnPosition = InitialFenceSegment.GetOffsetPositionFront(-1.0f);
                MarinesDirection = MathHelper.ConvertDirectionToHeading(Vector3.Negate(InitialFenceSegment.ForwardVector));
            }
        }

        private Rage.Object SpawnFenceAt(Vector3 spawnPosition)
        {
            float z;
            if (Rage.Native.NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, out z, false))
            {
                spawnPosition.Z = z;
            }

            Rage.Object fence = Rage.Native.NativeFunction.Natives.CREATE_OBJECT_NO_OFFSET<Rage.Object>(Game.GetHashKey("prop_fnclink_03h"), spawnPosition.X, spawnPosition.Y, spawnPosition.Z, true, true, false);
            Rage.Object barrier = Rage.Native.NativeFunction.Natives.CREATE_OBJECT_NO_OFFSET<Rage.Object>(Game.GetHashKey("prop_mp_barrier_01"), spawnPosition.X, spawnPosition.Y, spawnPosition.Z, true, true, false);
            barrier.Heading = Direction;
            fence.Heading = Direction;

            return fence;
        }
    }
}
