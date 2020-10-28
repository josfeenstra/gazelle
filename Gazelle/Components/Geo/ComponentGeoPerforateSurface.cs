using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace SferedApi.Components.Developer
{
    public class ComponentGeoPerforateSurface : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComponentTextSplitSurface class.
        /// </summary>
        public ComponentGeoPerforateSurface()
          : base(SD.Starter + "Perforate Surface",
                    SD.Starter + "Perf Srf",
                    SD.CopyRight + "Split a surface with curves on the surface, and Determine the leftover geometry",
                    SD.PluginTitle,
                    SD.PluginCategory4)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("surface", "S", "surface to split", GH_ParamAccess.item);
            pManager.AddCurveParameter("curves", "C", "curves to cut with", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Leftover Surface(s)", "S", "Will return multiple surfaces if the surface is cut in half", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Holes", "H", "Holes", GH_ParamAccess.list);

            // test
            pManager.AddGenericParameter("Test", "T", "Test", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // pre input 
            var Tolerance = 0.001;

            // input
            Brep brep = null;
            List<Curve> curves = new List<Curve>();
            DA.GetData(0, ref brep);
            DA.GetDataList(1, curves);
            
            // process
            var leftover = new List<Brep>();
            var holes = new List<Brep>();

            // test
            var test = new List<object>();
            var testMessage = new List<string>();

            if (brep.Faces.Count == 1)
            {
                // perforate single surface
                Message = "Surface";
                bool s = Helpers.PerforateSurface(curves, brep, Tolerance, out leftover, out holes, out test);
                if (!s)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Something went wrong");
            }
            else
            {
                // perforate multiple surfaces 
                Message = "Brep";
                
                bool s = Helpers.PerforateBrep(curves, brep, Tolerance, out leftover, out holes, out test);
                if (!s)
                {
                    DA.SetDataList(2, test);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Something went wrong");
                }
            }
            // output 
            DA.SetDataList(0, leftover);
            DA.SetDataList(1, holes);

            // test
            DA.SetDataList(2, test);
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
                return Properties.Resources.Perforate;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("90eac23f-d8c7-4fe3-b530-15a247e34866"); }
        }
    }
}