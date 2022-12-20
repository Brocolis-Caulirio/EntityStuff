using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

[DisallowMultipleComponent]
[AddComponentMenu("DOTS/vsAntisocialSocialMonsters/Entity Enemy Spawner")]
//IDeclareReferencedPrefabs is very important here zz
public class vsEntityEnemySpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{

    public GameObject[] Prefabs;
    public int[] DifficultyChangePrefabIndex;
    public float SpawnFrequency;
    public float2 SpawnDistanceMinMax;

    // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        foreach(GameObject p in Prefabs)
            referencedPrefabs.Add(p);
        //referencedPrefabs.Add(test);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

        var buffer = dstManager.AddBuffer<vsPrefabBuffer>(entity);
        foreach (GameObject e in Prefabs)
            buffer.Add(conversionSystem.GetPrimaryEntity(e));
        //buffer.Add(conversionSystem.GetPrimaryEntity(test));

        var iBuffer = dstManager.AddBuffer<vsDifficultyChangePrefabIndexBuffer>(entity);
        foreach (int i in DifficultyChangePrefabIndex) 
        {
            if(i < Prefabs.Length)
                iBuffer.Add(i);
        }            

        dstManager.AddComponentData(entity, new vsEntityEnemySpawnerData
        {
            Difficulty = 0f,
            SpawnFrequency = SpawnFrequency
        });
        dstManager.AddComponentData(entity, new vsEntityEnemySpawnerVariables
        {
            lastSpawn = 0,
            distance = SpawnDistanceMinMax,
            spawnFrequencyMultiplier = 1f
        });


    }

}
