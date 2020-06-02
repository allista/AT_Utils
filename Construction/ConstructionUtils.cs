using System.Collections.Generic;
using Experience;
using UnityEngine;

namespace AT_Utils
{
    public static class ConstructionUtils
    {
        /// <summary>
        /// Computes the workforce of a single kerbal with a particular
        /// experience effect based on the experience level.
        /// </summary>
        /// <param name="kerbal">A kerbal</param>
        /// <param name="minCap">How much workforce provides completely inexperienced kerbal</param>
        /// <typeparam name="E">ExperienceEffect that contributes to the workforce</typeparam>
        /// <returns>float workforce in the range [minCap, max experience level of a kerbal]</returns>
        public static float KerbalWorkforce<E>(ProtoCrewMember kerbal, float minCap) where E : ExperienceEffect
        {
            var workforce = 0f;
            var trait = kerbal.experienceTrait;
            for(int i = 0, traitEffectsCount = trait.Effects.Count; i < traitEffectsCount; i++)
            {
                if(!(trait.Effects[i] is E))
                    continue;
                workforce += Mathf.Max(trait.CrewMemberExperienceLevel(), minCap);
                break;
            }
            return workforce;
        }

        /// <summary>
        /// Computes the total workforce of a crew, counting kerbals with a particular
        /// experience effect based on their experience level.
        /// </summary>
        /// <param name="crew">A list of kerbals, the crew</param>
        /// <param name="minCap">How much workforce provides completely inexperienced kerbal</param>
        /// <typeparam name="E">ExperienceEffect that contributes to the workforce</typeparam>
        /// <returns>float workforce</returns>
        public static float CrewWorkforce<E>(List<ProtoCrewMember> crew, float minCap) where E : ExperienceEffect
        {
            var workforce = 0f;
            for(int k = 0, kerbalsCount = crew.Count; k < kerbalsCount; k++)
            {
                var kerbal = crew[k];
                workforce += KerbalWorkforce<E>(kerbal, minCap);
            }
            return workforce;
        }

        /// <summary>
        /// Computes the total workforce of a part's crew, counting kerbals with a particular
        /// experience effect based on their experience level.
        /// </summary>
        /// <param name="part">A part</param>
        /// <param name="minCap">How much workforce provides completely inexperienced kerbal</param>
        /// <typeparam name="E">ExperienceEffect that contributes to the workforce</typeparam>
        /// <returns>float workforce</returns>
        public static float PartWorkforce<E>(Part part, float minCap) where E : ExperienceEffect =>
            CrewWorkforce<E>(part.protoModuleCrew, minCap);

        /// <summary>
        /// Computes the total workforce a vessel's crew, counting kerbals with a particular
        /// experience effect based on their experience level.
        /// </summary>
        /// <param name="vessel">A vessel</param>
        /// <param name="minCap">How much workforce provides completely inexperienced kerbal</param>
        /// <typeparam name="E">ExperienceEffect that contributes to the workforce</typeparam>
        /// <returns>float workforce</returns>
        public static float VesselWorkforce<E>(Vessel vessel, float minCap) where E : ExperienceEffect =>
            CrewWorkforce<E>(vessel.GetVesselCrew(), minCap);

        /// <summary>
        /// Returns time passed from the last time; or the fixed delta time, if last time is less than 0.
        /// The lastTime is set to current UT.
        /// If time since level load is less than 1 second or if FlightGlobals are not ready, returns -1;
        /// </summary>
        /// <param name="lastTime">Last universal timestamp of the process</param>
        /// <returns>Time passed from the last timestamp, or -1</returns>
        public static double GetDeltaTime(ref double lastTime)
        {
            if(Time.timeSinceLevelLoad < 1 || !FlightGlobals.ready)
                return -1;
            if(lastTime < 0)
            {
                lastTime = Planetarium.GetUniversalTime();
                return TimeWarp.fixedDeltaTime;
            }
            var time = Planetarium.GetUniversalTime();
            var dT = time - lastTime;
            lastTime = time;
            return dT;
        }
    }
}
