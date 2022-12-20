using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
[AddComponentMenu("DOTS/vsAntisocialSocialMonsters/Prefab Enemy Spawner")]
[DisallowMultipleComponent]
public class vsPrefabEnemySpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    // Add fields to your component here. Remember that:
    //
    // * The purpose of this class is to store data for authoring purposes - it is not for use while the game is
    //   running.
    // 
    // * Traditional Unity serialization rules apply: fields must be public or marked with [SerializeField], and
    //   must be one of the supported types.
    //
    // For example,
    //    public float scale;
    public GameObject[] Prefabs;
    public int SpawnCount;
    public float SpawnsPerSecond;
    

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

        dstManager.AddComponentData(entity, new vsEnemySpawner 
        {
            spawnCount = SpawnCount,
            spawnsPerSecond = SpawnsPerSecond,
        });
        var buffer = dstManager.AddBuffer<vsPrefabSpawnerBuffer>(entity);
        foreach (var p in Prefabs) 
        {
            buffer.Add( new vsPrefabSpawnerBuffer { Value = new EntityPrefabReference(p) });
        }

    }
}
#endif