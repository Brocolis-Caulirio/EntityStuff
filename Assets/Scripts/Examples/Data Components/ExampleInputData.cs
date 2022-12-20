using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ExampleInputData : IComponentData
{
    public KeyCode UpKey;
    public KeyCode DownKey;
    public KeyCode LeftKey;
    public KeyCode RightKey;
}
