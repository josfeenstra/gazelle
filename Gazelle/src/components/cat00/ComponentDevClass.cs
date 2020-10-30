using System;
using System.Collections.Generic;
using System.Dynamic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Gazelle
{
    public class ComponentDevClass : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentDevClass()
          : base(SD.Starter + "Trying to export a datanode object to grasshopper",
                    SD.Starter + "",
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
            pManager.AddGenericParameter("Node", "Node", "I hope you know what ur doing :)", GH_ParamAccess.item);
            pManager.AddGenericParameter("SuperNode", "SuperNode", "I hope you know what ur doing :)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // static input 
            var dict = new Dictionary<string, object>();
            dict.Add("value1", "1.0");
            dict.Add("value2", "4.0");
            dict.Add("value3", "9.0");

            var Metadict = new Dictionary<string, object>();
            Metadict.Add("value4", new Dictionary<string, object>(dict));
            Metadict.Add("value5", new Dictionary<string, object>(dict));
            Metadict.Add("value6", new Dictionary<string, object>(dict));

            // output
            var datanode = new Datatypes.DataNode(dict);
            var outputparam = new Datatypes.GH_DataNode(datanode);
            

            var datanode2 = new Datatypes.DataNode(Metadict);
            var test = datanode2.GetFlatObject();
            var outputparam2 = new Datatypes.GH_DataNode(datanode2);
            DA.SetData(0, test);
            DA.SetData(1, outputparam2);
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
            get { return new Guid("e27d34d2-add9-447e-a57d-789d65da03f3"); }
        }
    }
}