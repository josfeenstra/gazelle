// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    
    public class DeconstructLoop : GH_Component
    {
        public DeconstructLoop() : base(SD.Starter + "Deconstruct Loop", "DeLoop", SD.CopyRight + "Deconstruct a BrepLoop of a Brep", SD.PluginTitle, SD.PluginCategory7)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep", 0);
            pManager.AddIntegerParameter("Int", "Li", "index of loop", 0, 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("TrimIndices", "Ti", "Trim indices", 1);
            pManager.AddTextParameter("LoopType", "T", "LoopType", 1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            int num = -1;
            DA.GetData<Brep>(0, ref brep);
            DA.GetData<int>(1, ref num);
            if (brep == null)
            {
                this.AddRuntimeMessage(20, "Input bad");
            }
            else if ((num < 0) || (num >= brep.Loops.get_Count()))
            {
                this.AddRuntimeMessage(10, "out of range");
            }
            else
            {
                BrepLoop loop = brep.Loops.get_Item(num);
                DA.SetDataList(0, from item in loop.get_Trims() select item.get_TrimIndex());
                DA.SetData(1, loop.get_LoopType().ToString());
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("0501bfa1-9040-42aa-8aee-0d235c5bfe8c");
        
        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DeconstructLoop.<>c <>9 = new DeconstructLoop.<>c();
            public static Func<BrepTrim, int> <>9__3_0;
            
            internal int <SolveInstance>b__3_0(BrepTrim item) => 
                item.get_TrimIndex();
        }
    }
}
