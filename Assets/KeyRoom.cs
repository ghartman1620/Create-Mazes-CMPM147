using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyRoom : MissionTerminal {
    public override void Build(float size, float baseX, float baseZ, HashSet<string> tags)
    {
        Debug.Log("key build");
    }

	// Use this for initialization
	void Start () {
        Debug.Log("key start");
    }
	
	// Update is called o nce per frame
	void Update () {
		
	}
}
