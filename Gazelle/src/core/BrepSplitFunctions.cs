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

            brep = BrepFunctions.DeepCopy(brep, SD.Tolerance);

            // turn curves into mapped fragments, and split brep edges with it
            var mappedFragments = new Dictionary<int, List<CurveFragment>>();
            var newEdges = new Dictionary<int, IndexPair>();
            foreach (var curve in curves)
            {
                SplitBrepEdgesWithCurves(ref brep, curve, ref mappedFragments, ref newEdges);
            }
            PostProcessFragments(ref brep, ref mappedFragments);

            // perform all fragments
            createdFaces = new List<int>();
            var oldFaces = new List<int>();

            brep.Standardize();
            Debug.LogGeo(brep);
            foreach (var face in mappedFragments.Keys)
            {
                brep = AddFragmentsToFace(brep, face, mappedFragments[face], ref newEdges,
                    ref createdFaces, ref oldFaces);
            }

            // TODO : signal all faces surrounded by new faces as 'new'



            // remove the faces that recieved new loops
            var nOldFaces = oldFaces.Count;
            brep = brep.RemoveFaces(oldFaces);

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
        private static void SplitBrepEdgesWithCurves(ref Brep brep, Curve curve, 
            ref Dictionary<int, List<CurveFragment>> mappedFragments, ref Dictionary<int, IndexPair> newEdges)
        {
            var succes = TryGetFragments(ref brep, curve, ref mappedFragments, ref newEdges);
            if (succes)
            {
                // option 1 : cuts are made
            }
            else if (TryMatchCurveToFace(brep, curve, out int faceIndex))
            {
                // option 2
                // Debug.Log("match: " + faceIndex.ToString());

                // add it to mapped fragments
                var frag = new CurveFragment(curve, faceIndex);
                if (!mappedFragments.ContainsKey(faceIndex))
                    mappedFragments.Add(faceIndex, new List<CurveFragment>());
                mappedFragments[faceIndex].Add(frag);

                //int face = brep.BuildInnerFace(faceIndex, curve);
                //createdFaces.Add(face);
            }
            else
            {
                // option 3
                // do nothing
            }
        }

        // HELPER | 
        public static bool TryGetFragments(ref Brep brep, Curve curve, ref Dictionary<int, 
            List<CurveFragment>> mappedFragments, ref Dictionary<int, IndexPair> newEdges)
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
                    newEdges.Add(newVertexIndex, new IndexPair(ei, newEdgeIndex));
                    ei = newEdgeIndex;
                    // debug
                    // Debug.Log($"added vertex {newVertexIndex}");        
                }         
            }

            // 3 | take all sorted intersection events, and build 'curve fragments' from them : 
            //   | trimmed curves that fall within 1 face.
            //   | fragments are stored in a dict per face
            // Debug.Log($"found {trimData.Count} intersections");
            for (int i = 0; i < trimData.Count; i++)
            {
                var xiFrom = trimData.Values[i];
                var xFrom = xs[xiFrom];
                var vertexFrom = newVertices[xiFrom];
                // var edgesFrom = newEdges[vertexFrom];

                var xiTo = trimData.Values[(i + 1) % trimData.Count];
                var xTo = xs[xiTo];
                var vertexTo = newVertices[xiTo];
                // var edgesTo = newEdges[vertexTo];

                // Debug.Log($"processing: vFrom {vertexFrom} - vTo {vertexTo}");

                // join first & last trim if curve is closed
                Curve trim = null;
                if (curve.IsClosed && i == trimData.Count - 1)
                {
                    // Debug.Log($"special");
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

                //// get trimIndexes in the edge order. 
                //int a = GetTrimFromEdge(ref brep, edgesFrom.I, faceID, out bool aReversed);
                //int b = GetTrimFromEdge(ref brep, edgesFrom.J, faceID, out bool bReversed);
                //int c = GetTrimFromEdge(ref brep, edgesTo.I, faceID, out bool cReversed);
                //int d = GetTrimFromEdge(ref brep, edgesTo.J, faceID, out bool dReversed);

                //// Debug.Log($"reversals : {aReversed}, {bReversed}, {cReversed}, {dReversed}");
                //if (aReversed != bReversed) Debug.Log("this would be weird");
                //if (cReversed != dReversed) Debug.Log("this would also be weird");

                //if (aReversed)
                //    Swap(ref a, ref b);
                //if (cReversed)
                //    Swap(ref c, ref d);

                var frag = new CurveFragment(
                    trim,
                    faceID, 
                    vertexFrom, 
                    vertexTo
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



        private static void PostProcessFragments(ref Brep brep, ref Dictionary<int, List<CurveFragment>> mappedFragments)
        {
            // we want to know adjacent trims, but this data only makes sense 
            // after splitting up ALL edges 
            foreach (var face in mappedFragments.Keys)
            {
                foreach (var frag in mappedFragments[face])
                {
                    if (frag.type != CurveFragmentType.EdgeCutter) continue;
                    //Debug.Log($"brep.vertices.Count {brep.Vertices.Count}");
                    //Debug.Log($"frag.vertexFrom {frag.vertexFrom}");
                    //Debug.Log($"frag.vertexTo {frag.vertexTo}");

                    GetTrimsAtVertex(ref brep, frag.vertexFrom, face, out int a, out int b);
                    GetTrimsAtVertex(ref brep, frag.vertexTo, face, out int c, out int d);
                    frag.SetAfterwards(a, b, c, d);
                    Debug.Log(frag.ToString());
                }
            }
        }



        // HELPER | the process of adding one curve fragment to one face.
        private static Brep AddFragmentsToFace(Brep brep, 
                                               int faceID, 
                                               List<CurveFragment> frags, 
                                               ref Dictionary<int, IndexPair> changedEdges,
                                               ref List<int> createdFaces, ref List<int> removeFaces)
        {
            var oldFace = brep.Faces[faceID];
            var surface = oldFace.SurfaceIndex;

            Debug.Log($"adding fragments to face {faceID}");

            // Do a whole bunch of work to eventually, just add the new curve to all the correct lists
            // in the correct way

            // use this to correctly traverse the loops, and add new edges to the loops
            var flc = new FaceLoopCollection(oldFace);
            var newEdges = new Dictionary<int, int>();
            var addTheseFragsAfterwards = new List<CurveFragment>();
            flc.PrintDictionary();

            int counter = 0;
            foreach (var frag in frags)
            {
                if (frag.type != CurveFragmentType.EdgeCutter) continue;

                counter -= 2;
                flc.AddTwoWayBridge(counter + 1, counter, frag.a, frag.b, frag.c, frag.d);

                // TODO change this if we know it does the same bloody thing.
                var fragmentEdge = brep.AddEdge(frag.curve, frag.vertexFrom, frag.vertexTo);
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
                            createdFaces.Add(face); // if this trim is used, it means that the attacted face is a new one.
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

            // finally, add unattached loops to the newly created faces, whatever they might be
            foreach (var frag in frags)
            {
                if (frag.type != CurveFragmentType.Unattached) continue;
                if (!TryMatchCurveToFace(brep, frag.curve, out int newfaceIndex))
                    throw new Exception("somehow, this unattacked frag could not be mapped anymore");

                int face = brep.BuildInnerFace(newfaceIndex, frag.curve);
                createdFaces.Add(face);
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
        private static void GetTrimsAtVertex(ref Brep brep, int vi, int fi, out int first, out int last)
        {
            var trimIndices = new List<int>();
            var reversals = new List<bool>();
            foreach (var ei in brep.Vertices[vi].EdgeIndices())
            {
                if (TryGetTrimFromEdge(ref brep, ei, fi, out int ti, out bool reversed))
                {
                    trimIndices.Add(ti);
                    reversals.Add(reversed);
                }
            }

            if (trimIndices.Count != 2 || reversals.Count != 2)
                throw new Exception($"this should not be true. tis.Count {trimIndices.Count} | reversals.Count {reversals.Count}");

            first = trimIndices[0];
            last = trimIndices[1];
            var all = brep.Trims[first].Loop.Trims.Select(x => x.TrimIndex).ToList();
            bool swap = all.IndexOf(first) > all.IndexOf(last);

            Debug.Log("reversed: " + swap.ToString());
            if (swap)
                Swap(ref first, ref last);
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

        // UTILITY | give the trim that corresponds to both the edge and face
        private static bool TryGetTrimFromEdge(ref Brep brep, int edge, int face, out int trim, out bool reversed)
        {
            var trims = brep.Edges[edge].TrimIndices();
            if (trims.Length == 1)
            {
                reversed = brep.Trims[trims[0]].IsReversed();
                trim = trims[0];
                return true;
            }
            else
            {
                foreach (var t in trims)
                {
                    if (brep.Trims[t].Face.FaceIndex == face)
                    {
                        reversed = brep.Trims[t].IsReversed();
                        trim = t;
                        return true;
                    }
                }
                reversed = false;
                trim = 0;
                return false;
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
