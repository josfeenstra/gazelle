using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SferedApi.Components.Meta
{
    public class ComponentDevPluginInfo : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComponentMetaPluginInfo class.
        /// </summary>
        public ComponentDevPluginInfo()
          : base(   SD.Starter + "Get Meta Info",
                    SD.Starter + "Meta",
                    SD.CopyRight + "This component is used to find out meta information about the plugin itself.",
                    SD.PluginTitle,
                    SD.PluginCategory0)
        {
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
            pManager.AddGenericParameter("Info", "Info", "All plugin info", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var i = new GazelleInfo();
            string text =   "SferedApi.SferedApiInfo\n" +
                            "   Version: " + i.Version + "\n" +
                            "   AssemblyVersion: " + i.AssemblyVersion + "\n" +
                            "SferedApi.Properties.AssemblyInfo\n" +
                            "   AssemblyVersion: " + "w.i.p." + "\n" +
                            "   AssemblyFileVersion: " + "w.i.p." + "\n";
            DA.SetData(0, text);
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
                return Properties.Resources.info;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("42512bbb-6995-4a3d-a498-3d271c644a56"); }
        }
    }
}