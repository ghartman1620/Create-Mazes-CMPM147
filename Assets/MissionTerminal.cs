using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


// An abstract base class for classes that will
// be things built in a MissionGraph.
// Current examples include space, key, door, entrance, and exit.
public abstract class MissionTerminal : MonoBehaviour
{
    // Builds this MissionTerminal in the space with
    // baseX, baseZ as the minimum point and baseX + size, baseZ + size as the maximum point.
    // It will be passed the tags created by the missiongraph for the terminal 
    // this is constructing - so it might include things like which key corresponds
    // to this door.
    public abstract void Build(Vertex v);
        ////Debug.Log("MissionTerminal build on vertex " + v.Name);
        ////Debug.Log("Base position - x: " + v.BasePosition.x + ", y: " + v.BasePosition.y);
        ////Debug.Log("Size - " + v.Size);
        ////Debug.Log("Tags - ");
        //foreach(String s in v.Tags)
        //{
        //    //Debug.Log(s);
        //}
        ////Debug.Log("end tags");
        ////Debug.Log("connections");
        //foreach (Connection c in v.Connections)
        //{
        //    //Debug.Log(c.Dir);
        //    //Debug.Log(" Position - x: " + c.Point.x + ", y: " + c.Point.y);
        //}
        ////Debug.Log("end connections");
}
