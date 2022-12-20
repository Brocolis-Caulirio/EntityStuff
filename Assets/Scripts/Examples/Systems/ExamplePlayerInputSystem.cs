using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//Sync before running, no need in SystemBase
//[AlwaysSynchronizeSystem]
//partial needed to be written to
public partial class ExamplePlayerInputSystem : SystemBase
{

    //required override
    //can be of a type JobHandle to get and return JobHandles
    protected override void OnUpdate()
    {

        //ref   variables first
        //in    variables last
        Entities/*.WithAll<ExampleMoveData>()*/.ForEach(( ref ExampleMoveData moveData, in ExampleInputData inputData) => 
        {
            moveData.horizontal = Input.GetKey(inputData.RightKey) ? 1 : Input.GetKey(inputData.LeftKey) ? -1 : 0;
            moveData.vertical   = Input.GetKey(inputData.UpKey) ? 1 : Input.GetKey(inputData.DownKey) ? -1 : 0;
        }
        ).Run();
        //.Run()        for Main
        //.Schedule()   for Worker Threads

        //throw new System.NotImplementedException();
    }

}
