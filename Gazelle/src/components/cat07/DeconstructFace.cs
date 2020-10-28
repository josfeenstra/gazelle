// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    
    public class DeconstructFace : GH_Component
    {
        public DeconstructFace() : base(SD.Starter + "Deconstruct Face", "DeFace", SD.CopyRight + "Deconstruct A BrepFace of a Brep", SD.PluginTitle, SD.PluginCategory7)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep", 0);
            pManager.AddIntegerParameter("Int", "Fi", "index of face", 0, 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "the brepface as a new brep", 0);
            pManager.AddSurfaceParameter("Surface", "S", "the surface it is based on", 0);
            pManager.AddIntegerParameter("Loops", "Li", "LoopIndices", 1);
            pManager.AddBooleanParameter("orientation", "O", "orientation. true means reversed", 0);
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
            else if ((num < 0) || (num >= brep.get_Faces().get_Count()))
            {
                this.AddRuntimeMessage(10, "out of range");
            }
            else
            {
                BrepFace face = brep.get_Faces().get_Item(num);
                DA.SetData(0, face.ToBrep());
                DA.SetData(1, face.UnderlyingSurface());
                DA.SetDataList(2, from item in face.Loops select item.get_LoopIndex());
                DA.SetData(3, face.get_OrientationIsReversed());
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("448adbf1-947f-4ef6-a990-42656da03db2");
        
        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DeconstructFace.<>c <>9 = new DeconstructFace.<>c();
            public static Func<BrepLoop, int> <>9__3_0;
            
            internal int <SolveInstance>b__3_0(BrepLoop item) => 
                item.get_LoopIndex();
        }
    }
}
