using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

// POD class that returns the results of path generation.
class GeneratedPath
{
    public List<Vector2> Spaces;
    public List<Vector2> Ramps;
    public GeneratedPath(List<Vector2> spaces, List<Vector2> ramps)
    {
        this.Spaces = spaces;
        this.Ramps = ramps;
    }
}

class UnsatisfiableException: System.Exception
{
    public UnsatisfiableException(): base("UNSATISFIABLE"){}
}

class PathGeneration
{
    public static GeneratedPath RunPathGeneration(List<Vector2> tiles, List<Vector2> startPoints, int modelCount, int tileSize, int max, int numRamps)
    {
        string exec = "../clingo-5.3.0-win64/clingo.exe";
        string[] inputs = { "Assets/path.lp", "Assets/ramps.lp" };
        string defaultArgs = modelCount + " --project --seed=" + Random.Range(0, int.MaxValue).ToString()
                + " -c n=" + tileSize + " -c max=" + max + " -c ramps=" + numRamps;
        // Input
        StringBuilder clingoArguments = new StringBuilder(defaultArgs);
        StringBuilder clingoFileInput = new StringBuilder();
        // not doing any check, but tiles ought to be ints describing coordinates
        // of adjacent tiles - for example (0,0),(0,1),(1,1)
        foreach (Vector2 tile in tiles)
        {
            clingoFileInput.Append("tile((" + (int)tile.x + "," + (int)tile.y + ")).\n");
        }
        // And endPoints should be in the grid denoted by the tiles and the size n of each tile.
        // So on the above grid, two valid endpoints might be (0, 2) and (5, 5).
        foreach (Vector2 point in startPoints)
        {
            clingoFileInput.Append("startpoint((" + (int)point.x + "," + (int)point.y + ")).\n");
        }
        const string FileName = "clingoinputtmp.lp";
        File.WriteAllText(FileName, clingoFileInput.ToString());

        foreach (string s in inputs)
        {
            clingoArguments.Append(" " + s);
        }
        clingoArguments.Append(" " + FileName);

        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.FileName = exec;
        p.StartInfo.Arguments = clingoArguments.ToString();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.Start();

        p.WaitForExit();

        if (!p.StandardError.EndOfStream)
        {
            StringBuilder s = new StringBuilder();
            while (!p.StandardError.EndOfStream)
            {
                s.Append(p.StandardError.ReadLine());
            }
            File.WriteAllText("clingoerror", s.ToString());
            
            throw new System.Exception("error from clingo:\n" + s + "\nclingo's input:\n" + clingoFileInput.ToString() + "\nclingo arguments:\n" + clingoArguments);
        }
        try
        {
            return ParseClingoOutput(p.StandardOutput);
        }
        catch(UnsatisfiableException)
        {
            throw new System.Exception("UNSATISFIABLE\nArguments given:\n" + clingoArguments + "\nInput given:\n" + clingoFileInput.ToString());
        }
        
    }
    private static GeneratedPath ParseClingoOutput(StreamReader output)
    {
        List<Vector2> outputPoints = new List<Vector2>();
        List<string> spaceStrings = new List<string>();
        List<Vector2> rampPoints = new List<Vector2>();
        string space = "space";
        string unsat = "UNSATISFIABLE";
        while (!output.EndOfStream)
        {
            string s = output.ReadLine();
            if (s.Length >= space.Length && s.Substring(0, space.Length).Equals(space))
            {
                spaceStrings.Add(s);
            }

            else if (s.Length >= unsat.Length && s.Substring(0, unsat.Length).Equals(unsat))
            {
                throw new UnsatisfiableException();
            }
        }
        // select one fo the random models for this tile.
        string pointString = spaceStrings[(int)Random.Range(0, spaceStrings.Count)];
        Regex spaceRegex = new Regex(@"space\(\([0-9]+,[0-9]+\)\)");
        Regex rampRegex = new Regex(@"ramp\(\([0-9]+,[0-9]+\),\([0-9]+,[0-9]+\),\([0-9]+,[0-9]+\)\)");
        Regex pairRegex = new Regex(@"\([0-9]+,[0-9]+\)");

        
        foreach (Match m in spaceRegex.Matches(pointString))
        {
            outputPoints.Add(parsePoint(m.Value));   
        }
        Debug.Log("parsing ramp matches");
        Debug.Log(pointString);
        foreach (Match m in rampRegex.Matches(pointString))
        {
            Debug.Log(m.Value);
            MatchCollection pairMatches = pairRegex.Matches(m.Value);
            Debug.Assert(pairMatches.Count == 3);
            foreach(Match pair in pairMatches)
            {
                rampPoints.Add(parsePoint(pair.Value));
            }
        }
        return new GeneratedPath(outputPoints, rampPoints);
    }
    //parses a string in the form (x, y) where x and y are some parsable ints and returns a vector2 with them in it.
    private static Vector2 parsePoint(string point)
    {
        Regex numRegex = new Regex(@"[0-9]+");
        MatchCollection numMatches = numRegex.Matches(point);
        Debug.Assert(numMatches.Count == 2);
        IEnumerator enumer = numMatches.GetEnumerator();
        enumer.MoveNext();
        int x = int.Parse(((Match)enumer.Current).Value);
        enumer.MoveNext();
        int y = int.Parse(((Match)enumer.Current).Value);
        return new Vector2(x, y);
    }
    private static StringBuilder ReadFiles(string[] inputs)
    {
        StringBuilder contents = new StringBuilder();
        foreach (string input in inputs)
        {
            contents.Append(File.ReadAllText(input));
        }
        return contents;
    }

}

