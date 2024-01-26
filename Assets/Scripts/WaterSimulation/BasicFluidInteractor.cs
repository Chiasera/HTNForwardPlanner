using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Fluid {
    [AddComponentMenu("FusionWater/BasicFluidInteractor")]
    public class BasicFluidInteractor : BaseFluidInteractor
    {
        public override void FluidUpdate()
        {
            var XZ_position = new Vector2(transform.position.x, transform.position.z);
            float difference = transform.position.y - fluid.GetWaterHeight(XZ_position);// the height of the water at the position of contact*/

            if (difference < 0)
            {
                Vector3 buoyancy = Vector3.up * floatStrength * Mathf.Abs(difference) * Physics.gravity.magnitude * volume * fluid.density;

                if (simulateWaterTurbulence)
                {
                    var drift = GenerateDrift();
                    buoyancy += GenerateTurbulence();
                    rb.AddTorque(GenerateTurbulence() * 0.5f);
                    rb.position += new Vector3(drift.x, 0, drift.y);
                }
                rb.AddForceAtPosition(buoyancy, transform.position, ForceMode.Force);
                rb.AddForceAtPosition(-rb.velocity * dampeningFactor * volume, transform.position, ForceMode.Force);
            }
        }
    }
}