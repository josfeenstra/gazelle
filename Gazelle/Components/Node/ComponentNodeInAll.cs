using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SferedApi
{
    public class ComponentNodeInAll : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComponentNodeGetAll class.
        /// </summary>
        public ComponentNodeInAll()
             : base(SD.Starter   + "Node Get All Data", 
                    SD.Starter   + "All",
                    SD.CopyRight + "Get all the data out of Node, and put it into a list ",
                    SD.PluginTitle, 
                    SD.PluginCategory1)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            var param = new GH_Param_DataNode();
            pManager.AddParameter(param, "Node input", "N", "Connect this to an Data Node.", GH_ParamAccess.item);
            pManager[0].Optional = false;
            pManager[0].MutableNickName = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data Output", "All", "contains all elements on this level as a list", GH_ParamAccess.tree);
            pManager[0].Optional = false;
            pManager[0].MutableNickName = true;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input: try to get the node object
            var rawData = new Datatypes.GH_DataNode();
            DA.GetData(0, ref rawData);

            // prepare output
            var tree = new DataTree<object>();
            var list = new List<object>();

            // per output parameter, try to fill it with data from the input node
            var branch = 0;
            foreach (var item in rawData.Value.Dict)
            {
                var value = item.Value;
                if (value is GH_Structure<IGH_Goo>)
                {
                    // add flattened tree as treebranch 
                    var aTree = (GH_Structure<IGH_Goo>)value;
                    foreach (var aItem in aTree.FlattenData())
                    {
                        tree.Add(aItem, new GH_Path(branch));
                    }
                }
                else if (value is DataTree<object>)
                {
                    // add flattened tree as treebranch 
                    var aTree = (GH_Structure<IGH_Goo>)value;
                    foreach (var aItem in aTree.FlattenData())
                    {
                        tree.Add(aItem, new GH_Path(branch));
                    }
                }
                else if (value is IEnumerable<object>)
                {
                    //  add list as treebranch 
                    var aList = (IEnumerable<object>)value;
                    foreach(var aItem in aList)
                    {
                        tree.Add(aItem, new GH_Path(branch));
                    }
                }
                else
                {
                    tree.Add(value, new GH_Path(branch));
                }
                branch += 1;

            }
            // output 
            DA.SetDataTree(0, tree);
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
                return Properties.Resources.NodeGetAll;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("da03477d-965a-4fc8-bb6d-feff62f926dd"); }
        }
    }
}