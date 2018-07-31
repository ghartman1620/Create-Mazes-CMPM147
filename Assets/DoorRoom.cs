using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRoom : MissionTerminal {
    public KeyLock Door;
    public GameObject Floor;
    public int TileSize;
    public override void Build(Vertex v)
    {
        Instantiate(Floor, new Vector3(TileSize*v.BasePosition.x, 0, TileSize*v.BasePosition.y), Quaternion.identity);
        GameObject g = null;
        foreach(Connection c in v.Connections)
        {
            switch (c.Dir)
            {
                case Direction.Up:
                    g = Instantiate(Door, 
                        new Vector3(TileSize * v.BasePosition.x, 0, TileSize * v.BasePosition.y), 
                        Quaternion.identity).gameObject;

                    break;
                case Direction.Down:
                    g = Instantiate(Door,
                        new Vector3(TileSize * v.BasePosition.x, 0, TileSize * v.BasePosition.y - TileSize),
                        Quaternion.identity).gameObject;
                    break;
                case Direction.Left:
                    g = Instantiate(Door,
                        new Vector3(TileSize * v.BasePosition.x - TileSize, 0, TileSize * v.BasePosition.y),
                        Quaternion.AngleAxis(90, new Vector3(0, 1, 0))).gameObject;
                    break;
                case Direction.Right:
                    g = Instantiate(Door,
                        new Vector3(TileSize * v.BasePosition.x, 0, TileSize * v.BasePosition.y),
                        Quaternion.AngleAxis(90, new Vector3(0, 1, 0))).gameObject;
                    break;
            }
            foreach (Renderer rend in g.gameObject.GetComponentsInChildren<Renderer>())
            {
                rend.material.color = v.Color;
            }
            g.GetComponent<KeyLock>().LockId = v.LockId;
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
