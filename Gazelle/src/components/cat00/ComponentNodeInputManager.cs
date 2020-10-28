// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.NodeConversion
{
    using Grasshopper.Kernel;
    using SferedApi;
    using SferedApi.Datatypes;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class ComponentNodeInputManager : GH_Component, IGH_VariableParameterComponent
    {
        public ComponentNodeInputManager() : base(SD.Starter + "Node Expire Manager", SD.Starter + "N Expire Manager", SD.CopyRight + "Used to intelligently judge expire procedure of Nodes. \nif input changes, the 'Node Get item' components downstream will be configured in such a way that only new data will be recalculated.", SD.PluginTitle, SD.PluginCategory2)
        {
        }
        
        public bool CanInsertParameter(GH_ParameterSide side, int index) => 
            (side == null) && (index == base.get_Params().get_Input().Count);
        
        public bool CanRemoveParameter(GH_ParameterSide side, int index) => 
            ((side == null) && (index != 0)) && (index == (base.get_Params().get_Input().Count - 1));
        
        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            GH_Param_DataNode node = new GH_Param_DataNode();
            node.set_Access(0);
            node.set_NickName("N " + index.ToString());
            node.set_MutableNickName(true);
            if (side != 0)
            {
                base.get_Params().RegisterOutputParam(node, index);
            }
            else
            {
                base.get_Params().RegisterInputParam(node, index);
                this.CreateParameter(1, index);
            }
            return node;
        }
        
        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            if (side == 0)
            {
                IGH_Param param = base.get_Params().get_Output()[index];
                base.get_Params().UnregisterOutputParameter(param);
                base.get_Params().OnParametersChanged();
            }
            return true;
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("submit", "S", "Submid the data", 0, false);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("A", "A", "Explains whats happening", 0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool flag = false;
            DA.GetData<bool>(0, ref flag);
            int num = 1;
            while (true)
            {
                if (num >= base.get_Params().get_Input().Count)
                {
                    DA.SetData(0, "test");
                    return;
                }
                GH_DataNode node = new GH_DataNode();
                DA.GetData<GH_DataNode>(num, ref node);
                DataNode node2 = node.get_Value();
                DA.SetData(num, node2);
                num++;
            }
        }
        
        public void VariableParameterMaintenance()
        {
        }
        
        protected override Bitmap Icon =>
            Resources.NodeController;
        
        public override Guid ComponentGuid =>
            new Guid("1c7445dd-469c-433f-a0d1-079f37cf25ac");
    }
}
