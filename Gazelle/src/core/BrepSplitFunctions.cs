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
    /// These are all functions for the advanced brep splitting tech
    /// </summary>
    internal static class BrepSplitFunctions
    {
        public static Brep SplitBrepWithCurves(this Brep brep, List<Curve> curves, out List<int> createdFaces, out List<int> otherFaces)
        {
            Debug.Flush();

            brep = brep.DuplicateBrep();
            createdFaces = new List<int>();
            var allOldFaces = new List<int>();

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

        /// <summary>
        /// based on a curves' starting point, judge which face it is a part of.
        /// </summary>
        private static bool TryMatchCurveToFace(Brep brep, Curve curve, out int match)
        {
            match = -1;

            var succes = brep.ClosestPoint(
                curve.PointAtStart,
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



        /// <summary>
        /// // TODO : FIX INTERNAL HOLES -> CAN BE DONE BY PROCESSING ALL FRAGMENTS PER FACE AT THE SAME TIME
        /// Per Curve, 3 options: 
        /// 
        /// option 1:
        /// if curve intersects one or more edges: split the curve in intelligent ways, and add it.
        /// 
        /// option 2: 
        /// if curve has no intersections BUT 
        /// its starting point touches a face:
        ///    build inner face
        /// 
        /// option 3: 
        /// if curve has no intersections AND 
        /// its starting point is not touching any face, 
        ///    do nothing
        ///
        /// </summary>
        private static Brep SplitBrepWithCurve(this Brep brep, Curve curve, 
            out List<int> createdFaces,
            out List<int> oldFaces)
        {
            createdFaces = new List<int>();
            oldFaces = new List<int>();

            var succes = TryGetFragments(brep, curve, out var fragments);
            if (succes)
            {
                brep = SplitEdges(brep, fragments, out var mappedFragments);
                foreach (var face in mappedFragments.Keys)
                {
                    brep = AddFragmentsToFace(brep, face, mappedFragments[face], 
                        out var adds, out var removes);
                    createdFaces.AddRange(adds);
                    oldFaces.AddRange(removes);
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


        /// <summary>
        ///This is a preprocessing step. 
        /// - take all edges interacting with fragments, and split them accordingly. 
        /// - Add data of new elements created to the corresponding curveFragments
        /// - also return a mapping so a face can find all fragments found on that face. 
        /// </summary>
        private static Brep SplitEdges(Brep brep, List<CurveFragment> fragments, 
            out Dictionary<int, List<CurveFragment>> mappedFragments)
        {
            // TODO : cleanup all unused data

            // first, create the mapping
            var mapping = new Dictionary<int, SplitResult>();
            foreach (var fragment in fragments)
            {
                int vCount = brep.Edges.Count;
                int edgeID = fragment.ab;
                brep.Edges.SplitEdgeAtParameters(
                    fragment.ab, 
                    new double[1] { fragment.paramFrom });

                var edgeID2 = brep.Edges.Count - 1;
                var e1 = brep.Edges[edgeID];
                var e2 = brep.Edges[edgeID2];
                var faceA = e1.AdjacentFaces()[0];
                var faceB = e1.AdjacentFaces()[1];

                var ti1 = e1.TrimIndices();
                var ti2 = e2.TrimIndices();

                // give fragment a notion of how these edges are split up

                var a = GetTrimFromEdge(ref brep, edgeID, faceA);
                var b = GetTrimFromEdge(ref brep, edgeID2, faceA);
                var c = GetTrimFromEdge(ref brep, edgeID, faceB);
                var d = GetTrimFromEdge(ref brep, edgeID2, faceB);

                var sr = new SplitResult(
                    edgeID,
                    faceA,
                    faceB, 
                    a,
                    b,
                    c,
                    d
                );

                mapping.Add(edgeID, sr);

                Debug.Log($"edge has become : {sr}");
            }

            // second, use the mapping to add extra data to the curve fragments
            // add it to mapped fragments
            mappedFragments = new Dictionary<int, List<CurveFragment>>();
            foreach (var frag in fragments)
            {
                Debug.Log("hallo haai");
                // extract the mapping
                mapping[frag.ab].GetTrims(frag.face, out int a, out int b);
                mapping[frag.cd].GetTrims(frag.face, out int c, out int d);
                int vertexFrom = brep.Edges[a].EndVertex.VertexIndex;
                int vertexTo = brep.Edges[c].EndVertex.VertexIndex;

                // add it to the fragments themselves
                frag.SetAfterSplitData(vertexFrom, vertexTo, a, b, c, d);
                Debug.Log($" set: {frag.a}, {frag.b}, {frag.c}, {frag.d}");

                // add it to mapped fragments
                if (mappedFragments.ContainsKey(frag.face))
                    mappedFragments[frag.face].Add(frag);
                else
                    mappedFragments.Add(frag.face, new List<CurveFragment>() { frag });
            }

            return brep;
        }

        // the process of adding one curve fragment to one face.
        private static Brep AddFragmentsToFace(
            Brep brep,
            int faceID, 
            List<CurveFragment> frags,
            out List<int> addedFaces,
            out List<int> removeFaces)
        {
            // store 
            addedFaces = new List<int>();
            removeFaces = new List<int>();

            var oldFace = brep.Faces[faceID];
            var surface = oldFace.SurfaceIndex;

            Debug.Log($"adding fragments to face {faceID}");

            // Do a whole bunch of work to eventually, just add the new curve to all the correct lists
            // in the correct way

            // use this to correctly traverse the loops, and add new edges to the loops
            var flc = new FaceLoopCollection(oldFace);
            var newEdges = new Dictionary<int, int>();

            int counter = 0;
            foreach (var frag in frags)
            {
                counter -= 2;
                Debug.Log(frag.ToString());
                Debug.Log($" recieved the following parameters: {frag.a}, {frag.b}, {frag.c}, {frag.d}");

                // TODO change this if we know it does the same bloody thing.
                var fromVectexID = brep.Edges[frag.ab].EndVertex.VertexIndex;
                var toVertexID = brep.Edges[frag.cd].EndVertex.VertexIndex;
                var fragmentEdge = brep.AddEdge(frag.fragment, fromVectexID, toVertexID);

                newEdges.Add(counter, fragmentEdge);

                flc.PrintDictionary();

                // add to new 'trims'
                Debug.Log($"adding {counter+1}, ")
                flc.AddTwoWayBridge(counter+1, counter, frag.a, frag.b, frag.c, frag.d);
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
                        if (trimID % 2 == 1)
                        {
                            // an odd negative integer represent regularly oriented edge
                            int fragmentEdge = newEdges[trimID - 1];
                            var fragmentEdge2D = brep.AddCurve2DBasic(fragmentEdge, surface, false);
                            brep.AddTrim(fragmentEdge, loop, false, fragmentEdge2D, IsoStatus.None, BrepTrimType.Mated);
                            addedFaces.Add(face); // if this trim is used, it means that the attacted face is a new one.
                        }
                        else
                        {
                            // amn even negative integer represent reversed edge
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

        // give the trim that corresponds to both the edge and face
        private static int GetTrimFromEdge(ref Brep brep, int edge, int face)
        {
            var trims = brep.Edges[edge].TrimIndices();
            if (trims.Length == 1)
            {
                return trims[0];
            }
            else
            {
                foreach (var trim in trims)
                {
                    if (brep.Trims[trim].Face.FaceIndex == face)
                    {
                        return trim;
                    }
                }
                throw new Exception("edge and face do not match");
            }
        }

        // --- UTILITY

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

        // this is how to fully replace a face.
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

        public static bool TryGetFragments(Brep brep, Curve curve, out List<CurveFragment> fragments)
        {
            fragments = new List<CurveFragment>();
            var data = new SortedList<double, Tuple<IntersectionEvent, int>>();
            foreach (var edge in brep.Edges)
            {
                foreach (var x in Intersection.CurveCurve(
                    curve, edge, SD.IntersectTolerance, SD.OverlapTolerance))
                {
                    // DISREGARD OVERLAP FOR NOW
                    data.Add(x.ParameterA, new Tuple<IntersectionEvent, int>(x, edge.EdgeIndex));
                }
            }

            if (data.Count == 0)
                return false;

            if (data.Count == 1)
                throw new Exception("does this work with only 1? It should not");

            // do complicated shit for the first & last if curve is closed
            if (curve.IsClosed)
            {
                var first = data.Values[0];
                var last = data.Values[data.Count - 1];
                var firstLastCurves = Curve.JoinCurves(new Curve[2] {
               curve.Trim(last.Item1.ParameterA, curve.Domain.T1),
               curve.Trim(curve.Domain.T0, first.Item1.ParameterA)
            });

                if (firstLastCurves.Length != 1)
                    throw new Exception("got multiple curves or no curves, should not happen");

                if (!TryMatchCurveToFace(brep, curve, out int firstFace))
                    throw new Exception("got no face, should not happen");

                fragments.Add(new CurveFragment(
                    firstFace,
                    last.Item2,
                    first.Item2,
                    last.Item1.ParameterB,
                    first.Item1.ParameterB,
                    firstLastCurves[0]));
            }

            // basics
            for (int i = 0; i < data.Count-1; i++)
            {
                var from = data.Values[i];
                var xFrom = from.Item1;
                var edgeFrom = from.Item2;

                var to = data.Values[i+1];
                var xTo = to.Item1;
                var edgeTo = to.Item2;

                var fragment = curve.Trim(xFrom.ParameterA, xTo.ParameterA);
                if (!TryGetCommonFace(brep, edgeFrom, edgeTo, out int faceID))
                    throw new Exception("Subsequent edges do not share a face, that is not suppose to happen...");

                fragments.Add(new CurveFragment(
                    faceID, 
                    edgeFrom, 
                    edgeTo, 
                    xFrom.ParameterB, 
                    xTo.ParameterB, 
                    fragment));
            }
            
            // return the fragments
            return true;
        }

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
    }
}
