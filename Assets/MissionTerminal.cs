using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


// An abstract base class for classes that will
// be things built in a MissionGraph.
// Current examples include space, key, door, entrance, and exit.
public abstract class MissionTerminal: MonoBehaviour
{
    // Builds this MissionTerminal in the space with
    // baseX, baseZ as the minimum point and baseX + size, baseZ + size as the maximum point.
    // It will be passed the tags created by the missiongraph for the terminal 
    // this is constructing - so it might include things like which key corresponds
    // to this door.
    public abstract void Build(float size, float baseX, float baseZ, HashSet<string> tags);
}
