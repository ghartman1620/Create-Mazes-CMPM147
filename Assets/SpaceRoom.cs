using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceRoom : MissionTerminal {
    public override void Build(float size, float baseX, float baseZ, HashSet<string> tags)
    {
        Debug.Log("space build");
    }
    // Use this for initialization
    void Start () {
        Debug.Log("space start");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
