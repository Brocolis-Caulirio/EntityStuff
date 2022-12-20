using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct vsEnemyData : IComponentData // reference static values
{

    public double idleTime;         // how long to do something while sitting still
    public float speed;             // how fast it moves
    public float followThreshold;   // distance to start following entity, 2/3 value for enemy

    public float damage;            // how much it affects players

    public float baseQuantity;      // for merged enemies

    public float baseHP;            // health points 
    public float baseConversion;    // necessary conversion for this enemy

    public float noise;             // to add to monster's range

}
