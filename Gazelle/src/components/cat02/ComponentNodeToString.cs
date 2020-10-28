using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SferedApi.Datatypes;

namespace SferedApi
{
    public class ComponentNodeToString : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentNodeToString()
          : base(SD.Starter + "Node get json string",
                    SD.Starter + "string",
                    SD.CopyRight + "get json string from node",
                    SD.PluginTitle,
                    SD.PluginCategory2)
        {
            IconDisplayMode = GH_IconDisplayMode.icon;
            MutableNickName = false;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        /// 
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("node","N","node",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("string", "S", "json string", GH_ParamAccess.item);
        }

   


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input 
            var inNode = new GH_DataNode();
            DA.GetData(0, ref inNode);

            // process 
            var outString = "";
            outString = inNode.Value.GetJson();

            // output
            DA.SetData(0, outString);
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
                return Properties.Resources.ToJson;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ec10d7a8-924f-4247-bbfb-a2146bfa804d"); }
        }

        // see this as a collection of all things possible within these objects 

        public void Popup(string sometext)
        {
            MessageBox.Show(sometext);
        }







    }
}