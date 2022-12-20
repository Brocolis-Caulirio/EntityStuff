using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Mathematics;
using UnityEngine;

[AddComponentMenu("DOTS/vsTriggerSystem/EnemyTriggerData")]
[GenerateAuthoringComponent]
public struct EnemyTriggerData : IComponentData 
{
    public float2 Target;
}

public class vsEnemyTriggerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float2 Target;

    void OnEnable() { }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (enabled) 
        {
            dstManager.AddComponentData(entity, new EnemyTriggerData()
            {
                Target = Target
            });
        }
    }
}

/// <summary>
/// this sets the target of a enemy that enters a trigger event with another dynamic body
/// </summary>
public partial class EnemyTriggerSystem : SystemBase
{

    BuildPhysicsWorld m_BuildPhysicsWorld;
    StepPhysicsWorld m_StepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    EntityQuery TriggerGroup;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        TriggerGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] {typeof(EnemyTriggerData), }
        });
    }

    [BurstCompile]
    struct EnemyTriggerJob : ITriggerEventsJob
    {
        
        [ReadOnly] public ComponentDataFromEntity<vsPlayerVariables> AllPlayers;
        [ReadOnly] public ComponentDataFromEntity<Translation> AllTranslations;
        public ComponentDataFromEntity<vsEnemyVariables> AllEnemies;
        public EntityCommandBuffer entityCommandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {

            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            bool isBodyATrigger = AllEnemies.HasComponent(entityA);
            bool isBodyBTrigger = AllEnemies.HasComponent(entityB);
            bool APlayer = AllPlayers.HasComponent(entityA);
            bool BPlayer = AllPlayers.HasComponent(entityB);

            if ((!isBodyATrigger && !isBodyBTrigger) || (APlayer ? false : BPlayer ? false : true))
                return;
            //else
                //entityCommandBuffer.DestroyEntity(isBodyATrigger ? entityB : entityA);

            var enemy = isBodyATrigger ? entityA : entityB;
            var triggerBody = isBodyATrigger ? entityB : entityA;

            var triggerEnemyComponent = AllTranslations[triggerBody];

            var first = AllEnemies[enemy];
            AllEnemies[enemy] = new vsEnemyVariables
            {
                moving = true,
                playerDistance = first.playerDistance,
                cluster = first.cluster,
                health = first.health,
                conversion = first.conversion,
                target = triggerEnemyComponent.Value
            };

            if (isBodyATrigger && isBodyBTrigger)
            {
                var second = AllEnemies[triggerBody];
                AllEnemies[triggerBody] = new vsEnemyVariables
                {
                    moving = true,
                    playerDistance = second.playerDistance,
                    cluster = second.cluster,
                    health = second.health,
                    conversion = second.conversion,
                    target = AllTranslations[enemy].Value                    
                };
            }

        }
    }

    protected override void OnUpdate()
    {

        if (TriggerGroup.IsEmpty)
            return;

        var job = new EnemyTriggerJob
        {
            AllEnemies = GetComponentDataFromEntity<vsEnemyVariables>(),
            AllPlayers = GetComponentDataFromEntity<vsPlayerVariables>(),
            AllTranslations = GetComponentDataFromEntity<Translation>(),
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer()
        };
        Dependency = job.Schedule(m_StepPhysicsWorld.Simulation, Dependency);
        commandBufferSystem.AddJobHandleForProducer(Dependency);

    }

}