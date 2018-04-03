//   ResourceLine.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;

namespace AT_Utils
{
    public class ResourceLine : ResourceWrapper<ResourceLine>
    {
        /// <summary>
        /// Gets the density in tons/unit.
        /// </summary>
        public float Density { get { return Resource.density; } }

        /// <summary>
        /// Gets conversion rate in tons/sec.
        /// </summary>
        public float Rate    { get; private set; }
        float base_rate;

        /// <summary>
        /// Gets conversion rate in units/sec.
        /// </summary>
        public float URate   { get; private set; } //u/sec

        public ResourcePump Pump { get; private set; }

        public ResourceLine() {}
        public ResourceLine(Part part, PartResourceDefinition res_def, float rate)
        { 
            Resource = res_def; 
            Rate = base_rate = rate;
            if(res_def != null) 
            {
                Pump = new ResourcePump(part, res_def.id);
                URate = rate/res_def.density;
            }
        }

        public void InitializePump(Part part, float rate_multiplier)
        { 
            Pump  = new ResourcePump(part, Resource.id);
            Rate  = base_rate * rate_multiplier;
            URate = Rate/Resource.density;
        }

        public override void LoadDefinition(string resource_definition)
        {
            var rate = load_definition(resource_definition);
            if(!Valid) return;
            Rate  = base_rate = rate;
            URate = rate/Resource.density;
        }

        public bool TransferResource(float rate = 1f)
        {
            Pump.RequestTransfer(rate*URate*TimeWarp.fixedDeltaTime);
            return Pump.TransferResource();
        }

        public bool PartialTransfer { get { return Pump.PartialTransfer; } }

        public string Info
        { get { return string.Format("{0}: {1}/sec", Resource.name, Utils.formatUnits(URate)); } }
    }
}

