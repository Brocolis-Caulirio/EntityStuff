using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using System;
using Unity.Scenes;
using Random = Unity.Mathematics.Random;


public partial struct vsEnemySpawner : IComponentData 
{
    public double lastSpawn;
    public float spawnCount;
    public float spawnsPerSecond;
}
public struct vsPrefabSpawnerBuffer : IBufferElementData 
{
    public static implicit operator EntityPrefabReference(vsPrefabSpawnerBuffer e) { return e.Value; }
    public static implicit operator vsPrefabSpawnerBuffer(EntityPrefabReference e) { return new vsPrefabSpawnerBuffer { Value = e}; }

    public EntityPrefabReference Value;
}

public partial class vsPrefabEnemySpawnSystem : SystemBase
{

    private BeginSimulationEntityCommandBufferSystem m_bSimECBS;

    protected override void OnCreate()
    {
        m_bSimECBS = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {

        var ecb = m_bSimECBS.CreateCommandBuffer().AsParallelWriter();
        var rnd = new Random((uint)Environment.TickCount);
        
        //load prefab
        Entities.WithNone<RequestEntityPrefabLoaded>().ForEach((
            Entity entity, int entityInQueryIndex,
            ref vsEnemySpawner spawner, in DynamicBuffer<vsPrefabSpawnerBuffer> prefabs) => 
        {
            ecb.AddComponent(entityInQueryIndex, entity,
                new RequestEntityPrefabLoaded { Prefab = prefabs[rnd.NextInt(prefabs.Length)] });
        }).ScheduleParallel();

        var dt = Time.DeltaTime;

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref vsEnemySpawner spawner, in PrefabLoadResult prefab) =>
        {

            var remaining = spawner.spawnCount;
            var newRemaining = remaining - dt * spawner.spawnsPerSecond;
            var spawnCount = (int)remaining - (int)newRemaining;

            for (int i = 0; i < spawnCount; ++i)
            {
                var instance = ecb.Instantiate(entityInQueryIndex, prefab.PrefabRoot);
                int index = i + (int)remaining;
                ecb.SetComponent(entityInQueryIndex, instance, new Translation { Value = new float3(index * ((index & 1) * 2 - 1), 0, 0) });
            }

            spawner.spawnCount = newRemaining;

        }).ScheduleParallel();

        m_bSimECBS.AddJobHandleForProducer(Dependency);

    }
}
