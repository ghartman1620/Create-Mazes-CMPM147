using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntranceRoom : MissionTerminal {
    public override void Build(float size, float baseX, float baseZ, HashSet<string> tags)
    {
        Debug.Log("Entrance build");
    }
    // Use this for initialization
    void Start () {
        Debug.Log("entrance start");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
