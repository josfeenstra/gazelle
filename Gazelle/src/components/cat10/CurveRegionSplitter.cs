// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.Experiments
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class CurveRegionSplitter : GH_Component
    {
        public CurveRegionSplitter() : base(SD.Starter + "split curve region", SD.Starter + "divide", SD.CopyRight, SD.PluginTitle, SD.PluginCategory10)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", (GH_ParamAccess)0);
            pManager.AddCurveParameter("Curve", "Cb", "boundary curves", (GH_ParamAccess)1);
            pManager.AddCurveParameter("Curve", "Cs", "splitter curves", (GH_ParamAccess)1);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve pieces", "C", "curve divided among the faces", (GH_ParamAccess)2);
            pManager.AddIntegerParameter("Face Indices", "Ei", "Indices of faces this curve interacts with", (GH_ParamAccess)1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            Curve curve = null;
            DA.GetData<Brep>(0, ref brep);
            DA.GetData<Curve>(1, ref curve);
            if (brep == null)
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)20, "input bad");
            }
            else if (((curve == null) || !curve.IsValid) || curve.IsClosed)
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)20, "curve is invalid, missing, or not closed.");
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("08e1aa16-64bf-4c19-806f-27b84a9485db");
    }
}
