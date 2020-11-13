using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace Gazelle
{
    // a package of data to save how and where parts of a curve need to be inserted
    class CurveFragment
    {
        public Curve fragment;
        public int face; 
        
        public int vertexFrom, vertexTo;
        public int a, b, c, d;

        public CurveFragment(Curve fragment, int face, int vertexFrom, int vertexTo, int a, int b, int c, int d)
        {
            this.fragment = fragment ?? throw new ArgumentNullException(nameof(fragment));
            this.face = face;

            this.vertexFrom = vertexFrom; // vertex in between a and b
            this.vertexTo = vertexTo; // vertex in between c and d

            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public override string ToString()
        {
            return $"Fragment | face {face} | vertexFrom {vertexFrom} | vertexTo {vertexTo} | a : {a} | b : {b} | c: {c} | d: {d}";
        }
    }

    class FaceLoopCollection
    {
        List<int> all;
        Stack<int> trims;
        Dictionary<int, int> nextTrim; // pointer to the next point in the loop

        public FaceLoopCollection(BrepFace face)
        {
            all = new List<int>();
            trims = new Stack<int>();
            nextTrim = new Dictionary<int, int>();
                    
            foreach (var loop in face.Loops)
            {
                int n = loop.Trims.Count;
                for (int i = 0; i < n; i++)
                {
                    // store some of the trim data, so we can 'highjack' these loops later
                    var t  = loop.Trims[i];
                    var tn = loop.Trims[(i + 1) % n];

                    int ti = t.TrimIndex;
                    int tin = tn.TrimIndex;
                    int vi = t.Edge.StartVertex.VertexIndex;
                    int nvi = t.Edge.EndVertex.VertexIndex;

                    all.Add(ti);
                    trims.Push(ti);
                    nextTrim.Add(ti, tin);
                }
            }
        }
       
        public List<int[]> CreateNewLoops()
        {
            var loops = new List<int[]>();
            var loop = new List<int>();
            var loopStart = int.MinValue;

            // keep extracting loops until exhausted. 
            var visited = new HashSet<int>();
            while (trims.Count > 0)
            {
                // we use this construction to make sure all original lines will eventually be visited
                // and turned into loops
                var ti = trims.Pop();
                if (visited.Contains(ti)) // never visit a line twice 
                    continue;
                visited.Add(ti);

                // debug
                // Debug.Log($"visiting {line}");

                if (!nextTrim.TryGetValue(ti, out int nti))
                    throw new Exception($"{ti} is not present in next dictionary!");


                // we fill the loop with trim indices
                loop.Add(ti);

                if (loopStart == int.MinValue)
                {
                    // first line added to loop
                    loopStart = ti;
                }
                else if (nti == loopStart)
                {
                    // end of the sequence, start a new loop
                    // Debug.Log("ca ching");
                    loops.Add(loop.ToArray());
                    loop = new List<int>();
                    loopStart = int.MinValue;
                }

                trims.Push(nti);
            }

            // the last created loop should always be empty...
            if (loop.Count != 0)
                Debug.Log("WARNING: leftover trims during the loop procedure!!");
            return loops;
        }

        // highjack part of the loop, and build a 'bridge' from one part to another
        // build two-way bridges to ensure we can create enough loops
        public void AddTwoWayBridge(int trimForward, int trimBackward, int a, int b, int c, int d)
        {
            // forwards
            nextTrim[a] = trimForward;  
            trims.Push(trimForward);
            nextTrim.Add(trimForward, d);

            // backwards
            nextTrim[c] = trimBackward;
            trims.Push(trimBackward);
            nextTrim.Add(trimBackward, b);    
        }

        private bool AppearsFirst(int firstItem, int secondItem)
        {
            return all.IndexOf(firstItem) < all.IndexOf(secondItem);
        }

        public void PrintDictionary()
        {
            var str = "LoopCollection : ";
            foreach (var key in nextTrim.Keys)
            {
                str += $"{key}: {nextTrim[key]} | ";
            }
            Debug.Log(str);
        }
    }
}
