//   ATMagneticDamper.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using UnityEngine;

namespace AT_Utils
{
    public class ATMagneticDamper: PartModule
    {
        [KSPField] public string Sensor = string.Empty;
        [KSPField] public float Attenuation = 0.5f;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if(!string.IsNullOrEmpty(Sensor))
            {
                var sensor = part.FindModelComponent<MeshFilter>(Sensor);
                if(sensor != null)
                {
                    sensor.AddCollider(true);
                    var damper = sensor.gameObject.AddComponent<Damper>();
                    damper.K = 1-Utils.Clamp(Attenuation, 0, 0.999f);
                }
            }
        }
  
        class Damper : MonoBehaviour
        {
            public float K;

            void OnTriggerStay(Collider col)
            {
                if(col != null && col.attachedRigidbody != null)
                {
                    if(col.CompareTag("Untagged"))
                    {
                        var p = col.attachedRigidbody.GetComponent<Part>();
                        if(p != null && p.vessel != null && p.vessel.loaded)
                        {
                            var r = p.Rigidbody;
                            if(r != null)
                            {
                                r.angularVelocity *= K;
                                r.velocity *= K;
                            }
                        }
                    }
                }
            }
        }
    }
}