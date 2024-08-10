using AOT;
using ME.BECS;
using ME.BECS.Addons.Physics.Runtime.Helpers;
using ME.BECS.Network;
using ME.BECS.Physics.Components;
using ME.BECS.Transforms;
using ME.BECS.Views;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Material = Unity.Physics.Material;

namespace Demo
{
    public struct MoveTo : IComponent
    {
        public int Value;
    }

    [BurstCompile]
    public struct SpawnDemoPhysicsSystem : IAwake, IUpdate
    {
        public View cubePrefab;
        public View planePrefab;

        private float spawnInterval;
        private float tillNextSpawn;

        [NetworkMethod]
        [MonoPInvokeCallback(typeof(NetworkMethodDelegate))]
        public static void MoveTo(in InputData data, ref SystemContext context)
        {
            var inp = data.GetData<MoveTo>();
            Debug.Log(inp.Value);
        }

        public void OnAwake(ref SystemContext context)
        {
            var rot = quaternion.Euler(45f, 45f, 45f);

            // this.SpawnCube(ref context, new float3(0f, 2f, 0f), float3.zero, rot, 1f);

            for (int i = 0; i < 10; i++)
            {
                this.SpawnCube(ref context, new float3(0f, 3f, 0f), UnityEngine.Random.insideUnitSphere, rot, 1f);
            }

            this.spawnInterval = 0.1f;
            this.tillNextSpawn = 0f;

            this.SpawnPlane(ref context);
        }

        private void SpawnCube(ref SystemContext context, float3 position, float3 velocity, quaternion rotation, float gravityFactor)
        {
            var ent = Ent.New(context.world);
            ent.GetOrCreateAspect<TransformAspect>();

            ent.Set(new LocalPositionComponent()
            {
                value = position,
            });

            ent.Set(new LocalRotationComponent()
            {
                value = rotation,
            });

            ent.Set(LocalScaleComponent.Default);

            ent.Set(new PhysicsVelocityBecs()
            {
                Angular = velocity,
                Linear = float3.zero,
            });

            ent.Set(new PhysicsConstraintPositionBecs()
            {
                freezeX = true,
            });

            ent.Set(new PhysicsColliderBecs()
            {
                Value = BoxColliderHelper.Create(in ent, new BoxGeometry()
                {
                    Center = float3.zero,
                    Orientation = quaternion.identity,
                    Size = new float3(1f, 1f, 1f),
                    BevelRadius = 0f,
                }, CollisionFilter.Default, Material.Default),
            });

            ent.Set(new PhysicsGravityFactorBecs()
            {
                Value = gravityFactor,
            });

            var massProperties = MassProperties.CreateBox(new float3(1f, 1f, 1f));
            // Lock rotation in recommended way
            massProperties.MassDistribution.InertiaTensor = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var mass = PhysicsMassBecs.CreateDynamic(massProperties, 1);
            mass.InverseInertia = float3.zero;
            ent.Set(mass);

            // ent.Set(new IsPhysicsStatic());

            ent.InstantiateView(this.cubePrefab);
        }

        private void SpawnPlane(ref SystemContext context)
        {
            var ent = Ent.New(context.world);
            ent.GetOrCreateAspect<TransformAspect>();

            ent.Set(LocalScaleComponent.Default);

            ent.Set(new PhysicsColliderBecs()
            {
                Value = BoxColliderHelper.Create(in ent, new BoxGeometry()
                {
                    Center = float3.zero,
                    Orientation = quaternion.identity,
                    Size = new float3(100f, 0.1f, 100f),
                    BevelRadius = 0f,
                }, CollisionFilter.Default, Material.Default)
            });

            ent.Set(new LocalPositionComponent()
            {
                value = new float3(0f, -5f, 0f),
            });

            ent.Set(new IsPhysicsStaticEcs());

            ent.InstantiateView(this.planePrefab);
        }

        public void OnUpdate(ref SystemContext context)
        {
            return;

            this.tillNextSpawn -= context.deltaTime;
            if (this.tillNextSpawn > 0) return;
            this.tillNextSpawn = this.spawnInterval;

            this.SpawnCube(ref context, new float3(0f, 3f, 0f), UnityEngine.Random.insideUnitSphere, quaternion.identity, 1f);
        }
    }
}