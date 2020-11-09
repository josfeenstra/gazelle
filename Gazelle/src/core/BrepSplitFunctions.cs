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
        public static Brep SplitBrepWithCurves(this Brep brep, List<Curve> curves, out List<int> createdFaces)
        {
            Debug.Flush();
            createdFaces = new List<int>();
            foreach (var curve in curves)
            {
                brep.SplitBrepWithCurve(curve, out List<int>newFaces);
                createdFaces.AddRange(newFaces);
            }
            return brep;
        }

        // UTIL 

        /// <summary>
        /// based on a curves' starting point, judge which face it is a part of.
        /// </summary>
        public static bool TryMatchCurveToFace(Brep brep, Curve curve, out int match)
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
        public static Brep SplitBrepWithCurve(this Brep brep, Curve curve, out List<int> createdFaces)
        {
            createdFaces = new List<int>();

            var succes = TryGetFragments(brep, curve, out List<CurveFragment> fragments);
            if (succes)
            {
                foreach (var fragment in fragments)
                {
                    var adds = AddFragment(brep, fragment);
                    Debug.Log(fragment.ToString());
                    Debug.LogGeo(fragment.fragment);
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

        public static List<int> AddFragment(Brep brep, CurveFragment fragment)
        {
            var addedFaces = new List<int>();
            var oldFace = brep.Faces[fragment.face];
            var surface = oldFace.SurfaceIndex;

            // simulate the whole procedure, to see if this works correctly 

            brep.RemoveFace(fragment.face);

            var newFace = brep.AddFace(surface);
            foreach (var oldLoop in oldFace.Loops)
            {
                var newLoop = brep.AddLoop(newFace, oldLoop.LoopType);
                foreach (var oldTrim in oldLoop.Trims)
                {
                    var newTrim = brep.AddOrientedTrim(
                        oldTrim.Edge.EdgeIndex, 
                        newLoop, 
                        oldTrim.IsoStatus, 
                        oldTrim.TrimType);
                }
            }
            return addedFaces;
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

            // do complicated shit for the first & last
            var first = data.Values[0];
            var last = data.Values[data.Count-1];
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
