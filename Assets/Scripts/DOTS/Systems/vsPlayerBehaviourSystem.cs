using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

public partial class vsPlayerBehaviourSystem : SystemBase
{

    EntityQuery EnemyQuery;
    EntityQuery PlayerQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        EnemyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] {typeof(vsEnemyVariables), typeof(vsEnemyData), typeof(Translation), typeof(LocalToWorld)}
        });
        PlayerQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] {typeof(vsPlayerVariables), typeof(vsPlayerData), typeof(vsPlayerInteractionData)}
        });
    }
    protected override void OnUpdate()
    {

        float dt = Time.DeltaTime;
        float eTime = (float)Time.ElapsedTime;
        var EM = EntityManager;

        bool tookDamage = false;

        vsEnemyVariables aggressor = new vsEnemyVariables();        
        var eVariables = EnemyQuery.ToComponentDataArray<vsEnemyVariables>(Allocator.Temp);
        var eTranslations = EnemyQuery.ToComponentDataArray<vsEnemyVariables>(Allocator.Temp);
        var eEntities = EnemyQuery.ToEntityArray(Allocator.Temp);


        Entity Player = PlayerQuery.ToEntityArray(Allocator.Temp)[0];



        Entities
            .WithAll<Translation, vsPlayerData, vsPlayerVariables>()
            .ForEach((Entity e, ref vsPlayerVariables player, ref Translation trans, ref Rotation rot, in vsPlayerData data, in LocalToWorld ltw) =>
        {

            float rSpeed = data.Speed * (player.dashing ? data.DashMultiplier : 1f);
            if (player.moving) 
            {
                //float2 mov = math.normalize(player.movement);
                float3 mov = new float3(player.movement.x, 0, player.movement.y);

                quaternion targetRot = quaternion.LookRotation(math.normalize(mov), new float3(0, 1, 0));
                rot.Value = math.slerp(rot.Value, targetRot, dt * data.Speed * data.Speed);


                trans.Value += ltw.Forward * rSpeed * dt;
                player.position = trans.Value;

                
            }

            

        }).Run();

        var InteractionDetection = new EnemyInteractionDetection
        {
            entities = new NativeArray<Entity>(eEntities, Allocator.TempJob),
            LTW = EnemyQuery.ToComponentDataArray<LocalToWorld>(eEntities, Allocator.TempJob),
            eVariables = new NativeArray<vsEnemyVariables>(eVariables, Allocator.TempJob),
            Player = Player,
            //collisions = EM.GetBuffer<vsEnemyCollisionBuffer>(Player),
            targets = EM.GetBuffer<vsEnemyTargetBuffer>(Player),
            //em = EM
        }.Schedule(PlayerQuery);

        InteractionDetection.Complete();
        var InteractionHandler = new EnemyInteractionHandler
        {
            time = eTime,
            Player = Player,
            eVariables = new NativeArray<vsEnemyVariables>(eVariables, Allocator.TempJob),
            targets = EM.GetBuffer<vsEnemyTargetBuffer>(Player),
            //em = EM
        }.Schedule(PlayerQuery);

        //deprecated, for reference only
        /* 
        var hitJob = new TakeDamageJob
        {
            time = eTime,
            aggressor = aggressor,
            tookDamage = tookDamage
        }.Schedule();
        */

    }

}


#region thread jobs
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= // // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= //

[BurstCompile]
partial struct EnemyInteractionDetection : IJobEntity 
{

    [DeallocateOnJobCompletionAttribute]
    [ReadOnly] public NativeArray<Entity> entities;
    [DeallocateOnJobCompletionAttribute]
    [ReadOnly] public NativeArray<LocalToWorld> LTW;
    [DeallocateOnJobCompletionAttribute]
    [ReadOnly] public NativeArray<vsEnemyVariables> eVariables;

    [ReadOnly] public Entity Player;
    //[ReadOnly] public EntityManager em;

    //public DynamicBuffer<vsEnemyCollisionBuffer> collisions;
    public DynamicBuffer<vsEnemyTargetBuffer> targets;

    public void Execute(ref vsPlayerInteractionData interactions, in vsPlayerVariables player, in vsPlayerData data) 
    {

         //= em.GetBuffer<vsEnemyCollisionBuffer>(Player);
        //collisions.Clear();

         //= em.GetBuffer<vsEnemyCollisionBuffer>(Player);
        targets.Clear();

        //var entities = Enemies.ToEntityArray(Allocator.Temp);

        //this sets the range for attack and range for getting hit to false as a 'reset'
        interactions.Targetting = false;
        interactions.Colliding  = false;

        int targetCount = 0;

        for (int i = 0; i < entities.Length; i++) 
        {

            LocalToWorld ltw = LTW[i];//em.GetComponentData<LocalToWorld>(entities[i]);
            float3 direction = ltw.Position - player.position;
            float distance = math.length(direction);

            if (distance < data.CollisionDistance)
            {
                //adds the information that the player is in contact with an enemy
                interactions.Colliding = true; 
            }

            if (distance < data.Range) 
            {
                //adds the information that the player is in range for attacking an enemy
                interactions.Targetting = true;
                //adds the enemy's information to the buffer
                if (targetCount < targets.Capacity) 
                {
                    targets.Add(eVariables[i]);
                    targetCount++;
                }
            }

        }

        //entities.Dispose();
        //LTW.Dispose();
        //eVariables.Dispose();
    }

}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= //

[BurstCompile]
partial struct EnemyInteractionHandler : IJobEntity
{

    [ReadOnly] public float time;
    [ReadOnly] public Entity Player;

    [DeallocateOnJobCompletion]
    [ReadOnly] public NativeArray<vsEnemyVariables> eVariables;

    //DynamicBuffer<vsEnemyCollisionBuffer> collisions;
    [ReadOnly] public DynamicBuffer<vsEnemyTargetBuffer> targets;
    //DynamicBuffer<vsEnemyAggressor> Aggressor;

    public void Execute(ref vsPlayerVariables player, ref vsPlayerInteractionData interactions, in vsPlayerData data) 
    {
        

        //Collisiong Handler
        //Sets Colliding true in interactions and adds a buffer of vsEnemyAggressor to the player
        if (player.invincible)
        {
            player.invincible = time - player.iStart > player.iFrames ? false : player.invincible;
        }
        else if (interactions.Colliding )
        {
            //Aggressor.Clear();
            vsEnemyVariables aggressor = new vsEnemyVariables();

            float HighestDamage = 0;
            int iterator = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                float damage = eVariables[i].damage;
                HighestDamage = damage > HighestDamage ? damage : HighestDamage;
                aggressor = damage > HighestDamage ? targets[i] : aggressor;
                iterator = damage > HighestDamage ? i : iterator;
            }

            interactions.Damage = aggressor.damage;
            interactions.EnemyBufferIdentifier = iterator;

            player.health -= HighestDamage;
            player.invincible = true;
            player.iStart = time;

        }

        //eVariables.Dispose();

        //Target Handler

    }

}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= //

//deprecated, for reference only
[BurstCompile]
partial struct TakeDamageJob : IJobEntity 
{
    [ReadOnly] public float time;
    [ReadOnly] public vsEnemyVariables aggressor;
    [ReadOnly] public bool tookDamage;

    public void Execute(ref vsPlayerVariables player, in vsPlayerData data) 
    {
        
    }

}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= // // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-= //
#endregion