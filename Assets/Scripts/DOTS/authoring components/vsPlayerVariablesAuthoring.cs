using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
public class vsPlayerVariablesAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public vsPlayerData data;

    [Header("Player Data")]
    [Header("Controls")]
    public KeyCode Up;
    public KeyCode Down;
    public KeyCode Left;
    public KeyCode Right;
    public KeyCode Dash;

    [Space(5)]
    [Header("Stats")]
    public float DashCd;
    public float DashDuration;
    public float DashMultiplier;

    public float MaxHealth;
    public float MaxSanity;
    public float BaseNoiseGen;
    public float speed;
    public float range;

    [Space(5)]
    [Header("centralized information")]
    public float CollisionDistance;


    [Space(10)]
    [Header("Player Variables")]
    [Header("movement")]
    public float2 movement;
    public bool moving;
    public bool dashing;
    public bool dashCD;
    public double lastDash;

    [Space(5)]
    [Header("information")]
    public float3 position;

    [Space(5)]
    [Header("invincibility")]
    public bool invincible;
    public float iFrames;
    public float iStart;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

        dstManager.AddComponentData(entity, new vsPlayerData
        {
            Up = Up,
            Down = Down,
            Left = Left,
            Right = Right,
            Dash = Dash,

            DashCd = DashCd,
            DashDuration = DashDuration,
            DashMultiplier = DashMultiplier,

            MaxHealth = MaxHealth,
            MaxSanity = MaxSanity,
            NoiseGen = BaseNoiseGen,
            Speed = speed,
            Range = range,

            CollisionDistance = CollisionDistance
        });
        dstManager.AddComponentData(entity, new vsPlayerVariables
        {
            movement = movement,
            moving = moving,
            dashing = dashing,
            dashCD = dashCD,
            lastDash = lastDash,

            position = position,

            health = MaxHealth,
            sanity = MaxSanity,

            invincible = invincible,
            iFrames = iFrames,
            iStart = iStart,

        });
        dstManager.AddComponentData(entity, new vsPlayerInteractionData 
        {
            Targetting = false,
            Colliding = false,
        });
        //dstManager.AddBuffer<vsEnemyCollisionBuffer>(entity);
        dstManager.AddBuffer<vsEnemyTargetBuffer>(entity);
        //dstManager.AddBuffer<vsEnemyAggressor>(entity);

    }
}
#endif