using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[GenerateAuthoringComponent]
public struct vsPlayerData : IComponentData
{
    /*
     * stuff like range, attack and so on
     */
    [Header("inputs")]
    public KeyCode Up;
    public KeyCode Down;
    public KeyCode Left;
    public KeyCode Right;
    public KeyCode Dash;

    [Space(5)]
    [Header("dash variables")]
    public float DashCd;
    public float DashDuration;
    public float DashMultiplier;

    [Space(5)]
    [Header("base stats")]
    public float MaxHealth;
    public float MaxSanity;
    public float NoiseGen;
    public float Speed;
    public float Range;
    public float MaxTargets;

    [Space(5)]
    [Header("centralized information")]
    public float CollisionDistance;


}
