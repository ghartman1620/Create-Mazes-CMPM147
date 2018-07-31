using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MissionSpace : MonoBehaviour {
    public int SpaceBudget;
    public int ObstacleBudget;
    public int TileSize;
    public MissionTerminal Door;
    public MissionTerminal Exit;
    public MissionTerminal Entrance;
    public MissionTerminal Space;
    public MissionTerminal Key;
    public Transform player;

    private MissionGraph g;
	// Use this for initialization
	void Start () {
        g = new MissionGraph(ObstacleBudget, SpaceBudget);
        g.CompleteProduction();
        
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

        // Pick a vertex. Give it spot (0, 0). If it's a space, give it some size.

        // ITerate over the ForwardAdj of the currently selected vertex. For each,
        // Add a connection to this vertex, and add the corresponding connection to 
        // each of those vertices.
        // Then go through each and pick them and find a spot for each based on its connections.

        IEnumerator<Vertex> enumer = g.GetEnumerator();
        enumer.MoveNext();
        Vertex curr = enumer.Current;
        curr.BasePosition = new Vector2(0, 0);
        curr.Visited = true;
        Queue<Vertex> rooms = new Queue<Vertex>();
        rooms.Enqueue(curr);
        while (rooms.Count != 0)
        {

            //Oh my. It's the night before the assignment is due. This is some 
            // "night before assignment is due" code.
            // Modified BFS to give each vertex physical connections to 
            // nearby vertices and space.
            curr = rooms.Dequeue();
            if (curr.Name == "space")
            {
                Debug.Assert(curr.Size == SpaceBudget);
            }

            // This is literally some of the worst code. A not modular, untested, 100 line monstrosity of a loop.
            // yeesh. cmon gabe.
            //Debug.Log("Visiting vertex with name " + curr.Name);

            bool upUsed = false;
            bool downUsed = false;
            bool rightUsed = false;
            bool leftUsed = false;
            foreach(Connection c in curr.Connections)
            {
                if(c.Dir == Direction.Up)
                {
                    upUsed = true;
                }
                if(c.Dir == Direction.Left)
                {
                    leftUsed = true;
                }
                if(c.Dir == Direction.Right)
                {
                    rightUsed = true;
                }
                if(c.Dir == Direction.Down)
                {
                    downUsed = true;
                }
            }
            HashSet<Vertex> allVertices = new HashSet<Vertex>();
            foreach (Vertex v in curr.ForwardAdj)
            {
                allVertices.Add(v);

            }
            foreach (Vertex v in curr.BackAdj)
            {
                allVertices.Add(v);

            }

            foreach (Vertex v in allVertices)
            {
               
                //Only visit if we've not visited it before.
                if(!v.Visited)
                {
                    //Debug.Log("Examining connection to " + v.Name);
                    v.Visited = true;
                    // Aaaaaaaaaaaaaaaah
                    bool directionValid = false;
                    Direction d = Direction.Up; //void uninitialized value
                    while (!directionValid)
                    {
                        d = randomDirection();
                        if (d == Direction.Up && !upUsed)
                        {
                            upUsed = true;
                            directionValid = true;
                        }
                        else if (d == Direction.Down && !downUsed)
                        {
                            downUsed = true;
                            directionValid = true;
                        }
                        else if (d == Direction.Left && !leftUsed)
                        {
                            leftUsed = true;
                            directionValid = true;
                        }
                        else if(d == Direction.Right && !rightUsed)
                        {
                            rightUsed = true;
                            directionValid = true;
                        }
                    }
                    //If you're reading this, Jo, I worked really hard on this and 
                    // I'm just as disappointed as you are that this code I'm writing here 
                    // is lame and unscalable.
                    int intersectX = 0;
                    int intersectY = 0;
                    int connectedTileX = 0;
                    int connectedTileY = 0;

                    switch (d)
                    {
                        case Direction.Up:
                            intersectX = (int)curr.BasePosition.x + (int)Mathf.Floor(Random.Range(0, curr.Size));
                            intersectY = (int)curr.BasePosition.y + curr.Size-1;
                            connectedTileX = intersectX;
                            connectedTileY = intersectY + 1;
                            break;
                        case Direction.Down:
                            intersectX = (int)curr.BasePosition.x + (int)Mathf.Floor(Random.Range(0, curr.Size));
                            intersectY = (int)curr.BasePosition.y;
                            connectedTileX = intersectX;
                            connectedTileY = intersectY - 1;
                            break;
                        case Direction.Left:
                            intersectX = (int)curr.BasePosition.x;
                            intersectY = (int)curr.BasePosition.y + (int)Mathf.Floor(Random.Range(0, curr.Size));
                            connectedTileX = intersectX - 1;
                            connectedTileY = intersectY;
                            break;
                        case Direction.Right:
                            intersectX = (int)curr.BasePosition.x + curr.Size-1;
                            intersectY = (int)curr.BasePosition.y + (int)Mathf.Floor(Random.Range(0, curr.Size));
                            connectedTileX = intersectX + 1;
                            connectedTileY = intersectY;
                            break;
                    }
                    //Debug.Log("adding connection between " + curr.Name + " and " + v.Name);
                    //Debug.Log("in direction from source " + d);
                    curr.Connections.Add(new Connection(d, new Vector2(intersectX, intersectY)));
                    v.Connections.Add(new Connection(oppositeDirection(d), new Vector2(connectedTileX, connectedTileY)));
                    if (v.Size == 1)
                    {
                        v.BasePosition = new Vector2(connectedTileX, connectedTileY);
                    }
                    else
                    {
                        switch (oppositeDirection(d))
                        {
                            // random x position with part at intersect point, y position down from intersect by size
                            case Direction.Up:
                                v.BasePosition = new Vector2(connectedTileX - v.Size + 1 + Mathf.Floor(Random.Range(0, v.Size)),
                                                        connectedTileY - v.Size + 1);
                                break;
                            //random x position with part at the intersect point, y  position at intersect
                            case Direction.Down:
                                v.BasePosition = new Vector2(connectedTileX - v.Size + 1 + Mathf.Floor(Random.Range(0, v.Size)),
                                                        connectedTileY);
                                break;
                            //random y position with part at intersect point, x position at 
                            case Direction.Left:
                                v.BasePosition = new Vector2(connectedTileX,
                                                            connectedTileY - v.Size + 1 + Mathf.Floor(Random.Range(0, v.Size)));
                                break;
                            case Direction.Right:
                                v.BasePosition = new Vector2(connectedTileX - v.Size + 1,
                                                            connectedTileY - v.Size + 1 + Mathf.Floor(Random.Range(0, v.Size)));
                                break;
                        }
                    }
                    rooms.Enqueue(v);
                }
            }
        }
        foreach(Vertex v in g)
        {
            MissionTerminal t = Instantiate(terminalType(v.Name), new Vector3(0, 0, 0), Quaternion.identity);
            t.Build(v);
        }
        ResetPlayerPosition();
        
    }
    private Direction randomDirection()
    {
        int rand = (int)Mathf.Floor(Random.Range(0, 4));
        switch (rand)
        {
            case 0:
                return Direction.Up;
            case 1:
                return Direction.Down;
            case 2:
                return Direction.Left;
            case 3:
                return Direction.Right;
            default:
                throw new System.Exception("this can not happen");
        }
    }
    private Direction oppositeDirection(Direction d)
    {
        switch (d)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Left:
                return Direction.Right;
            case Direction.Down:
                return Direction.Up;
            default: //right
                return Direction.Left;
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
                //Debug.Log("invalid terminal type " + s);
                Debug.Assert(false);
                return null;
        }
    }

	// Update is called once per frame
	void Update () {
        if (player.position.y < -5)
        {
            ResetPlayerPosition();
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
	}
    void ResetPlayerPosition()
    {
        foreach (Vertex v in g)
        {
            
            if (v.Name == "entrance")
            {
                player.position = new Vector3(v.BasePosition.x * TileSize, 0, v.BasePosition.y * TileSize);
            }
        }
    }
}
