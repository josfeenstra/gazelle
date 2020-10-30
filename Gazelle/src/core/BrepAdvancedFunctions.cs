using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;
using Grasshopper;

namespace Gazelle
{
    internal static class BrepAdvancedFunctions
    {
        // same as GH's Split Brep
        public static Brep SplitBrep(Brep brep, List<Curve> curves)
        {
            Debug.LogGeo(curves);
            return null;
        }
    }
}
