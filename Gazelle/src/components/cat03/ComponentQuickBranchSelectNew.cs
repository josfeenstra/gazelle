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
    public class ComponentQuickBranchSelect : GH_Component, IGH_VariableParameterComponent
    {
        const string MINIndicator = "min";
        const string MAXIndicator = "max";
        const int minimumIndex = 0;
        int maximumIndex;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentQuickBranchSelect()
          : base(   SD.Starter + "Get Tree Branch", 
                    "GB",
                    SD.CopyRight + "Get a branch of a tree by changing this nickname to the branch id.",
                    SD.PluginTitle, 
                    SD.PluginCategory3)
        {
            Params.ParameterChanged += new GH_ComponentParamServer.ParameterChangedEventHandler(OnParameterChanged);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Tree", "T", "Connect to a tree.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Branch", "0", "The n's branch.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var tree = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out tree);
            maximumIndex = tree.Branches.Count-1;
            for (int i = 0; i <= Params.Output.Count - 1; i++)
            {
                try
                {
                    // try to build an int out of the nickname, and use it as
                    var output = Params.Output[i];
                    string indexstring = output.NickName;
                    var response = CheckTextForValidIndex(indexstring, out int index);
                    if (response)
                    {
                        // one item
                        var branch = tree.Branches[index];
                        DA.SetDataList(i, branch);
                    }
                    else
                    {
                        // select multiple indexes.
                        var outTree = new DataTree<object>();
                        var parts = indexstring.Replace(" ", "").Split(',');
                        foreach (string part in parts)
                        {
                            var results = new List<int>();
                            var succes = TryExtractRange(part, out results);
                            if (succes)
                            {
                                foreach(var result in results)
                                {
                                    var branch = tree.Branches[result];
                                    outTree.AddRange(branch, new GH_Path(result));
                                }  
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        DA.SetDataTree(i, outTree);
                    }
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Couldnt extract valid index out of Nickname and Tree. " + e);
                }
            }
        }

        private bool CheckTextForValidIndex(string text, out int index)
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
        private bool TryExtractRange(string part, out List<int> results)
        {
            results = new List<int>();
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
                return Properties.Resources.pick_branch;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("920e7646-efc9-4ba8-b17e-1ea75ff5f43c"); }
        }
    }
}