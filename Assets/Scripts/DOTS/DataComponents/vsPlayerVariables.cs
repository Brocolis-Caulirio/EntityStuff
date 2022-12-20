using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//[GenerateAuthoringComponent]
[AddComponentMenu("DOTS/vsAntisocialSocialMonsters/Player Components")]
public struct vsPlayerVariables : IComponentData
{
    /*
     * stuff like position for quick reference by enemies and other systems
     */
    //controls
    public float2 movement;
    public bool moving;
    public bool dashing;
    public bool dashCD;
    public double lastDash;

    //information
    public float3 position;

    //stats
    public float health;        // changes the spritesheet
    public float sanity;        // for distortions and stuff

    public bool invincible;
    public float iFrames;
    public float iStart;    

}

