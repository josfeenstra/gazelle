using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using GH = Grasshopper;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;



// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Gazelle
{
    public class ComponentNodeIn : GH_Component, IGH_VariableParameterComponent
    {
        private AttributesButtonVarIn myButton;
        public int ButtonState;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ComponentNodeIn()
          : base(   SD.Starter   + "Node Get Values / Data Input", 
                    SD.Starter   + "IN",
                    SD.CopyRight + "This component is used to extract data out of a singe Data Node Object. ",
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
            pManager[0].Optional = true;
            pManager[0].MutableNickName = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

        }

        // special data
        Datatypes.DataNode data = new Datatypes.DataNode();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // override name thingie 
            // collapse input node name 
            // Params.Input[0].NickName = "N";

            // input: try to get the node object
            var rawData = new Datatypes.GH_DataNode();
            DA.GetData(0, ref rawData);
            data = rawData.Value;

            // per output parameter, try to fill it with data from the input node
            int i = 0;
            foreach (var param in Params.Output)
            {
                if (data.HasKey(param.Name))
                {
                    // hoi
                    var value = data.Get(param.Name);
                    if (value is GH_Structure<IGH_Goo>)
                    {
                        DA.SetDataTree(i, (GH_Structure<IGH_Goo>)value);
                    }
                    else if (value is IEnumerable<object>)
                    {
                        DA.SetDataList(i, (IEnumerable<object>)value);
                    }
                    else
                        DA.SetData(i, value);

                }
                i++;
            }
        }

        // small helper function
        private bool ParamExists(string name)
        {
            foreach (var param in Params.Output)
                if (param.Name == name)
                {
                    param.NickName = name;
                    return true;
                }
            
            return false;
        }

        // called by the button attribute 
        public void Expand()
        {
            int i = 0;
            var SortedKeys = data.Dict.Keys.ToList();

            SortedKeys.Sort();
            foreach (var key in SortedKeys)
            {
                i++;
                if (ParamExists(key))
                {
                    continue;
                }
                IGH_Param newParam = CreateParameter(GH_ParameterSide.Output, i);
                newParam.Name = key;
                newParam.NickName = key;
                Params.RegisterOutputParam(newParam, i);
                
            }
            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        // COLLAPSE: Called by the button attribute. 
        public void Collapse()
        {
            // reset node name 
            Params.Input[0].NickName = "N";

            // per parameter
            int count = Params.Output.Count;
            int j = 0;
            for (int i = 0; i < count; i++)
            {
                // keep parameter if a recipient is attached. else, remove parameter
                var param = Params.Output[j];

                
                var recips = param.Recipients;
                if (recips.Count > 0)
                {
                    // turned on for now, can be really annoying. TODO Maybe add it as a menu option?
                    

                    // if its a loose parameter, change its name
                    foreach (var recip in recips) {
                        
                        if (recip.MutableNickName)
                        {
                            recip.NickName = param.NickName;
                        }
                        param.NickName = param.Name[0].ToString() + "...";
                    }
                    

                    // skip this parameter the next iteration
                    j += 1;
                }
                else
                {
                    // remove the parameter
                    Params.UnregisterOutputParameter(param);
                }
            }
            // must be called after fiddeling with parameters
            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        // Create Button
        public override void CreateAttributes()
        {
            var newButtonAttribute = new AttributesButtonVarIn(this, "EXPAND", "COLLAPSE");
            // newButtonAttribute.mouseDownEvent += OnMouseDownEvent;
            myButton = newButtonAttribute;
            m_attributes = newButtonAttribute;
        }

        // ---------------------------------------------------------- Parameter Voodoo
        
        #region Methods of IGH_VariableParameterComponent interface

        // only output params may be added 
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;

        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            // add normal params
            var outParam = new Param_GenericObject();
            outParam.NickName = String.Empty;
            outParam.Access = GH_ParamAccess.tree;
            outParam.MutableNickName = false;
            Params.RegisterOutputParam(outParam, index); 
            return outParam;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        // TODO sort parameters
        public void VariableParameterMaintenance()
        {
            int outputParamCount = this.Params.Output.Count;

            for (int i = 0; i < outputParamCount; i++)
            {
                var outParam = this.Params.Output[i];
                outParam.MutableNickName = false;
            }

        }

        #endregion



        //----------------------------------------------------- final part 


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.NodeIn;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("93871034-0dd7-45ab-8ff2-6a6563fa5de1"); }
        }
    }
}
