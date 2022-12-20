using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct vsEnemyVariables : IComponentData //runtime values
{

    public double lastMove;
    public bool moving;
    public float3 position;

    public float playerDistance;    // how far from player
    public float cluster;           // how many entities are near this one, if lower than conversion > merge
    public float health;            // regards damage done by the player
    public float conversion;        // regards damage done by other monsters

    public float damage;

    public float3 target;
    //public Translation tRef;
    //public float3 m_pos;

}