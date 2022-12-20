using Unity.Entities;
using Unity.Jobs;
//using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
partial struct EnemyMoveJob : IJobEntity 
{
    public float dt;
    public void Execute(ref Translation translation, ref vsEnemyVariables enemyVar, ref Rotation rot, in vsEnemyData enemyData, in vsPlayerVariables player) 
    {

        //col.
        float angleR = math.atan2(enemyVar.target.x - translation.Value.x, enemyVar.target.z - translation.Value.z);
        float3 direction = math.normalize(enemyVar.target - translation.Value);
        translation.Value += enemyData.speed * direction * dt;
        rot.Value = math.mul(rot.Value, quaternion.RotateY(angleR * dt));

    }
}

public partial class vsEnemySystem : SystemBase
{

    #region variables
    ComponentDataFromEntity<vsEnemyVariables> AllEnemies;
    ComponentDataFromEntity<vsPlayerVariables> AllPlayers;
    ComponentDataFromEntity<Translation> AllTranslations;
    EntityQuery PlayerEntityQuery;
    EntityQuery EnemyEntityQuery;
    //NativeArray<float3> EnemyList;

    #endregion

    #region structs and functions

    public void ReloadEntityReference()
    {
        AllEnemies = GetComponentDataFromEntity<vsEnemyVariables>(true);
        AllPlayers = GetComponentDataFromEntity<vsPlayerVariables>(true);
        AllTranslations = GetComponentDataFromEntity<Translation>(true);
    }

    public struct GetNearest : IJob
    {
        [ReadOnly] public NativeArray<float3> Comparisson;
        [ReadOnly] public float3 Compared;
        public float3 value;
        public float distance;

        void IJob.Execute()
        {
            for (int iterator = 0; iterator < Comparisson.Length; iterator++) 
            {

                distance = iterator == 0 ? 9999f : distance;
                float d = math.abs( math.length(Compared - Comparisson[iterator]));
                
                value = d < distance ? Comparisson[iterator] : value;
                distance = d < distance ? d : distance;

            }
        }
    }

    #endregion

    private StepPhysicsWorld stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        //EnemyList = new NativeArray<float3>(new float3[0], Allocator.Persistent);
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        ReloadEntityReference();
        PlayerEntityQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Translation), typeof(vsPlayerVariables), typeof(vsPlayerData) }
        });
        EnemyEntityQuery = GetEntityQuery(new EntityQueryDesc 
        { 
            All = new ComponentType[] { typeof(Translation), typeof(vsEnemyData), typeof(vsEnemyVariables)}
        });
    }

    protected override void OnUpdate()
    {
        
        //setup for frequently used data and variables
        float dt = Time.DeltaTime;
        double elapsedTime = Time.ElapsedTime;
        //runs collisions
        var prevList = new NativeList<vsEnemyVariables>(Allocator.Temp);
        var enemyList = new NativeList<Translation>(Allocator.Temp);
        var PlayerTranslation = PlayerEntityQuery.ToComponentDataArray<Translation>(Allocator.Temp)[0];

        Entities.WithStoreEntityQueryInField(ref EnemyEntityQuery)/* .WithAll<Translation>().WithAny<vsEnemyData, vsPlayerData>()*/.ForEach((ref vsEnemyVariables e, in Translation trans) => {
            prevList.Add(e);
            e.position = trans.Value;
        }).Run();
        //Entities.WithStoreEntityQueryInField(ref EnemyEntityQuery).ForEach((in Translation trans) => {});

        //runs the behaviour for the enemies
        Entities.WithAll<vsEnemyData, vsEnemyVariables, Translation>().ForEach((ref Translation translation, ref Rotation rot, ref vsEnemyVariables enemyVariables, in vsEnemyData enemyData, in LocalToWorld ltw) => 
        {
            //setup
            enemyList.Add(translation);
            float angleR = math.atan2(enemyVariables.target.x - translation.Value.x, enemyVariables.target.z - translation.Value.z);

            //target finding
            float3 closestEnemy = translation.Value;
            float distanceEnemy = 0;
            for (int iterator = 0; iterator < prevList.Length; iterator++)
            {

                distanceEnemy = iterator == 0 ? 9999f : distanceEnemy;
                float d = math.abs(math.length(translation.Value - prevList[iterator].position));
                if (d > .125)
                {
                    closestEnemy = d < distanceEnemy ? prevList[iterator].position : closestEnemy;
                    distanceEnemy = d < distanceEnemy ? d : distanceEnemy;
                    //enemyVariables.tRef = d < distanceEnemy ? prevList[iterator] : enemyVariables.tRef;
                }

            } //gets distance and the target
            float distancePlayer = math.length(translation.Value - PlayerTranslation.Value);
            bool fPlayer = distancePlayer < enemyData.followThreshold;
            bool fEnemy = distanceEnemy < enemyData.followThreshold;

            enemyVariables.target = fPlayer ? PlayerTranslation.Value : fEnemy ? closestEnemy : enemyVariables.target;
            float distance = math.length(enemyVariables.target - translation.Value);

            bool idle = enemyVariables.moving;
            enemyVariables.moving = (distance > 0.625);
            double feTime = math.floor(elapsedTime);

            if (idle && !enemyVariables.moving)
            {
                enemyVariables.lastMove = elapsedTime;
            }
            else if (!enemyVariables.moving && elapsedTime - enemyVariables.lastMove > enemyData.idleTime)
            {   //as close to random as i bother to get lol
                enemyVariables.target = translation.Value + (new float3(feTime % 3 == 0 ? 1 : -1, 0, (feTime + 1) % 3 == 0 ? 1 : -1) * 3);
                enemyVariables.moving = true;
            }

            //movement
            if (enemyVariables.moving)
            {
                //float3 direction = math.normalize(enemyVariables.target - translation.Value);
                quaternion targetRot = quaternion.LookRotation(math.normalize(enemyVariables.target - translation.Value), new float3(0, 1, 0));
                rot.Value = math.slerp(rot.Value, targetRot, dt * enemyData.speed);

                translation.Value += enemyData.speed * math.normalize(ltw.Forward) * dt;
                
            }


        }
        ).Run();

        //EnemyList = enemyList.AsArray();

    }

}
