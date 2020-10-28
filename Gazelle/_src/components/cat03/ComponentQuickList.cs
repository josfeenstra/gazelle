using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SferedApi
{
    public class ComponentQuickList : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentQuickList()
          : base(   SD.Starter + "Create List",
                    "List",
                    SD.CopyRight + "Make a list from items, with specific order",
                    SD.PluginTitle,
                    SD.PluginCategory3)
        {
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
            pManager.AddGenericParameter("data input", "0", "Add Data Here", GH_ParamAccess.item);
            pManager.AddGenericParameter("data input", "1", "Add Data Here", GH_ParamAccess.item);
            pManager.AddGenericParameter("data input", "2", "Add Data Here", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("List", "L", "the resulting list", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // dynamic input, build a dictionary out of the found data 
            var list = new List<object>();
            for (int i = 0; i < Params.Input.Count; i++)
            {
                // get the proper value
                object item = null;
                DA.GetData(i, ref item);
                list.Add(item);
            }

            // output
            DA.SetDataList(0, list);

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
            inParam.Access = GH_ParamAccess.item;
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
                return Properties.Resources.CreateList;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d218067f-a15a-441f-9330-e71558c92131"); }
        }
    }
}