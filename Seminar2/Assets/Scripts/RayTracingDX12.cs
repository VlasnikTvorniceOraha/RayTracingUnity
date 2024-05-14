using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class RayTracingDX12 : MonoBehaviour
{
    RayTracingAccelerationStructure _AccelerationStructure;
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        var postavke = new RayTracingAccelerationStructure.RASSettings();
        postavke.layerMask = LayerMask.NameToLayer("RTDXLayer");
        postavke.managementMode = RayTracingAccelerationStructure.ManagementMode.Automatic;
        postavke.rayTracingModeMask = RayTracingAccelerationStructure.RayTracingModeMask.Everything;

        _AccelerationStructure = new RayTracingAccelerationStructure(postavke);
    }

    // Update is called once per frame
    void Update()
    {
        //update and make the geometry for raycasting
    
        _AccelerationStructure.Build();
    }
}
