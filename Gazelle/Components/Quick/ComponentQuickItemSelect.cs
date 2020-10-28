using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;

namespace SferedApi.Components.Misc
{
    public class ComponentQuickItemSelect : GH_Component, IGH_VariableParameterComponent
    {
        const string MINIndicator = "min";
        const string MAXIndicator = "max";
        const int minimumIndex = 0;
        int maximumIndex;

        /// <summary>
        /// Initializes a new instance of the ComponentQuickItemSelect class.
        /// </summary>
        public ComponentQuickItemSelect()
          : base(
                    SD.Starter + "Get List Item",
                    "GI",
                    SD.CopyRight + "Get an item of a list by changing this nickname to the item ID.",
                    SD.PluginTitle,
                    SD.PluginCategory3)
        {
            this.Params.ParameterChanged += new GH_ComponentParamServer.ParameterChangedEventHandler(OnParameterChanged);
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("List", "L", "Connect this to a list.", GH_ParamAccess.list);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("item", "0", "The n's item of the list", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get the list, test if we have someting
            var inList = new List<object>();
            DA.GetDataList(0, inList);
            if (inList.Count == 0)
                return;
            maximumIndex = inList.Count - 1;
            int i = 0;
            foreach(var output in Params.Output)
            {
                try
                {
                    // try to build an int out of the nickname, and use it as
                    string indexstring = output.NickName;
                    int index;
                    var response = int.TryParse(indexstring, out index);
                    if (response)
                    {
                        // one item
                        var item = inList[index];
                        DA.SetData(i, item);
                    }
                    else
                    {
                        // select multiple indexes.
                        var outList = new List<object>();
                        var parts = indexstring.Replace(" ", "").Split(',');
                        foreach (string part in parts)
                        {
                            var results = new List<int>();
                            var succes = TryExtractRange(part, out results);
                            if (succes)
                            {
                                foreach (var result in results)
                                {
                                    var item = inList[result];
                                    outList.Add(item);
                                }
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        DA.SetDataList(i, outList);
                    }
                    i++;
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Couldnt extract valid index out of Nickname and List." + e.ToString());
                }
            }
        }

        public bool CheckTextForValidIndex(string text, out int index)
        {
            if (text == MINIndicator)
            {
                index = minimumIndex;
                return true;
            }
            if (text == MAXIndicator)
            {
                index = maximumIndex;
                return true;
            }
            var suc = int.TryParse(text, out index);
            return suc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="results"></param>
        /// <returns>true if a value is found</returns>
        public bool TryExtractRange(string part, out List<int> results)
        {
            // "part" must have no spaces, and is split at comma's. 1 - 2 -> 1-2 
            results = new List<int>();

            // extract single number 
            int index;
            var succes = CheckTextForValidIndex(part, out index);
            if (succes)
            {
                results.Add(index);
                return true;
            }

            // try extract range of numbers 
            if (part.Contains("-"))
            {
                var subParts = part.Split('-');
                if (subParts.Length != 2) return false; // quit with statements like: 1-2-3 or --1
                int lowIndex;
                var succes1 = CheckTextForValidIndex(subParts[0], out lowIndex);
                int highIndex;
                var succes2 = CheckTextForValidIndex(subParts[1], out highIndex);
                if (!(succes1 && succes2 && lowIndex < highIndex)) return false; // quit if the low and high index of the range do not make sense

                // indexes are correct, extract range
                for (int i = lowIndex; i <= highIndex; i++) // up to and including highindex
                {
                    results.Add(i);
                }

                // success
                return true;
            }

            // string found at "part" cannot be understood
            return false;
        }


        protected virtual void OnParameterChanged(object sender, GH_ParamServerEventArgs e)
        {
            // only change if an output parameter has changed (name change)
            if (e.ParameterSide == GH_ParameterSide.Output)
                ExpireSolution(true);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
                return false;
            return true;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
                return false;
            return true;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            // add normal params
            var param = new Param_GenericObject();
            param.Access = GH_ParamAccess.item;
            param.MutableNickName = true;

            // determine name 
            if (index > 0)
            {
                try
                {
                    int prevID = int.Parse(Params.Output[index - 1].NickName);
                    param.NickName = (prevID + 1).ToString();
                }
                catch
                {
                    param.NickName = "0";
                }
            }
            else
                param.NickName = index.ToString();

            // register 
            Params.RegisterOutputParam(param, index);
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            // nothing has to happen here
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
                return Properties.Resources.pick_item;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cc46362c-241b-4c75-bb1a-cfede05a6c77"); }
        }


    }
}