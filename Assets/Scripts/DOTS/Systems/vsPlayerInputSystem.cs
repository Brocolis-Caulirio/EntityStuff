using Unity.Entities;
using Unity.Jobs;
//using Unity.Physics;
//using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

public partial class vsPlayerInputSystem : SystemBase
{

    protected override void OnCreate() 
    {
        //get the keybinds
    }

    protected override void OnUpdate()
    {

        double t = Time.ElapsedTime;

        Entities.WithAll<vsPlayerData, vsPlayerVariables>().ForEach((ref vsPlayerVariables player, in vsPlayerData data, in Translation trans) =>
        {

            player.movement = new float2(
                Input.GetKey(data.Right) ? 1 : Input.GetKey(data.Left) ? -1 : 0,
                Input.GetKey(data.Up) ? 1 : Input.GetKey(data.Down) ? -1 : 0);
            bool sDash = (!player.dashCD && Input.GetKey(data.Dash));
            player.moving = math.length(player.movement) > 0;

            if (sDash)
            {

                player.lastDash = t;
                player.dashCD = true;
                player.dashing = true;

            }
            else if (player.dashCD) 
            {
                player.dashCD = t - player.lastDash > data.DashCd ? false : player.dashCD;
            }

            player.dashing = t - player.lastDash > data.DashDuration ? false : player.dashing;
            player.position = trans.Value;// should be moved I think, but oh well


        }).Run();
    }

}
