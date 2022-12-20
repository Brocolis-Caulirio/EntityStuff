using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class vsCameraBehaviour : MonoBehaviour
{
    //TODO:
    /*
     * MAKE A PLAYER HANDLING SCRIPT SO ITS MORE CENTRALIZED
     */

    //instance
    public static vsCameraBehaviour instance;

    //commom variables
    public Entity entityToFollow;
    public float3 offset;
    public float speed;
    private EntityManager eManager;
    private Rigidbody rb;

    //info between updates
    Translation entPos;
    vsPlayerVariables player;
    vsPlayerData pData;
    Vector3 targPos;

    #region Setup functions
    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- // // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- //

    private bool UpdateEntity()
    {

        if (eManager.Exists(entityToFollow))
            return true;
        else
            Debug.Log("entity does not exist, getting player entity");

        EntityQueryDescBuilder builder = new EntityQueryDescBuilder(Unity.Collections.Allocator.Temp);
        builder.AddAll(typeof(Translation));
        builder.AddAll(typeof(vsPlayerVariables));
        builder.FinalizeQuery();
        var mQuery = eManager.CreateEntityQuery(builder);
        Debug.Log("found " + mQuery.CalculateEntityCount() + " player entities");

        if (mQuery.CalculateEntityCount() <= 0)
        {
            Debug.LogError("no player entity found");
            return false;
        }

        entityToFollow = mQuery.ToEntityArray(Unity.Collections.Allocator.Temp)[0];
        return true;

    }

    private void UpdateEntity(bool NoReturnType)
    {

        if (entityToFollow == null)
        {
            EntityQueryDescBuilder builder = new EntityQueryDescBuilder(Unity.Collections.Allocator.Temp);
            builder.AddAll(typeof(Translation));
            builder.AddAll(typeof(vsPlayerVariables));
            builder.FinalizeQuery();
            var mQuery = eManager.CreateEntityQuery(builder);
            entityToFollow = mQuery.ToEntityArray(Unity.Collections.Allocator.Temp)[0];
            //transToFollow = mQuery.ToComponentDataArray<Translation>(Unity.Collections.Allocator.Temp)[0];

            Debug.Log("finished query");
            Debug.Log("query results numbers are " + mQuery.CalculateEntityCount());

            if (entityToFollow == null)
            {
                Debug.LogError("null entity at " + transform.name);
            }
        }

    }

    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- // // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- //
    #endregion
    #region Public convinience functions
    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- // // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- //

    public vsPlayerVariables GetPlayerVar()
    {
        UpdateEntity();
        return eManager.GetComponentData<vsPlayerVariables>(entityToFollow);
    }

    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- // // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- //
    #endregion
    #region Unity Functions
    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- // // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- //

    private void Awake()
    {
        eManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (instance != null) 
        {
            Debug.LogError("TWO INSTANCES OF VSCAMERABEHAVIOUR AT " + transform.name + " AND " + instance.transform.name);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void LateUpdate()
    {
        if (!UpdateEntity())
            return;

        entPos = eManager.GetComponentData<Translation>(entityToFollow);
        player = eManager.GetComponentData<vsPlayerVariables>(entityToFollow);
        pData = eManager.GetComponentData<vsPlayerData>(entityToFollow);
        float3 m = new float3(player.movement.x, 0f, player.movement.y);
        targPos = (Vector3)(entPos.Value); //+m * multiplier of some sorts
        targPos = targPos + (Vector3)offset;
        
        hpBar.instance.SetMHP(pData.MaxHealth);
        hpBar.instance.SetCHP(player.health);
        MapSystem.instance.PlayerPos = (Vector3)entPos.Value;

    }
    public void FixedUpdate()
    {
        Vector3 dir = (targPos - transform.position);
        float mul = Mathf.Max(0, Mathf.Min(1, dir.magnitude));
        rb.velocity = dir.normalized * (speed + pData.Speed) * mul;
    }

    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- // // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=- //
    #endregion

}


