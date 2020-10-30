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
            var xs = new List<IntersectionEvent>();
            var ids = new List<int>();
            foreach (var edge in brep.Edges)
            {
                foreach (var x in Intersection.CurveCurve(
                    curve, edge, SD.IntersectTolerance, SD.OverlapTolerance))
                {
                    xs.Add(x);
                    ids.Add(edge.EdgeIndex);
                }
            }
            if (xs.Count > 0)
            {
                IncorporateForeignCurve(brep, curve, xs, ids);
            }
            else
            {
                int faceIndex = MatchCurveToFace(brep, curve);
                if (faceIndex != -1)
                {
                    int face = brep.BuildInnerFace(faceIndex, curve); // 2
                    createdFaces.Add(face);
                }
            }
            return brep;
        }

        private static void IncorporateForeignCurve(
            Brep brep, 
            Curve curve, 
            List<IntersectionEvent> xs, 
            List<int> ids)
        {
            // get all unique faces
            var faceHash = new HashSet<int>();
            foreach (var i in ids)
                foreach (var j in brep.Edges[i].AdjacentFaces())
                    faceHash.Add(j);
            var faces = faceHash.ToArray();
            
            // per face

            Debug.LogGeo(curve);
            
        }

        private static void IncorporateCurveFragment(BrepLoop loop, Curve fragment)
        {
            // split loop with fragment
            foreach (BrepTrim trim in loop.Trims)
            {
                //
            }
        }

        /// <summary>
        /// based on a curves' starting point, judge which face it is a part of.
        /// </summary>
        public static int MatchCurveToFace(Brep brep, Curve curve)
        {
            var succes = brep.ClosestPoint(
                curve.PointAtStart,
                out Point3d closestPoint,
                out ComponentIndex ci,
                out double s,
                out double t,
                SD.PointTolerance,
                out Vector3d normal);

            if (!succes)
                return -1;

            switch (ci.ComponentIndexType)
            {
                case ComponentIndexType.BrepFace:
                    return ci.Index;
                case ComponentIndexType.BrepEdge:
                    return brep.Edges[ci.Index].AdjacentFaces()[0];
                case ComponentIndexType.BrepVertex:
                    var edgeIndex = brep.Vertices[ci.Index].EdgeIndices()[0];
                    return brep.Edges[edgeIndex].AdjacentFaces()[0];
                default:
                    Debug.Log("this should not happen!!");
                    return -1;
            }
        }


        // NOTE : obsolete thanks to brep.ClosestPoint();
        public static bool IsCurveTouchingFaceObsolete(Curve curve, BrepFace face)
        {
            // pulling to get UV
            var response = face.PullPointsToFace(
                new Point3d[1] { curve.PointAtStart },
                SD.Tolerance);
            if (response.Count() == 0)
            {
                Debug.Log("pull failed");
                return false;
            }
            else if (response.Count() != 1)
            {
                Debug.Log("multiple pull results??");
                return false;
            }

            double u = response[0].X;
            double v = response[0].Y;
            var rel = face.IsPointOnFace(u, v);
            return rel != PointFaceRelation.Exterior;
        }
    }
}
