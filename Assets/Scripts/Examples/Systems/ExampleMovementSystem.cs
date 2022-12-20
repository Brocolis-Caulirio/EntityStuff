using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

[BurstCompile]
partial struct ExampleEntityMoveJob : IJobEntity
{
    public float dt;
    //ref   variables first, being changed
    //in    variables last,  being only read
    public void Execute(ref Translation translation, in ExampleMoveData md)
    {
        translation.Value.x = math.clamp(translation.Value.x + (md.speed * md.horizontal * dt), -3, 3);
        translation.Value.z = math.clamp(translation.Value.z + (md.speed * md.vertical   * dt), -3, 3);
    }
}

//Sync before running, no need in SystemBase
//[AlwaysSynchronizeSystem]
//partial needed to be written to
public partial class ExampleMovementSystem : SystemBase
{
    
    //required override
    //can be of a type JobHandle to get and return JobHandles
    protected override void OnUpdate() 
    {

        float DeltaTime = Time.DeltaTime;

        var ExampleWorkerThreadJob = new ExampleEntityMoveJob()
        {
            dt = DeltaTime
        };
        ExampleWorkerThreadJob.Schedule();
        // running on main thread is more efficient than scheduling worker thread
        // if its a simple task, run profiler and see which way is more efficient

        #region main thread example
        /* 
        //ref   variables first, being changed
        //in    variables last,  being only read
        Entities.ForEach((ref Translation translation, in ExampleMoveData md) =>
        {
            translation.Value.x = math.clamp(translation.Value.x + (md.speed * md.horizontal * DeltaTime), -3, 3);
            translation.Value.y = math.clamp(translation.Value.y + (md.speed * md.vertical * DeltaTime), -3, 3);
        }
        ).Run();
        //.Run()        for Main
        //.Schedule()   for Worker Threads
        */
        #endregion

    }

}
