//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2015 Allis Tauri
//
// This work is licensed under the Creative Commons Attribution-ShareAlike 4.0 International License. 
// To view a copy of this license, visit http://creativecommons.org/licenses/by-sa/4.0/ 
// or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.

namespace AT_Utils
{
    public struct AtmosphereParams
    {
        public readonly CelestialBody Body;
        public readonly double Alt;
        public readonly double P;
        public readonly double T;
        public readonly double Rho;
        public readonly double Mach1;

        public AtmosphereParams(CelestialBody body, double altitude)
        {
            Alt = altitude;
            Body = body;
            if(Body.atmosphere)
            {
                P = Body.GetPressure(Alt);
                T = Body.GetTemperature(Alt);
                Rho = Body.GetDensity(P, T);
                Mach1 = Body.GetSpeedOfSound(P, Rho);
            }
            else
            {
                P = 0;
                T = -273;
                Rho = 0;
                Mach1 = 0;
            }
        }

        public override string ToString()
        {
            return Utils.Format("{} Atmosphere Params at Alt: {} m\nP {}, T {}, Rho {}, Mach1 {} m/s",
                                Body.name, Alt, P, T, Rho, Mach1);
        }
    }
}
