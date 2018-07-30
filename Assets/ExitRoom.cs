using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRoom : MissionTerminal {
    public override void Build(float size, float baseX, float baseZ, HashSet<string> tags)
    {
        Debug.Log("exit build!");
    }
    // Use this for initialization
    void Start () {
        Debug.Log("exit start");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
