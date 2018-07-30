using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionSpace : MonoBehaviour {
    public int SpaceBudget;
    public int ObstacleBudget;
    public MissionTerminal Door;
    public MissionTerminal Exit;
    public MissionTerminal Entrance;
    public MissionTerminal Space;
    public MissionTerminal Key;

	// Use this for initialization
	void Start () {
        MissionGraph g = new MissionGraph(ObstacleBudget, SpaceBudget);
        g.CompleteProduction();
        Debug.Log(g.TerminalCount);
        foreach(Vertex v in g)
        {
            MissionTerminal t = Instantiate(terminalType(v.Name), new Vector3(0, 0, 0), Quaternion.identity);
            t.Build(0, 0, 0, v.Tags);
        }
        // So, for now, due to lack of time, I'm going to make some assumptions
        // specific to this grammar to create the space. This makes it easier to write
        // this bit of code but makes it harder in the future to change the missiongraph grammar
        // But I can remove this stuff if I chose to change the grammar in the future
        // and make more scalable code here.
        // So this is fine for now.

        // Here, in assertion form, are the assumptions I make about a graph
        // produced by the current grammar
        foreach(Vertex v in g)
        {
            // Spaces can only have four vertices conneted to them
            if (v.Name.Equals("space"))
            {
                Debug.Assert(v.BackAdj.Count + v.ForwardAdj.Count <= 4);
            }

            // Nothing leads into an entrance, and an entrance leads into one vertex
            else if(v.Name.Equals("entrance"))
            {
                Debug.Assert(v.ForwardAdj.Count == 1 && v.BackAdj.Count == 0);
            }
            // Only one vertex leads into an exit, and an exit leads into nothing
            else if (v.Name.Equals("exit"))
            {
                Debug.Assert(v.ForwardAdj.Count == 0 && v.BackAdj.Count == 1);
            }
            // Only one vertex leads into a key room and a key room leads nowhere
            else if(v.Name.Equals("key"))
            {
                Debug.Assert(v.ForwardAdj.Count == 0 && v.BackAdj.Count == 1);
            }
            // Exactly one vertex enters a door and exactly one vertex leads out of a door.
            else if (v.Name.Equals("door"))
            {
                Debug.Assert(v.ForwardAdj.Count == 1 && v.BackAdj.Count == 1);
            }
        }


	}
	private MissionTerminal terminalType(string s)
    {
        switch (s)
        {
            case "door":
                return Door;
            case "exit":
                return Exit;
            case "space":
                return Space;
            case "key":
                return Key;
            case "entrance":
                return Entrance;
            default:
                Debug.Log("invalid terminal type " + s);
                Debug.Assert(false);
                return null;
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
