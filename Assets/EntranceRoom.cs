using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntranceRoom : MissionTerminal {
    public GameObject Floor;
    public GameObject Door;
    public GameObject Wall;
    public int TileSize;

    public override void Build(Vertex v)
    {
        Debug.Assert(v.Size == 1);
        Debug.Assert(v.Connections.Count == 1);
        Instantiate(Floor, new Vector3(TileSize * v.BasePosition.x, 0, TileSize * v.BasePosition.y), Quaternion.identity);
    }
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
