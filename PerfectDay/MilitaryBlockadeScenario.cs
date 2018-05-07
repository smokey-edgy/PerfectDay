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
        private Vector3 position;
        private float direction;
        private Rage.Object initialFenceSegment;

        public MilitaryBlockadeScenario(Vector3 blockadePosition)
        {
            position = blockadePosition;
            DetermineBlockadeDirection();
        }
        
        public void Start()
        {
            SpawnMilitaryBlockade();
        }

        private void SpawnMilitaryBlockade()
        {
            initialFenceSegment = SpawnFenceAt(position);
            
            ExtendFenceToTheEdgeOfTheRoadHeadingRight(initialFenceSegment.RightPosition);
            ExtendFenceToTheEdgeOfTheRoadHeadingLeft(initialFenceSegment.LeftPosition);
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
            Rage.Native.NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(position.X, position.Y, position.Z, out ignore, out direction, 1, 3, 0);            
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
            barrier.Heading = direction;
            fence.Heading = direction;

            return fence;
        }
    }
}
