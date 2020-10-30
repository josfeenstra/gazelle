using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Gazelle.Datatypes;

namespace Gazelle.Components.Node
{
    public class ComponentNodeAdd : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the ComponentNodeMerge class.
        /// </summary>
        public ComponentNodeAdd()
          : base(   SD.Starter + "Node Output Adder",
                    SD.Starter + "Add",
                    SD.CopyRight + "This component is used to add data to an existing Data Node Object. " +
                                   "new object with known key will overwrite old objects.",
                    SD.PluginTitle,
                    SD.PluginCategory1)
        {
            Params.ParameterSourcesChanged += ParamSourcesChanged;
        }

        private void ParamSourcesChanged(object sender, GH_ParamServerEventArgs e)
        {
            VariableParameterMaintenance();
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

            // add a couple of samples, makes life easier
            pManager.AddGenericParameter("data input", "(0)", "add Data Here", GH_ParamAccess.tree);
            pManager.AddGenericParameter("data input", "(1)", "add Data Here", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "Data Node", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[0].MutableNickName = false;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input: try to get the node object
            var GH_Node = new GH_DataNode();
            DA.GetData(0, ref GH_Node);
            
            // copy the node 
            GH_Node = new GH_DataNode(GH_Node);
            DataNode data = GH_Node.Value;

            // dynamic input, build a dictionary out of the found data. START COUNTING AT 1
            for (int i = 1; i < Params.Input.Count; i++)
            {
                // key must be set to 1 single source nickname
                if (Params.Input[i].Sources.Count != 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "input " + i.ToString() + " needs to have 1 source.");
                    continue;
                }

                // get the proper key
                string key = Params.Input[i].Sources[0].NickName;

                // get the proper value
                var tree = new GH_Structure<IGH_Goo>();
                DA.GetDataTree(i, out tree);

                // if the value is only a list or a single item, save it appropriately
                if (tree.Branches.Count == 0)
                    continue;               // dit kan buggy zijn watch out 
                if (tree.Branches.Count == 1)
                {
                    // the tree is actually a list 
                    var list = new List<IGH_Goo>();
                    list = tree.Branches[0];
                    if (list.Count == 0)
                        continue;           // dit kan buggy zijn watch out
                    if (list.Count == 1)
                    {
                        // the list is actually an item 
                        var item = list[0];
                        data.Add(key, item);
                    }
                    else
                    {
                        // the list is a proper list 
                        data.Add(key, list);
                    }
                }
                else
                {
                    // the tree is a proper tree 
                    data.Add(key, tree);
                }
            }

            // output
            DA.SetData(0, GH_Node);

            // change variable names 
            VariableParameterMaintenance();
        }

        #region Methods of IGH_VariableParameterComponent interface

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index != 0)
                return true;
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index != 0)
                return true;
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            // add normal params
            var inParam = new Param_GenericObject();
            inParam.Access = GH_ParamAccess.tree;
            inParam.NickName = String.Empty;
            inParam.MutableNickName = true;
            Params.RegisterOutputParam(inParam, index);
            return inParam;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            int inputs = this.Params.Input.Count;
            for (int i = 1; i < inputs; i++)
            {
                var input = this.Params.Input[i];
                input.MutableNickName = false;
                if (input.Sources.Count == 1)
                {
                    string name = input.Sources[0].NickName;
                    input.NickName = name;
                    input.MutableNickName = false;
                }
            }

            // this is guaranteed
            Params.Output[0].NickName = Params.Input[0].NickName;

        }

        #endregion

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.NodeMerge;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e8a60b38-d1e9-41e7-989e-43c7553a55ab"); }
        }
    }
}