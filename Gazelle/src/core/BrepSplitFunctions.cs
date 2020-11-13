using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Rhino.Geometry.Intersect;

namespace Gazelle
{
    /// <summary>
    /// These are all functions for advanced brep splitting
    /// </summary>
    internal static class BrepSplitFunctions
    {
        // MAIN | 
        public static Brep SplitBrepWithCurves(this Brep brep, List<Curve> curves, 
            out List<int> createdFaces, out List<int> otherFaces)
        {
            Debug.Flush();

            brep = brep.DuplicateBrep();
            createdFaces = new List<int>();
            var allOldFaces = new List<int>();

            // MAIN
            foreach (var curve in curves)
            {
                brep = brep.SplitBrepWithCurve(curve, out var newFaces, out var oldFaces);
                createdFaces.AddRange(newFaces);
                allOldFaces.AddRange(oldFaces);
            }

            // remove the faces that recieved new loops
            var nOldFaces = allOldFaces.Count;
            brep = brep.RemoveFaces(allOldFaces);

            // change these indexes accordingly
            for (int i = 0; i < createdFaces.Count; i++)
            {
                createdFaces[i] -= nOldFaces;
            }

            // get all faces that are NOT newly created, but edited
            // TODO make this more efficient
            otherFaces = new List<int>();
            for (int i = 0; i < brep.Faces.Count; i++)
            {
                if (!createdFaces.Contains(i))
                    otherFaces.Add(i);
            }

            // finally, fix everyhing, and hope that this doenst change face indexing
            brep.Standardize();
            return brep;
        }
     

        // HELPER | TODO : FIX INTERNAL HOLES -> CAN BE DONE BY PROCESSING ALL FRAGMENTS PER FACE AT THE SAME TIME
        // Per Curve, 3 options: 
        // 
        // option 1:
        // if curve intersects one or more edges: split the curve in intelligent ways, and add it.
        // 
        // option 2: 
        // if curve has no intersections BUT 
        // its starting point touches a face:
        //    build inner face
        // 
        // option 3: 
        // if curve has no intersections AND 
        // its starting point is not touching any face, 
        //    do nothing
        private static Brep SplitBrepWithCurve(this Brep brep, Curve curve, 
            out List<int> createdFaces, out List<int> oldFaces)
        {
            createdFaces = new List<int>();
            oldFaces = new List<int>();
            var mappedFragments = new Dictionary<int, List<CurveFragment>>();

            var succes = TryGetFragments(brep, curve, ref mappedFragments);
            if (succes)
            {
                // debug
                Debug.Log("All Fragments");
                foreach (var face in mappedFragments.Keys)
                    foreach (var frag in mappedFragments[face])
                        Debug.Log("face: " + frag.ToString());

                // option 1
                foreach (var face in mappedFragments.Keys)
                {
                    brep = AddFragmentsToFace(brep, face, mappedFragments[face],
                        ref createdFaces, ref oldFaces);
                }
            }
            else if (TryMatchCurveToFace(brep, curve, out int match))
            {
                // option 2
                Debug.Log("match: " + match.ToString());
                int face = brep.BuildInnerFace(match, curve);
                createdFaces.Add(face);
            }
            else
            {
                // option 3
                // do nothing
            }

            return brep;
        }


        // HELPER | 
        public static bool TryGetFragments(Brep brep, Curve curve, ref Dictionary<int, List<CurveFragment>> mappedFragments)
        {  
            var xs = new List<IntersectionEvent>(); // all intersectionEvents 
            var trimData = new SortedList<double, int>();
            var edgeCutData = new Dictionary<int, SortedList<double, int>>();

            foreach (var edge in brep.Edges)
            {
                foreach (var x in Intersection.CurveCurve(
                    curve, edge, SD.IntersectTolerance, SD.OverlapTolerance))
                {
                    // add intersection event
                    int xi = xs.Count;
                    xs.Add(x);
                    
                    // use this to cut curve A 
                    trimData.Add(x.ParameterA, xi);

                    // use this to cut B curves;
                    int ei = edge.EdgeIndex;
                    if (!edgeCutData.ContainsKey(ei))
                        edgeCutData.Add(ei, new SortedList<double, int>());
                    edgeCutData[ei].Add(x.ParameterB, xi);
                }
            }

            if (xs.Count == 0)
                return false;

            if (xs.Count == 1)
                throw new Exception("does this work with only 1? It should not");

            // 2 | cut the edges of the brep in the correct order
            //   | find all new vertices
            var newVertices = new Dictionary<int, int>();
            var newEdges = new Dictionary<int, IndexPair>();
            foreach (var edgeIndex in edgeCutData.Keys)
            {
                // per intersection of an edge
                int ei = edgeIndex;
                foreach (int xi in edgeCutData[edgeIndex].Values)
                {
                    // TODO : Implement overlap 
                    var x = xs[xi];
                    brep.Edges.SplitEdgeAtParameters(
                        ei,
                        new double[1] { x.ParameterB }
                    );

                    // splitting an edge once always results in 1 newly added vertex, and 1 newly added edge
                    // TODO : CHECK IF THIS ASSUMPTION IS ALWAYS CORRECT
                    int newVertexIndex = brep.Vertices.Count - 1; // the latest
                    int newEdgeIndex = brep.Edges.Count - 1; // the latest

                    
                    newVertices.Add(xi, newVertexIndex);
                    newEdges.Add(xi, new IndexPair(ei, newEdgeIndex));
                    ei = newEdgeIndex;
                    // debug
                    Debug.Log($"added vertex {newVertexIndex}");        
                }         
            }

            // 3 | take all sorted intersection events, and build 'curve fragments' from them : 
            //   | trimmed curves that fall within 1 face.
            //   | fragments are stored in a dict per face
            Debug.Log($"found {trimData.Count} intersections");
            for (int i = 0; i < trimData.Count; i++)
            {
                var xiFrom = trimData.Values[i];
                var xFrom = xs[xiFrom];
                var vertexFrom = newVertices[xiFrom];
                var edgesFrom = newEdges[xiFrom];

                var xiTo = trimData.Values[(i + 1) % trimData.Count];
                var xTo = xs[xiTo];
                var vertexTo = newVertices[xiTo];
                var edgesTo = newEdges[xiTo];

                Debug.Log($"processing: vFrom {vertexFrom} - vTo {vertexTo}");

                // join first & last trim if curve is closed
                Curve trim = null;
                if (curve.IsClosed && i == trimData.Count - 1)
                {
                    Debug.Log($"special");
                    var firstLastCurves = Curve.JoinCurves(new Curve[2] {
                        curve.Trim(xFrom.ParameterA, curve.Domain.T1),
                        curve.Trim(curve.Domain.T0, xTo.ParameterA)
                    });

                    if (firstLastCurves.Length != 1)
                        throw new Exception("got multiple curves or no curves, should not happen");
                    trim = firstLastCurves[0];
                }
                else
                {
                    // normal case 
                    trim = curve.Trim(xFrom.ParameterA, xTo.ParameterA);
                }
   
                if (!TryMatchCurveToFace(brep, trim, out int faceID, 0.5))
                    throw new Exception("got no face, should not happen");

                // get trimIndexes in the edge order. 
                int a = GetTrimFromEdge(ref brep, edgesFrom.I, faceID, out bool aReversed);
                int b = GetTrimFromEdge(ref brep, edgesFrom.J, faceID, out bool bReversed);
                int c = GetTrimFromEdge(ref brep, edgesTo.I,   faceID, out bool cReversed);
                int d = GetTrimFromEdge(ref brep, edgesTo.J,   faceID, out bool dReversed);

                Debug.Log($"reversals : {aReversed}, {bReversed}, {cReversed}, {dReversed}");
                if (aReversed != bReversed) Debug.Log("this would be weird");
                if (cReversed != dReversed) Debug.Log("this would also be weird");

                if (aReversed)
                    Swap(ref a, ref b);
                if (cReversed)
                    Swap(ref c, ref d);
            

                var frag = new CurveFragment(
                    trim,
                    faceID, 
                    vertexFrom, 
                    vertexTo,
                    a,
                    b,
                    c,
                    d
                );

                // add it to mapped fragments
                if (mappedFragments.ContainsKey(frag.face))
                    mappedFragments[frag.face].Add(frag);
                else
                    mappedFragments.Add(frag.face, new List<CurveFragment>() { frag });
            }
            
            // mappedFragments should now be richer
            return true;
        }


        // HELPER | This is a preprocessing step. 
        // - take all edges interacting with fragments, and split them accordingly. 
        // - Add data of new elements created to the corresponding curveFragments
        // - also return a mapping so a face can find all fragments found on that face. 
        //private static Brep SplitEdges(Brep brep, List<CurveFragment> fragments,
        //    ref Dictionary<int, List<CurveFragment>> mappedFragments)
        //{
        //    // TODO : cleanup all unused data

        //    // first, create the mapping
        //    var mapping = new Dictionary<int, SplitResult>();
        //    for (int i = 0; i < fragments.Count; i++)
        //    {
        //        var fragment = fragments[i];
        //        var fragmentBefore = (i > 0) ? fragments[i - 1] : fragments[fragments.Count - 1];

        //        int vCount = brep.Edges.Count;
        //        int edgeID = fragment.ab;
        //        brep.Edges.SplitEdgeAtParameters(
        //            fragment.ab,
        //            new double[1] { fragment.paramFrom });

        //        var edgeID2 = brep.Edges.Count - 1;
        //        var e1 = brep.Edges[edgeID];
        //        var e2 = brep.Edges[edgeID2];
        //        var faceA = e1.AdjacentFaces()[0];
        //        var faceB = e1.AdjacentFaces()[1];

        //        var addedVertex = e1.EndVertex.VertexIndex;
        //        fragment.vertexFrom = addedVertex;
        //        fragmentBefore.vertexTo = addedVertex;

        //        var ti1 = e1.TrimIndices();
        //        var ti2 = e2.TrimIndices();

        //        // give fragment a notion of how these edges are split up

        //        var a = GetTrimFromEdge(ref brep, edgeID, faceA);
        //        var b = GetTrimFromEdge(ref brep, edgeID2, faceA);
        //        var c = GetTrimFromEdge(ref brep, edgeID, faceB);
        //        var d = GetTrimFromEdge(ref brep, edgeID2, faceB);

        //        var sr = new SplitResult(
        //            edgeID,
        //            faceA,
        //            faceB,
        //            a,
        //            b,
        //            c,
        //            d
        //        );



        //        Debug.Log($"edge {edgeID} has become : {sr}");
        //        // Debug.Log($"added {addedVertex} to dictionary");

        //        mapping.Add(addedVertex, sr);
        //    }

        //    // second, use the mapping to add extra data to the curve fragments
        //    // add it to mapped fragments

        //    foreach (var frag in fragments)
        //    {
        //        // extract the mapping
        //        mapping[frag.vertexFrom].GetTrims(frag.face, out int a, out int b);
        //        mapping[frag.vertexTo].GetTrims(frag.face, out int c, out int d);

        //        // add it to the fragments themselves
        //        frag.SetAfterSplitData(frag.vertexFrom, frag.vertexTo, a, b, c, d);
        //        Debug.Log($" set: {frag.a}, {frag.b}, {frag.c}, {frag.d}");

        //        // add it to mapped fragments
        //        if (mappedFragments.ContainsKey(frag.face))
        //            mappedFragments[frag.face].Add(frag);
        //        else
        //            mappedFragments.Add(frag.face, new List<CurveFragment>() { frag });
        //    }

        //    return brep;
        //}


        // HELPER | the process of adding one curve fragment to one face.

        
        private static Brep AddFragmentsToFace(Brep brep, int faceID, List<CurveFragment> frags, 
            ref List<int> addedFaces, ref List<int> removeFaces)
        {
            var oldFace = brep.Faces[faceID];
            var surface = oldFace.SurfaceIndex;

            Debug.Log($"adding fragments to face {faceID}");

            // Do a whole bunch of work to eventually, just add the new curve to all the correct lists
            // in the correct way

            // use this to correctly traverse the loops, and add new edges to the loops
            var flc = new FaceLoopCollection(oldFace);
            var newEdges = new Dictionary<int, int>();
            flc.PrintDictionary();

            int counter = 0;
            foreach (var frag in frags)
            {
                counter -= 2;
                Debug.Log(frag.ToString());

                flc.AddTwoWayBridge(counter + 1, counter, frag.a, frag.b, frag.c, frag.d);

                // TODO change this if we know it does the same bloody thing.
                var fragmentEdge = brep.AddEdge(frag.fragment, frag.vertexFrom, frag.vertexTo);
                newEdges.Add(counter, fragmentEdge);
            }

            flc.PrintDictionary();

            foreach (var trimSequence in flc.CreateNewLoops())
            {
                // this should recreate the surface
                var face = brep.AddFace(surface);
                var loop = brep.AddLoop(face, BrepLoopType.Outer);
                foreach (var trimID in trimSequence)
                {
                    if (trimID < 0)
                    {
                        // a new edge must be added 
                        if (Math.Abs(trimID) % 2 == 1)
                        {
                            // an odd negative integer represent regularly oriented edge
                            // Debug.Log($"now asking for {trimID-1}");
                            int fragmentEdge = newEdges[trimID - 1];
                            var fragmentEdge2D = brep.AddCurve2DBasic(fragmentEdge, surface, false);
                            brep.AddTrim(fragmentEdge, loop, false, fragmentEdge2D, IsoStatus.None, BrepTrimType.Mated);
                            addedFaces.Add(face); // if this trim is used, it means that the attacted face is a new one.
                        }
                        else
                        {
                            // an even negative integer represent reversed edge
                            // Debug.Log($"asking for {trimID}");
                            int fragmentEdge = newEdges[trimID];
                            var fragmentEdge2DFlipped = brep.AddCurve2DBasic(fragmentEdge, surface, true);
                            brep.AddTrim(fragmentEdge, loop, true, fragmentEdge2DFlipped, IsoStatus.None, BrepTrimType.Mated);
                        }

                    }
                    else
                    {
                        AddOldTrimToNewLoop(ref brep, brep.Trims[trimID], loop);
                    }
                }

                // debug 
                var debug = "Sequence: ";
                foreach (var trimID in trimSequence)
                {
                    debug += $"{trimID}, ";
                }
                Debug.Log(debug);
            }

            // remove old face
            removeFaces.Add(faceID);
            return brep;
        }


        // UTILITY | 
        private static void AddOldTrimToNewLoop(ref Brep brep, BrepTrim oldTrim, int newLoop)
        {
            int curve2d = brep.Curves2D.Add(oldTrim.DuplicateCurve());
            var newTrim = brep.AddTrim(
                oldTrim.Edge.EdgeIndex,
                newLoop,
                oldTrim.IsReversed(),
                curve2d,
                oldTrim.IsoStatus,
                oldTrim.TrimType);
        }


        // UTILITY | this is how to fully replace a face.
        private static Brep ReplaceFaceTemplate(Brep brep, int face)
        {
            // simulate the whole procedure, to see if this works correctly 
            var oldFace = brep.Faces[face];
            var surface = oldFace.SurfaceIndex;

            var newFace = brep.AddFace(surface);
            foreach (var oldLoop in oldFace.Loops)
            {
                var newLoop = brep.AddLoop(newFace, oldLoop.LoopType);
                foreach (var oldTrim in oldLoop.Trims)
                {
                    int curve2d = brep.Curves2D.Add(oldTrim.DuplicateCurve());
                    var newTrim = brep.AddTrim(
                        oldTrim.Edge.EdgeIndex,
                        newLoop,
                        oldTrim.IsReversed(),
                        curve2d,
                        oldTrim.IsoStatus,
                        oldTrim.TrimType);

                    Debug.Log("trim: " + oldTrim.TrimIndex.ToString());
                    Debug.Log("oldTrim.ProxyCurveIsReversed: " + oldTrim.ProxyCurveIsReversed.ToString());
                    Debug.Log("oldTrim.IsReversed(): " + oldTrim.IsReversed().ToString());
                    // Debug.Log("isTrimReversedEdge: " + isTrimReversedEdge.ToString());
                }
            }

            brep = brep.RemoveFace(face);
            return brep;
        }


        // UTILITY | 
        private static bool TryGetCommonFace(Brep brep, int edgeFrom, int edgeTo, out int face)
        {
            face = -1;
            var faces1 = brep.Edges[edgeFrom].AdjacentFaces();
            var faces2 = brep.Edges[edgeTo].AdjacentFaces();
            foreach (int face1 in faces1)
            {
                foreach (int face2 in faces2)
                {
                    if (face1 == face2)
                    {
                        face = face1;
                        return true;
                    }                 
                }
            }
            return false;
        }


        // UTILITY | based on a curves' starting point, judge which face it is a part of.
        private static bool TryMatchCurveToFace(Brep brep, Curve curve, out int match, double normParam = 0.0)
        {
            match = -1;

            var curveT = curve.Domain.ParameterAt(normParam);

            var succes = brep.ClosestPoint(
                curve.PointAt(curveT),
                out Point3d closestPoint,
                out ComponentIndex ci,
                out double s,
                out double t,
                SD.PointTolerance,
                out Vector3d normal);

            if (!succes)
                return false;

            switch (ci.ComponentIndexType)
            {
                case ComponentIndexType.BrepFace:
                    match = ci.Index;
                    return true;
                case ComponentIndexType.BrepEdge:
                    Debug.Log("curve starting point on edge!!");
                    match = brep.Edges[ci.Index].AdjacentFaces()[0];
                    return true;
                case ComponentIndexType.BrepVertex:
                    Debug.Log("curve starting point on vertex!!");
                    var edgeIndex = brep.Vertices[ci.Index].EdgeIndices()[0];
                    match = brep.Edges[edgeIndex].AdjacentFaces()[0];
                    return true;
                default:
                    Debug.Log("this should not happen!!");
                    return false;
            }
        }

        // UTIL 
        private static int[] GetTrimsAtVertex(BrepVertex v, int fi)
        {
            // 
            Debug.Log($" face given to vertex: {fi} ");
            var eis = v.EdgeIndices();
            if (eis.Length != 2)
            {
                var str = "";
                str += $"vertex: {v.VertexIndex}";
                foreach (var e in eis)
                {
                    str += "edgeIndex: " + e.ToString();
                }

                throw new Exception("did not find two edgeindices. " + str);
            }
                
            // 
            var brep = v.Brep;
            int a = GetTrimFromEdge(ref brep, eis[0], fi, out var _);
            int b = GetTrimFromEdge(ref brep, eis[1], fi, out _);

            // sort by trim direction (THIS DOES NOT WORK IF ANYTHING IS FLIPPED BE MINDFULL)
            if (     RoughlyEqual(brep.Trims[a].PointAtEnd, brep.Trims[b].PointAtStart))
                return new int[2] { a, b };
            else if (RoughlyEqual(brep.Trims[a].PointAtEnd, brep.Trims[b].PointAtStart))
                return new int[2] { b, a };
            else
                throw new Exception("trims are not correctly alligned...");
        }

        // UTIL
        private static bool RoughlyEqual(Point3d a, Point3d b)
        {
            return Math.Abs(a.X - b.X) < SD.Tolerance &&
                   Math.Abs(a.Y - b.Y) < SD.Tolerance &&
                   Math.Abs(a.Z - b.Z) < SD.Tolerance;
        }

        // UTILITY | give the trim that corresponds to both the edge and face
        private static int GetTrimFromEdge(ref Brep brep, int edge, int face, out bool reversed)
        {
            var trims = brep.Edges[edge].TrimIndices();
            if (trims.Length == 1)
            {
                reversed = brep.Trims[trims[0]].IsReversed();
                return trims[0];
            }
            else
            {
                // Debug.Log($" face given: {face} ");
                foreach (var trim in trims)
                {
                    // Debug.Log($" face: {brep.Trims[trim].Face.FaceIndex} ");
                    if (brep.Trims[trim].Face.FaceIndex == face)
                    {
                        reversed = brep.Trims[trim].IsReversed();
                        return trim;
                    }
                }
                throw new Exception("edge and face do not match");
            }
        }

        static void Swap<T>(ref T x, ref T y)
        {

            T tempswap = x;
            x = y;
            y = tempswap;
        }
    }
}
