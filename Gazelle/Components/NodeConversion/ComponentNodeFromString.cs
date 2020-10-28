using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SferedApi.Datatypes;

namespace SferedApi
{
    public class ComponentNodeFromString : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentNodeFromString()
          : base(SD.Starter + "Node parse Json String",
                    SD.Starter + "Json in",
                    SD.CopyRight + "Create a node object from a string containing json data",
                    SD.PluginTitle,
                    SD.PluginCategory2)
        {
            IconDisplayMode = GH_IconDisplayMode.icon;
            MutableNickName = false;
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("String", "S", "Json String", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Data Node", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input 
            var inString = "";
            DA.GetData(0, ref inString);

            // process: set the data of a new datanode to the json data
            var outNode = new GH_DataNode();
            var response = outNode.Value.SetJson(inString);
            if (response == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "couldn't convert dictionary");
                return;
            }

            // output
            DA.SetData(0, outNode);
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
                return Properties.Resources.FromJson;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("197d2ec4-c3b1-47ed-8355-6af3b7612f01"); }
        }
    }
}