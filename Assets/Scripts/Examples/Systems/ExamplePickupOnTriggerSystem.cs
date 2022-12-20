using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Burst;
using Unity.Collections;

public partial class ExamplePickupOnTriggerSystem : SystemBase
{
    private StepPhysicsWorld stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var job = new PickupOnTriggerSystemJob
        {
            allPickups = GetComponentDataFromEntity<ExampleInputData>(true),
            allPlayers = GetComponentDataFromEntity<ExampleMoveData>(true),
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer()
        };
        Dependency = job.Schedule(stepPhysicsWorld.Simulation, Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }

    [BurstCompile]
    struct PickupOnTriggerSystemJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<ExampleInputData> allPickups;
        [ReadOnly] public ComponentDataFromEntity<ExampleMoveData> allPlayers;
        public EntityCommandBuffer entityCommandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;
            if (allPickups.HasComponent(entityA) && allPickups.HasComponent(entityB))
                return;

            if (allPickups.HasComponent(entityA) && allPlayers.HasComponent(entityB))
                entityCommandBuffer.DestroyEntity(entityA);
            else if (allPlayers.HasComponent(entityA) && allPickups.HasComponent(entityB))
                entityCommandBuffer.DestroyEntity(entityB);
        }
    }
}