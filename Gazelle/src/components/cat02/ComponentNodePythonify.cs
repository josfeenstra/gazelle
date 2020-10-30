using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Dynamic;
namespace Gazelle
{
    public class ComponentNodePythonify : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComponentVarPythonify class.
        /// </summary>
        public ComponentNodePythonify()
          : base(SD.Starter + "Pythonify",
                    SD.Starter + "PY",
                    SD.CopyRight + "Turns a data node into an object python can access.",
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
            var param = new GH_Param_DataNode();
            pManager.AddParameter(param, "Node input", "N", "Connect this to an Data Node.", GH_ParamAccess.item);
            pManager[0].Optional = false;
            pManager[0].MutableNickName = false;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Dynamic Object / ExpandoObject", "Py", " ExpandoObject ", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get the node, give the object back
            var rawData = new Datatypes.GH_DataNode();
            DA.GetData(0, ref rawData);

            var pythonifiedObject = rawData.Value.GetFlatObject();            
            
            DA.SetData(0, pythonifiedObject);
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
                return Properties.Resources.pythonify; ;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7af8f158-9d94-4ad4-96dd-3d9e276f1051"); }
        }

        public void popup(string message)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, message);
        }

    }
}