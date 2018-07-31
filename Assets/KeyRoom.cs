using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyRoom : MissionTerminal {
    public GameObject Floor;
    public GameObject Key;
    public int TileSize;


    public override void Build(Vertex v)
    {
        Instantiate(Floor, new Vector3(TileSize*v.BasePosition.x, 0, TileSize*v.BasePosition.y), Quaternion.identity);
        Instantiate(Key, new Vector3(TileSize * v.BasePosition.x+TileSize/4, 1, TileSize * v.BasePosition.y), Quaternion.identity);
        Instantiate(Key, new Vector3(TileSize * v.BasePosition.x-TileSize/4, 1, TileSize * v.BasePosition.y), Quaternion.identity);

    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called o nce per frame
	void Update () {
		
	}
}
