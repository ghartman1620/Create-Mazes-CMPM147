using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRoom : MissionTerminal {
    public OpenableDoor Door;
    public GameObject Floor;
    public int TileSize;
    public override void Build(Vertex v)
    {
        Instantiate(Floor, new Vector3(TileSize*v.BasePosition.x, 0, TileSize*v.BasePosition.y), Quaternion.identity);
        foreach(Connection c in v.Connections)
        {
            switch (c.Dir)
            {
                case Direction.Up:
                    Instantiate(Door, 
                        new Vector3(TileSize * v.BasePosition.x, 0, TileSize * v.BasePosition.y), 
                        Quaternion.identity);

                    break;
                case Direction.Down:
                    Instantiate(Door,
                        new Vector3(TileSize * v.BasePosition.x, 0, TileSize * v.BasePosition.y - TileSize),
                        Quaternion.identity);
                    break;
                case Direction.Left:
                    Instantiate(Door,
                        new Vector3(TileSize * v.BasePosition.x - TileSize, 0, TileSize * v.BasePosition.y),
                        Quaternion.AngleAxis(90, new Vector3(0, 1, 0)));
                    break;
                case Direction.Right:
                    Instantiate(Door,
                        new Vector3(TileSize * v.BasePosition.x, 0, TileSize * v.BasePosition.y),
                        Quaternion.AngleAxis(90, new Vector3(0, 1, 0)));
                    break;
            }
        }
    }
    // Use this for initialization
    void Start () {
        //Debug.Log("door start");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
