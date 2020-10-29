using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SferedApi.Components.Geo
{
    public class ComponentGeoAsymmetricOffset : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComponentGeoAsymmetricOffset class.
        /// </summary>
        public ComponentGeoAsymmetricOffset()
          : base(   SD.Starter + "asymmetric Offset",
                    SD.Starter + "Asym Off",
                    SD.CopyRight + "Move a curve along a vector, and create Sinus-Like transition between the higher and original curves.",
                    SD.PluginTitle,
                    SD.PluginCategory4)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "curve to raise", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vector", "V", "Vector to raise curve with", GH_ParamAccess.item, Vector3d.Zero);
            pManager.AddIntegerParameter("N controlpoints / degree", "N", "number of controlpoints to do raise with", GH_ParamAccess.item, (GH_ParamAccess)3);
            pManager.AddNumberParameter("Factor", "F", "use this factor to amplyfy the ramping process. IF you use this, keep contolpoints to a minimum!", GH_ParamAccess.item, (GH_ParamAccess)0);
            pManager.AddNumberParameter("Halfpoint", "H", "use this factor to change where the curve flips, to get other curve shapes. Must be used in conjunction with factor!", GH_ParamAccess.item, 0.5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "sinus-like curve created out of the two fractions", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input
            Curve curve = null;
            Vector3d vector = Vector3d.Zero;
            int degree = 3;   // MUST BE ODD (?)
            double factor = 0;
            double halfPoint = 0.5;
            DA.GetData(0, ref curve);
            DA.GetData(1, ref vector);
            DA.GetData(2, ref degree);
            DA.GetData(3, ref factor);
            DA.GetData(4, ref halfPoint);

            // process
            if (degree % 2 != 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "component works best if degree is odd!");
            }
            double max = degree - 1;

            // calculate invidivdual displacement vectors 
            var displacementFactors = new List<double>();
            for (int i = 0; i < degree; i++) 
            {
                // cosinify a [degree] amount of numbers between 0 and 1
                double value = Cosinify(i / max, factor, halfPoint);
                displacementFactors.Add(value);
            }

            // create the points of the new polyline
            var points = new List<Point3d>();
            var tValues = curve.DivideByCount(degree - 1, true);
            for (int i = 0; i < degree; i++)
            {
                // get data needed for this interval of the curve
                var t = tValues[i];
                var disFac = displacementFactors[i];
                var curvePoint = curve.PointAt(t);

                // determine new positioning 
                var newPoint = curvePoint + vector * disFac;
                points.Add(newPoint);
            }

            // new curve
            var outCurve = Curve.CreateInterpolatedCurve(points,
                                                         3,
                                                         CurveKnotStyle.Chord,
                                                         curve.TangentAtStart,
                                                         curve.TangentAtEnd);

            // output 
            DA.SetData(0, outCurve);
        }

        // method to cosinify value 
        private static double Cosinify(double value, double factor = 0, double half = 0.5)
        {
            // 0 -> 0
            // 0.5 -> 0.5
            // 1 -> 1
            // rest -> like a nice sinus curve

            var pi = Math.PI;
            var ans = Math.Cos(value * pi + pi);
            ans = (ans + 1) / 2;

            // apply factor
            if (value < half)
                ans += -1 * ans * factor;
            else if (value > half)
                ans += (1 - ans) * factor;

            // succes
            return ans;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.AsymOffset;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0760a3cd-145f-48c4-ac8f-1f21a9da5359"); }
        }
    }
}