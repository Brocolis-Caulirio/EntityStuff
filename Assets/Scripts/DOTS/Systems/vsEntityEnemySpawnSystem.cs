using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial class vsEntityEnemySpawnSystem : SystemBase
{

    private Random m_random;
    private BeginSimulationEntityCommandBufferSystem m_ecbs;
    private EntityQuery playerQuery;

    protected override void OnStartRunning()
    {

        m_random.InitState(6283);
        m_ecbs = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        

    }

    protected override void OnCreate() 
    {

        playerQuery = GetEntityQuery(new EntityQueryDesc 
        {
            All = new ComponentType[] 
            {
                typeof(vsPlayerVariables), 
                typeof(vsPlayerData), 
                typeof(Translation), 
                typeof(LocalToWorld) 
            }
        });

    }

    protected override void OnUpdate()
    {
        var et = Time.ElapsedTime;
        var player = playerQuery.GetSingletonEntity();
        var pPos = playerQuery.GetSingleton<Translation>();

        Entities.WithStructuralChanges().ForEach(
        (
            Entity entity, int entityInQueryIndex,
            DynamicBuffer<vsPrefabBuffer> pBuffer, DynamicBuffer<vsDifficultyChangePrefabIndexBuffer> iBuffer,
            ref vsEntityEnemySpawnerVariables variables, in vsEntityEnemySpawnerData data
        ) => {


            int p = m_random.NextInt(0, pBuffer.Length);
            var prefab = pBuffer[p];

            bool condition = et - variables.lastSpawn > (data.SpawnFrequency * variables.spawnFrequencyMultiplier);
            if (condition)
            {

                var newCapsule = EntityManager.Instantiate(prefab);

                float dist = m_random.NextFloat(variables.distance.x, variables.distance.y);
                float angle = m_random.NextFloat(0f, 360f);
                var newPos = pPos.Value + GetPos(dist, angle);

                EntityManager.SetComponentData(newCapsule, new Translation { Value = newPos });
                variables.lastSpawn = et;
                Debug.Break();

            }


        }).Run();

    }

    float3 GetPos(float dist, float angle) 
    {

        angle = math.radians(angle);

        float x = math.cos(angle) * dist;
        float z = math.sin(angle) * dist;

        return  new float3(x, 0f, z);

    }

}
public struct vsEntityEnemySpawnerData : IComponentData 
{

    public float Difficulty;
    public float SpawnFrequency;

}
public struct vsEntityEnemySpawnerVariables : IComponentData
{
    
    public double lastSpawn;
    public float2 distance;
    public float spawnFrequencyMultiplier;

}
public struct vsPrefabBuffer : IBufferElementData 
{

    public static implicit operator Entity(vsPrefabBuffer e) { return e.Value; }
    public static implicit operator vsPrefabBuffer(Entity e) { return new vsPrefabBuffer { Value = e }; }

    public Entity Value;

}
public struct vsDifficultyChangePrefabIndexBuffer : IBufferElementData
{

    public static implicit operator int(vsDifficultyChangePrefabIndexBuffer e) { return e.Value; }
    public static implicit operator vsDifficultyChangePrefabIndexBuffer(int e) 
    { return new vsDifficultyChangePrefabIndexBuffer { Value = e }; }

    public int Value;

}

//refs
/*
 * some links I used to come up with the solutions in here
 * https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/ECSSamples/Assets/HelloCube/5.%20SpawnFromEntity/SpawnerSystem_FromEntity.cs
 * https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/ECSSamples/Assets/HelloCube/5.%20SpawnFromEntity/SpawnerAuthoring_FromEntity.cs
 * https://github.com/Unity-Technologies/EntityComponentSystemSamples/tree/master/ECSSamples/Assets/HelloCube/5.%20SpawnFromEntity
 * https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/ECSSamples/Assets/Advanced/EntityPrefab/PrefabSpawnerSystem.cs
 * https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/ECSSamples/Assets/Advanced/EntityPrefab/PrefabSpawnerAuthoring.cs
 * https://gist.github.com/JohnnyTurbo/dc2d4896488ed3e92a61e690d031af14
 */
