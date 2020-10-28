using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SferedApi
{
    public class ComponentDevPythonInject : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComponentDevPythonInject class.
        /// </summary>
        public ComponentDevPythonInject()
          : base(SD.Starter + "Python Injector",
                    SD.Starter + "Py",
                    SD.CopyRight + "",
                    SD.PluginTitle,
                    SD.PluginCategory0)
        {
            IconDisplayMode = GH_IconDisplayMode.icon;
            MutableNickName = false;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("code", "code", "code", GH_ParamAccess.item);
            pManager.AddGenericParameter("json", "json", "json", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string codeString = Properties.Resources.TextFile1;
            string jsonString = Properties.Resources.json1;
            DA.SetData(0, codeString);
            DA.SetData(1, jsonString);
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
                return Properties.Resources.Image1;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("03f46200-f00d-439d-911d-75c799e95019"); }
        }
    }
}