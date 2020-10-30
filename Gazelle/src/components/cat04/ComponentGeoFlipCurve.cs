using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Gazelle.Components.Geo
{
    public class ComponentGeoFlipCurve : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentGeoFlipCurve()
          : base(SD.Starter + "Flip Curve Based upon TangentAtStart",
                    SD.Starter + "Flip",
                    SD.CopyRight + "Flip a curve based on comparing the tangent at start",
                    SD.PluginTitle,
                    SD.PluginCategory4)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve","C","Curve to Flip", GH_ParamAccess.item);
            pManager.AddCurveParameter("Guide Curve", "Ct", "Curve to Test With", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "input curve, possibly flipped.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Bool", "B", "if True, this curve is flipped.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input
            Curve curve = null;
            Curve guideCurve = null;
            DA.GetData(0, ref curve);
            DA.GetData(1, ref guideCurve);

            // calculate angles
            var angle        = Vector3d.VectorAngle(guideCurve.TangentAtStart, curve.TangentAtStart);
            var reverseAngle = Vector3d.VectorAngle(guideCurve.TangentAtStart, curve.TangentAtStart * -1);

            // if the angle of the reverse is smaller, curve should be flipped
            var boolean = reverseAngle < angle;
            if (boolean)
                curve.Reverse();

            // output
            DA.SetData(0, curve);
            DA.SetData(1, boolean);     
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("217afeed-6dc0-44a3-b8f2-7e2cdd16dec3"); }
        }
    }
}