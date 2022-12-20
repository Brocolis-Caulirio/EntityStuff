using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct vsPlayerInteractionData : IComponentData
{

    #region ENEMY INTERACTIONS
    [Header("Enemy interactions")]
    [Space(10)]

    [Header("Attack interactions")]
    public bool Targetting;
    [Space(5)]

    [Header("Damaged interactions")]
    public bool Colliding;
    public float Damage;
    public int EnemyBufferIdentifier;


    #endregion


}

#region Tags
[GenerateAuthoringComponent]
public struct vsPlayerProjectileTag : IComponentData
{
}
#endregion

#region Dynamic Buffers

//limits to 10 entities being interacted with simultaneously
/*
[InternalBufferCapacity(10)]
[GenerateAuthoringComponent]
public struct vsEnemyCollisionBuffer : IBufferElementData
{

    public static implicit operator Entity(vsEnemyCollisionBuffer e) { return e.Value; }
    public static implicit operator vsEnemyCollisionBuffer(Entity e) { return new vsEnemyCollisionBuffer { Value = e }; }

    public Entity Value;
}
*/
//limits to 10 entities being targeted with simultaneously
[InternalBufferCapacity(10)]
[GenerateAuthoringComponent]
public struct vsEnemyTargetBuffer : IBufferElementData
{

    public static implicit operator vsEnemyVariables(vsEnemyTargetBuffer e) { return e.Value; }
    public static implicit operator vsEnemyTargetBuffer(vsEnemyVariables e) { return new vsEnemyTargetBuffer { Value = e }; }

    public vsEnemyVariables Value;
}
/*
public struct vsEnemyAggressor : IBufferElementData 
{
    public static implicit operator Entity(vsEnemyAggressor e) { return e.Value; }
    public static implicit operator vsEnemyAggressor(Entity e) { return new vsEnemyAggressor { Value = e }; }

    public Entity Value;
}*/

public struct vsPlayerCooldown :IBufferElementData
{
    public static implicit operator float(vsPlayerCooldown e) { return e.Value; }
    public static implicit operator vsPlayerCooldown(float e) { return new vsPlayerCooldown { Value = e }; }

    public float Value;
}
#endregion



