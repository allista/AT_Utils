using FinePrint.Utilities;

namespace AT_Utils
{
    public static class OrbitalExtensions
    {
        public static bool ApAhead(this Orbit obt) => obt.timeToAp < obt.timeToPe;

        public static bool Contains(this Orbit obt, double UT) =>
            obt.StartUT <= UT && UT <= obt.EndUT;

        public static double MinPeR(this Orbit obt) =>
            obt.referenceBody.atmosphere
                ? obt.referenceBody.Radius + obt.referenceBody.atmosphereDepth
                : obt.referenceBody.Radius
                  + CelestialUtilities.GetHighestPeak(obt.referenceBody)
                  + 1000;

        public static double GetEndUT(this Orbit obt)
        {
            var end = obt.EndUT;
            while(obt.nextPatch != null
                  && obt.nextPatch.referenceBody != null
                  && obt.patchEndTransition != Orbit.PatchTransitionType.FINAL)
            {
                obt = obt.nextPatch;
                end = obt.EndUT;
            }
            return end;
        }

        public static Vector3d hV(this Orbit obt, double UT) =>
            Vector3d.Exclude(obt.getRelativePositionAtUT(UT), obt.getOrbitalVelocityAtUT(UT));

        public static double TerrainAltitude(this CelestialBody body, double Lat, double Lon)
        {
            if(body.pqsController == null)
                return 0;
            var alt = body.pqsController.GetSurfaceHeight(body.GetRelSurfaceNVector(Lat, Lon))
                      - body.pqsController.radius;
            return body.ocean && alt < 0 ? 0 : alt;
        }

        public static double TerrainAltitude(this CelestialBody body, Vector3d wpos) =>
            TerrainAltitude(body, body.GetLatitude(wpos), body.GetLongitude(wpos));

        public static AtmosphereParams AtmoParamsAtAltitude(this CelestialBody body, double alt) =>
            new AtmosphereParams(body, alt);

        public static double ApAUT(this Orbit orb) => Planetarium.GetUniversalTime() + orb.timeToAp;

        public static double PeAUT(this Orbit orb) => Planetarium.GetUniversalTime() + orb.timeToPe;

        public static Vector3d ApV(this Orbit orb) =>
            orb.getRelativePositionAtUT(Planetarium.GetUniversalTime() + orb.timeToAp);

        public static Vector3d PeV(this Orbit orb) =>
            orb.getRelativePositionAtUT(Planetarium.GetUniversalTime() + orb.timeToPe);
    }
}
