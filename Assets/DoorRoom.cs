using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRoom : MissionTerminal {
    public override void Build(float size, float baseX, float baseZ, HashSet<string> tags)
    {
        Debug.Log("door build");
    }
    // Use this for initialization
    void Start () {
        Debug.Log("door start");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
