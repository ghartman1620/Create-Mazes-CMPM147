using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRoom : MissionTerminal {
    public GameObject Floor;
    public GameObject WinObject;
    public int TileSize;

    public override void Build(Vertex v)
    {
        Instantiate(Floor, new Vector3(v.BasePosition.x * TileSize, 0, v.BasePosition.y * TileSize), Quaternion.identity);
        Instantiate(WinObject, new Vector3(v.BasePosition.x * TileSize, 0, v.BasePosition.y * TileSize), Quaternion.identity);

    }
    // Use this for initialization
    void Start () {
        //Debug.Log("exit start");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
