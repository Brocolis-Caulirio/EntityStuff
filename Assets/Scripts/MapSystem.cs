using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSystem : MonoBehaviour
{

    public static MapSystem instance;

    [Space(5)]
    [Header("Map node system variables")]
    public Vector3 GridOffset;
    public int GridSquareSide;
    public float GridSpacing;
    public float DistanceForActivation;
    public Color[] ColorDifficultyProgression;
    public int NodesForProgression;
    public Object NodePrefab;

    [Header("Player information")]
    public Vector3 PlayerPos;

    [Space(5)]
    [Header("Internal information handling")]
    [SerializeField] private int nodeCount;
    [SerializeField] private int currentColor;
    [SerializeField] private GameObject[] sNodes;
    Dictionary<Vector3, Renderer> nodeLib;
    Dictionary<Renderer, bool> nodeCheck;

    [Space(5)]
    [Header("External information handling")]
    public Dictionary<Vector3, int> Difficulty;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("MORE THAN ONE INSTANCE AT " + instance.transform.name + " AND " + transform.name);
            return;
        }
        instance = this;
    }

    void Start()
    {

        //validates color progression
        if (ColorDifficultyProgression.Length <= 0)
        {
            Debug.LogError("NO NODES OR COLORS ASSIGNED AT " + transform.name);
            return;
        }

        //other setup
        nodeCount = 0;
        currentColor = 1;

        //instantiates ?
        nodeCheck = new Dictionary<Renderer, bool>();
        nodeLib = new Dictionary<Vector3, Renderer>();
        Difficulty = new Dictionary<Vector3, int>();

        //validates node progression
        NodesForProgression = NodesForProgression > 0 ? NodesForProgression : 5;

        //grid setup and node spawn
        int gSize = GridSquareSide * GridSquareSide;
        sNodes = new GameObject[gSize];
        for (int i = 0; i < gSize; i++)
        {
            sNodes[i] = Instantiate(NodePrefab, transform) as GameObject;
        }

        //nodes positioning
        int iterator = 0;
        for (int ix = 0; ix < GridSquareSide; ix++)
        {
            for (int iy = 0; iy < GridSquareSide; iy++)
            {
                //Debug.Log("setting the " + iterator + " node");
                Transform t = sNodes[iterator].transform;
                Renderer r = sNodes[iterator].GetComponent<Renderer>();
                r.SetPropertyBlock(GetColorSet());
                //Debug.Log("renderer is " + !(r == null));

                Vector3 gpos = transform.position +
                    new Vector3(ix * GridSpacing, -(float)iterator / gSize, iy * GridSpacing) - GridOffset;

                t.position = gpos;
                nodeCheck.Add(r, false);
                nodeLib.Add(t.position, r);
                Difficulty.Add(t.position, currentColor);
                iterator++;
            }
        }

    }

    void Update()
    {
        float dist;
        Vector3 c = GetClosest(PlayerPos, out dist);
        if (dist < DistanceForActivation)
        {
            if (UpdateNode(c))
            {
                nodeCount++;
                if (nodeCount % NodesForProgression == 0)
                    currentColor++;
            }
        }
    }

    MaterialPropertyBlock GetColorSet()
    {
        MaterialPropertyBlock o = new MaterialPropertyBlock();
        Color prev = ColorDifficultyProgression[currentColor - 1];
        prev.a = 0;
        o.SetColor("_ColorE", prev);
        o.SetColor("_ColorH", ColorDifficultyProgression[currentColor]);
        return o;
    }
    bool UpdateNode(Vector3 pos)
    {
        Renderer r = nodeLib[pos];//GetClosest(pos);
        if (r == null)
            return false;

        if (!nodeCheck[r])
        {
            r.SetPropertyBlock(GetColorSet());
            nodeCheck[r] = true;
            Difficulty[pos] = currentColor;
            return true;
        }
        else
            return false;
    }

    public Vector3 GetClosest(Vector3 pos, out float dist)
    {
        Vector3 index = pos / GridSpacing;
        Vector3 objective = new Vector3(Mathf.Round(index.x), Mathf.Round(index.y), Mathf.Round(index.z));
        objective *= GridSpacing;
        dist = (objective - pos).magnitude;
        return objective;
    }
    Renderer GetClosest(Vector3 pos)
    {
        float f;
        return nodeLib[GetClosest(pos, out f)];
    }

}
