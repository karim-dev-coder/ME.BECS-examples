using UnityEngine;
using ME.BECS;
using ME.BECS.Network;
using ME.BECS.Network.Markers;

namespace Demo
{
    public class DemoInitializer : NetworkWorldInitializer
    {
        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse");
                this.world.SendNetworkEvent(new MoveTo()
                {
                    Value = 100
                }, SpawnDemoPhysicsSystem.MoveTo);
            }
        }
    }
}