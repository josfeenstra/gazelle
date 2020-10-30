using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Gazelle
{
    public class ComponentQuickTree : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentQuickTree()
          : base(SD.Starter + "Create Tree",
                    "Tree",
                    SD.CopyRight + "Make a tree from various entries. ",
                    SD.PluginTitle,
                    SD.PluginCategory3)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // add a couple of samples, makes life easier
            pManager.AddGenericParameter("data input", "0", "Add Data Here", GH_ParamAccess.tree);
            pManager.AddGenericParameter("data input", "1", "Add Data Here", GH_ParamAccess.tree);
            pManager.AddGenericParameter("data input", "2", "Add Data Here", GH_ParamAccess.tree);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Tree", "T", "the resulting list", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // prepare output
            var outTree = new DataTree<object>();

            // per input parameter, 
            var branch = 0;
            for (int iParam = 0; iParam < Params.Input.Count; iParam++)
            {
                // get the input variable
                var inTree = new GH_Structure<IGH_Goo>();
                DA.GetDataTree(iParam, out inTree);
                
                // per branch of the input tree
                foreach (var aBranch in inTree.Branches)
                {
                    // per item of the input branch
                    foreach (var anItem in aBranch)
                    {
                        outTree.Add(anItem, new GH_Path(branch));
                    }
                    branch += 1;
                }
            }
            // output 
            DA.SetDataTree(0, outTree);

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
            inParam.NickName = index.ToString();
            inParam.MutableNickName = true;
            inParam.Optional = true;
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
                    string name = i.ToString();
                    input.NickName = name;
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
                return Properties.Resources.CreateTree;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e89afdd9-8345-4826-8fb5-5b9911c0d482"); }
        }
    }
}