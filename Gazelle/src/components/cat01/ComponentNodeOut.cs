using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SferedApi.Components.Node
{
    public class ComponentNodeOut : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the ComponentNodeOut class.
        /// </summary>
        public ComponentNodeOut()
          : base(   SD.Starter + "Node Set Values",
                    SD.Starter + "OUT",
                    SD.CopyRight + "This component is used to create a new Data Node Object out of data. ",
                    SD.PluginTitle,
                    SD.PluginCategory1)
        {
            Params.ParameterSourcesChanged += ParamChanged;
            Params.ParameterNickNameChanged += ParamChanged;
        }

        private void ParamChanged(object sender, GH_ParamServerEventArgs e)
        {
            VariableParameterMaintenance();
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // add a couple of samples, makes life easier
            pManager.AddGenericParameter("data input", "(0)", "Add Data Here", GH_ParamAccess.tree);
            pManager.AddGenericParameter("data input", "(1)", "Add Data Here", GH_ParamAccess.tree);
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
            // dynamic input, build a dictionary out of the found data 
            var dict = new Dictionary<string, object>();
            for(int i = 0; i < Params.Input.Count; i++)
            {
                // key must be set to 1 single source nickname
                if (Params.Input[i].Sources.Count != 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "input " + i.ToString() + " needs to have 1 source.");
                    continue;
                }

                // get the proper key at sources nickname
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
                        dict.Add(key, item);
                    }
                    else
                    {
                        // the list is a proper list 
                        dict.Add(key, list);
                    }
                }
                else
                {
                    // the tree is a proper tree 
                    dict.Add(key, tree);
                }
            }

            // output
            var outputparam = new Datatypes.GH_DataNode(dict);
            DA.SetData(0, outputparam);

            // change name thingie
            VariableParameterMaintenance();
        }

        #region Methods of IGH_VariableParameterComponent interface

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
                return true;
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
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
            for (int i = 0; i < inputs; i++)
            {
                var input = this.Params.Input[i];
                input.MutableNickName = false;
                if (input.Sources.Count == 1)
                {
                    // changed nickname into name. Sometimes things get too bulky
                    string name = input.Sources[0].NickName;
                    input.Name = name;
                    input.NickName = name[0].ToString() + "...";
                }
            }
            Params.OnParametersChanged();
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
                return Properties.Resources.NodeOut; ;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a8e7478e-1310-49d4-993c-f57ac1449b40"); }
        }
    }
}