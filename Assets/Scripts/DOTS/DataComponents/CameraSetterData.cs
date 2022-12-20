using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[AddComponentMenu("DOTS/vsAntisocialSocialMonsters/Camera Setter")]
[GenerateAuthoringComponent]
public struct CameraSetter : IComponentData
{
    public float3 camBhv;
}
