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
}
