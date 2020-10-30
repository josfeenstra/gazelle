using System;
using System.Collections.Generic;
using Gazelle.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Gazelle.Components
{
    public class SplitBrepPlus : GH_Component
    {
        protected override System.Drawing.Bitmap Icon => Resources.Sfered_Iconified;

        public override Guid ComponentGuid => new Guid("d065afed-c680-471b-b2f1-c5693a3b627c");

        /// <summary>
        /// Initializes a new instance of the SplitBrepPlus class.
        /// </summary>
        public SplitBrepPlus()
          : base(SD.Starter + "Split Brep Plus",
                SD.Starter + "Split",
                SD.CopyRight + "Split a brep with closed, rightly oriented curves, " +
                "and save a lot of time compared to the regular split brep.",
                SD.PluginTitle,
                SD.PluginCategory8)
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", (GH_ParamAccess)0);
            pManager.AddCurveParameter("Curve", "C", "closed, on-surface curve to trim with", (GH_ParamAccess)1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "New Brep with the addition", (GH_ParamAccess)0);
            pManager.AddIntegerParameter("Face Indices", "Fi", "Indices of new faces", (GH_ParamAccess)1);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            int face = -1;
            List<Curve> curves = new List<Curve>();
            DA.GetData<Brep>(0, ref brep);
            DA.GetDataList<Curve>(1, curves);

            if (((brep == null)) || object.ReferenceEquals(curves, null))
            {
                this.Error("Input Bad");
                return;
            }

            brep = BrepSplitFunctions.SplitBrepWithCurves(brep, curves, out List<int> faces);
                
            DA.SetData(0, brep);
            DA.SetDataList(1, faces);
        }
    }
}