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
    public int AttemptsBeforeTimeout;

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
        // I actually don't mind this that much even though it looks janky as fuck.
        // One of the ways we discussed to deal with bad results of generation is 
        // constraint checking - generate some stuff and make sure it follows the rules.
        // While this constraint checking could be prevented with a more sophisticated
        // space assigning algorithm, I'm worried such an algorithm might become slower
        // than using an unsophisticated algorithm willing to say "oh no, I've made a horrible mistake,
        // let's try again." 

        // If I wanted to go down that route because maybe at some point in the future when I'm trying to generate very large
        // levels because I have more kinds of things to fill the space with, the first place I'd try is to 
        // teach the algorithm to select directions in a better way than random selection. That's ultimately what causes
        // the problems, and doing something like, for example, trying to find the deepest branch of a tree and allocate it
        // a direction to grow in, might solve many issues here. 


        // But for now this works okay.

        // It seems like the reasonable maximum number of obstacles for the current (as of 8/8) grammar is 13.

        // But nobody wants a maze that big anyway I think the realistic maximum is like 6.
        int attempts = 1;
        while(attempts < AttemptsBeforeTimeout)
        {
            try
            {
                AssignVerticesSpace();
                break;
            }
            catch(System.Exception)
            {
                
                attempts++;
                int seed = Random.Range(int.MinValue, int.MaxValue);
                Random.InitState(seed);
            }
        }
        if(attempts == AttemptsBeforeTimeout)
        {
            throw new System.Exception("Error! World not successfully generated after " + attempts + " attempts, exiting.");
        }
        //Debug.Log("Success after " + attempts + " attempts to generate");
        foreach (Vertex v in g)
        {
            MissionTerminal t = Instantiate(terminalType(v.Name), new Vector3(0, 0, 0), Quaternion.identity);

            t.Build(v);
        }
        ResetPlayerPosition();
        
    }
    public void AssignVerticesSpace()
    {
        // Pick a vertex. Give it spot (0, 0). If it's a space, give it some size.

        // ITerate over the ForwardAdj of the currently selected vertex. For each,
        // Add a connection to this vertex, and add the corresponding connection to 
        // each of those vertices.
        // Then go through each and pick them and find a spot for each based on its connections.
        foreach (Vertex v in g)
        {
            v.BasePosition = new Vector2(0, 0);
            v.Connections = new HashSet<Connection>();
            v.Coordinate = new Vector2(0, 0);
            v.Visited = false;
        }

        IEnumerator<Vertex> enumer = g.GetEnumerator();
        enumer.MoveNext();
        Vertex curr = enumer.Current;
        curr.BasePosition = new Vector2(0, 0);
        curr.Visited = true;
        curr.Coordinate = new Vector2(0, 0);
        Queue<Vertex> rooms = new Queue<Vertex>();
        rooms.Enqueue(curr);
        HashSet<Vector2> usedCoords = new HashSet<Vector2>();
        usedCoords.Add(curr.Coordinate);
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
            ////Debug.Log("Visiting vertex with name " + curr.Name);

            bool upUsed = false;
            bool downUsed = false;
            bool rightUsed = false;
            bool leftUsed = false;
            foreach (Connection c in curr.Connections)
            {
                if (c.Dir == Direction.Up)
                {
                    upUsed = true;
                }
                if (c.Dir == Direction.Left)
                {
                    leftUsed = true;
                }
                if (c.Dir == Direction.Right)
                {
                    rightUsed = true;
                }
                if (c.Dir == Direction.Down)
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
                if (!v.Visited)
                {
                    v.Visited = true;
                    // Aaaaaaaaaaaaaaaah
                    bool directionValid = false;
                    Direction d = randomDirection();

                    int loopCount = 0;
                    while (!directionValid)
                    {
                        if (++loopCount == 5)
                        {
                            throw new System.Exception("An unarrangable arrangement of tiles was made. You should write some backtracking code to deal with this.");
                        }
                        directionValid = true;
                        // check coordinate taken
                        directionValid = (d == Direction.Up && !usedCoords.Contains(new Vector2(curr.Coordinate.x, curr.Coordinate.y + 1))) ||
                            (d == Direction.Down && !usedCoords.Contains(new Vector2(curr.Coordinate.x, curr.Coordinate.y - 1))) ||
                            (d == Direction.Left && !usedCoords.Contains(new Vector2(curr.Coordinate.x - 1, curr.Coordinate.y))) ||
                            (d == Direction.Right && !usedCoords.Contains(new Vector2(curr.Coordinate.x + 1, curr.Coordinate.y)));
                        // check used side already
                        if (directionValid)
                        {
                            directionValid = (d == Direction.Up && !upUsed) ||
                                (d == Direction.Down && !downUsed) ||
                                (d == Direction.Left && !leftUsed) ||
                                (d == Direction.Right && !rightUsed);
                        }
                        if (!directionValid)
                        {
                            d = nextDirection(d);
                        }

                    }

                    int intersectX = 0;
                    int intersectY = 0;
                    int connectedTileX = 0;
                    int connectedTileY = 0;

                    switch (d)
                    {
                        case Direction.Up:
                            intersectX = (int)curr.BasePosition.x + (int)Mathf.Floor(Random.Range(0, curr.Size));
                            intersectY = (int)curr.BasePosition.y + curr.Size - 1;
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
                            intersectX = (int)curr.BasePosition.x + curr.Size - 1;
                            intersectY = (int)curr.BasePosition.y + (int)Mathf.Floor(Random.Range(0, curr.Size));
                            connectedTileX = intersectX + 1;
                            connectedTileY = intersectY;
                            break;
                    }
                    ////Debug.Log("adding connection between " + curr.Name + " and " + v.Name);
                    ////Debug.Log("in direction from source " + d);
                    curr.Connections.Add(new Connection(d, new Vector2(intersectX, intersectY)));
                    v.Connections.Add(new Connection(oppositeDirection(d), new Vector2(connectedTileX, connectedTileY)));
                    switch (d)
                    {
                        case Direction.Up:
                            v.Coordinate = new Vector2(curr.Coordinate.x, curr.Coordinate.y + 1);
                            break;
                        case Direction.Down:
                            v.Coordinate = new Vector2(curr.Coordinate.x, curr.Coordinate.y - 1);
                            break;
                        case Direction.Right:
                            v.Coordinate = new Vector2(curr.Coordinate.x + 1, curr.Coordinate.y);
                            break;
                        case Direction.Left:
                            v.Coordinate = new Vector2(curr.Coordinate.x - 1, curr.Coordinate.y);
                            break;

                    }
                    usedCoords.Add(v.Coordinate);
                    if (v.Size == 1)
                    {

                        v.BasePosition = new Vector2(connectedTileX, connectedTileY);

                    }
                    else
                    {
                        switch (d)
                        {
                            // random x position with part at intersect point, y position down from intersect by size
                            case Direction.Up:
                                v.BasePosition = new Vector2(curr.BasePosition.x, curr.BasePosition.y + curr.Size);
                                break;
                            //random x position with part at the intersect point, y  position at intersect
                            case Direction.Down:
                                v.BasePosition = new Vector2(curr.BasePosition.x, curr.BasePosition.y - curr.Size);
                                break;
                            //random y position with part at intersect point, x position at 
                            case Direction.Left:
                                v.BasePosition = new Vector2(curr.BasePosition.x - curr.Size, curr.BasePosition.y);
                                break;
                            case Direction.Right:
                                v.BasePosition = new Vector2(curr.BasePosition.x + curr.Size, curr.BasePosition.y);
                                break;
                        }
                    }

                    rooms.Enqueue(v);
                }
            }
        }
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
    private Direction nextDirection(Direction d)
    {
        switch (d)
        {
            case Direction.Up:
                return Direction.Right;
            case Direction.Right:
                return Direction.Down;
            case Direction.Down:
                return Direction.Left;
            case Direction.Left:
                return Direction.Right;
            default:
                throw new System.Exception("no such direction for nextDirection: " + d);
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
                ////Debug.Log("invalid terminal type " + s);
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
