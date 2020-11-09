using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grasshopper;

namespace SferedApi.src.core
{



    internal static class PolygonFunctions
    {
        internal enum Relation
        {
            None,
            Entering,
            Leaving 
        }

        internal static void RhinoClip(Curve[] a, Curve[] b)
        {



        }

        internal static void Clip(Point3d[] polygonA, Point3d[] polygonB)
        {
            // Weiler Atherton



        }



        internal static bool TryIntersect(
            Point3d a, Point3d b, Point3d c, Point3d d, out Point3d x, out Relation relation)
        {
            // Cyrus Beck
            x = Point3d.Unset;
            relation = Relation.None;

            return false;
        }

        internal static bool TryIntersect(
            Curve ab, Curve cd, out Point3d x, out Relation relation)
        {
            // Cyrus Beck
            x = Point3d.Unset;
            relation = Relation.None;

            return false;
        }
    }
}
