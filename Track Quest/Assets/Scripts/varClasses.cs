using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StatClasses
{
    public enum terrainTypes
    {
        road, dirtRoad, grass, sand, snow, ice, cobbles
    }

    public struct carStats
    {
        public float topSpeed, acceleration, brakes, steering;

        public float roadGrip, offRoadGrip;
    }

    public enum driverNumbers
    {
        P1, P2, P3, P4, P5, P6, P7, P8
    }

    public enum driveState
    {
        still, accelerating, deaccelerating, braking, reversing, drifting, turning, in_air, boosting, mini_boosting
    }

    public class timer
    {
        public float currentTime, duration;

        public timer(float timerDuration)
        {
            duration = timerDuration;
            currentTime = 0;
        }

        public void Tick()
        {
            if (currentTime < duration)
                currentTime += Time.deltaTime;
        }

        public void Reset()
        {
            currentTime = 0;
        }

        public bool Ended()
        {
            if (currentTime >= duration)
                return true;
            else
                return false;
        }
    }
}
