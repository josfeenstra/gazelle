using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace Gazelle
{
    struct CurveFragment
    {
        public int face, edgeFrom, edgeTo;
        public double paramFrom, paramTo;
        public Curve fragment;

        public CurveFragment(int face, int edgeFrom, int edgeTo, double paramFrom, double paramTo, Curve fragment)
        {
            this.face = face;
            this.edgeFrom = edgeFrom;
            this.edgeTo = edgeTo;
            this.paramFrom = paramFrom;
            this.paramTo = paramTo;
            this.fragment = fragment ?? throw new ArgumentNullException(nameof(fragment));
        }

        public override string ToString()
        {
            return $"Fragment | face {face} | edgeFrom {edgeFrom} | edgeTo {edgeTo} | " +
                   $"paramFrom {paramFrom} | paramTo {paramTo}.";
        }
    }

    // used for better traversal through breploops
    class FaceLoopCollection
    {
        List<int> sequence; // the entire sequence

        Dictionary<int, int> nextLine;
        HashSet<int> visited;
        Stack<int> unvisited;

        public FaceLoopCollection(BrepFace face)
        {
            nextLine = new Dictionary<int, int>();
            visited = new HashSet<int>();
            unvisited = new Stack<int>();
            sequence = new List<int>();

            foreach (var loop in face.Loops)
            {
                for(int i = 0; i < loop.Trims.Count; i++)
                {
                    int trimID = loop.Trims[i].TrimIndex;
                    int trimIDNext = loop.Trims[(i + 1) % loop.Trims.Count].TrimIndex;
                    sequence.Add(trimID);
                    nextLine.Add(trimID, trimIDNext);
                    unvisited.Push(trimID);
                }
            }
        }

        // keep extracting loops untill exhausted. 
        public List<int[]> CreateNewLoops()
        {
            var loops = new List<int[]>();
            var loop = new List<int>();
            var loopStart = int.MinValue;

            while (unvisited.Count > 0)
            {
                // we use this construction to make sure all original lines will eventually be visited
                // and turned into loops
                var line = unvisited.Pop();
                if (visited.Contains(line)) // never visit a line twice 
                    continue;
                visited.Add(line);
                // Debug.Log($"visiting {line}");

                // actually add something to the loop
                loop.Add(line);

                if (!nextLine.TryGetValue(line, out int next))
                    throw new Exception($"{line} is not present in next dictionary!");

                if (loopStart == int.MinValue)
                {
                    // first line added to loop
                    loopStart = line;
                }
                else if (next == loopStart)
                {
                    // end of the sequence, start a new loop
                    // Debug.Log("ca ching");
                    loops.Add(loop.ToArray());
                    loop = new List<int>();
                    loopStart = int.MinValue;
                }

                unvisited.Push(next);
            }

            // the last created loop should always be empty...
            if (loop.Count != 0)
                Debug.Log("WARNING: leftover trims during the loop procedure!!");
            return loops;
        }

        private bool AppearsFirst(int firstItem, int secondItem)
        {
            return sequence.IndexOf(firstItem) < sequence.IndexOf(secondItem);
        }

        // highjack part of the loop, and build a 'bridge' from one part to another
        // build two-way bridges to ensure we can create enough loops
        public void AddTwoWayBridge(int bridgeHeen, int bridgeTerug, int a, int b, int c, int d)
        {
            // for a / b. from = whomever is first
            bool aFirst = AppearsFirst(a, b);
            bool cFirst = AppearsFirst(c, d);

            int HeenFrom  = aFirst ? a : b;
            int HeenTo    = cFirst ? d : c;
            int TerugFrom = cFirst ? c : d;
            int TerugTo   = aFirst ? b : a;

            // towards
            nextLine[HeenFrom] = bridgeHeen;          
            nextLine.Add(bridgeHeen, HeenTo);
            unvisited.Push(bridgeHeen);

            // back
            nextLine[TerugFrom] = bridgeTerug;
            nextLine.Add(bridgeTerug, TerugTo);
            unvisited.Push(bridgeTerug);
        }

        public void PrintDictionary()
        {
            var str = "NextLine : ";
            foreach (var key in nextLine.Keys)
            {
                str += $"{key}: {nextLine[key]} | ";
            }
            Debug.Log(str);
        }
    }

    // these are the parts automaticly added after a trim action like this
    struct SplitResult
    {
        int originalEdgeIndex;
        int faceA, faceB;
        int trimA1, trimA2, trimB1, trimB2;

        public SplitResult(int originalEdgeIndex, int faceA, int faceB, int trimA1, int trimA2, int trimB1, int trimB2)
        {
            this.originalEdgeIndex = originalEdgeIndex;
            this.faceA = faceA;
            this.faceB = faceB;
            this.trimA1 = trimA1;
            this.trimA2 = trimA2;
            this.trimB1 = trimB1;
            this.trimB2 = trimB2;
        }

        public bool GetTrims(int face, out int trim1, out int trim2)
        {     
            if (face == faceA)
            {
                trim1 = trimA1;
                trim2 = trimA2;
                return true;
            }
            else if (face == faceB)
            {
                trim1 = trimB1;
                trim2 = trimB2;
                return true;
            }
            else
            {
                trim1 = -1;
                trim2 = -1;
                return false;
            }
        }

        public override string ToString()
        {
            return $"SplitResult : {originalEdgeIndex} TO {trimA1} {trimA2} {trimB1} {trimB2}";
        }
    }

}
