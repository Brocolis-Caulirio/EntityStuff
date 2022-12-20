using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ExampleMoveData : IComponentData
{
    public int vertical;
    public int horizontal;
    public float speed;

}
