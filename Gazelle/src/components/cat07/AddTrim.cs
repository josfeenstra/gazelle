// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class AddTrim : GH_Component
    {
        public AddTrim() : base(
            SD.Starter + "AddTrim", 
            "Trim", 
            SD.CopyRight ?? "", 
            SD.PluginTitle,
            SD.PluginCategory7)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", (GH_ParamAccess)0);
            pManager.AddIntegerParameter("Edge index", "Ei", "Edge Index", (GH_ParamAccess)0, 0);
            pManager.AddIntegerParameter("Loop index", "Li", "Loop Index", (GH_ParamAccess)0, 0);
            pManager.AddBooleanParameter("Reversed", "R", "Is trim reversed edge", 0, false);
            pManager.AddBooleanParameter("Flip", "F", "flip the curve", 0, false);
            pManager.AddIntegerParameter("Isostatus", "Iso", "Iso Status :      { None = 0, X = 1,  Y = 2, West = 3,  South = 4, East = 5, North = 6 }", (GH_ParamAccess)0, 0);
            pManager.AddIntegerParameter("TrimType", "T", "Trim Type :      { Unknown = 0, Boundary = 1, Mated = 2, Seam = 3, Singular = 4, CurveOnSurface = 5, PointOnSurface = 6, Slit = 7 }", (GH_ParamAccess)0, 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "New Brep with the addition", (GH_ParamAccess)0);
            pManager.AddIntegerParameter("Trim Index", "Ti", "Trim index", (GH_ParamAccess)0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            int edge = -1;
            int loop = -1;
            bool isTrimReversedEdge = false;
            bool flipTrim = false;
            int num3 = -1;
            int num4 = -1;
            DA.GetData<Brep>(0, ref brep);
            DA.GetData<int>(1, ref edge);
            DA.GetData<int>(2, ref loop);
            DA.GetData<bool>(3, ref isTrimReversedEdge);
            DA.GetData<bool>(4, ref flipTrim);
            DA.GetData<int>(5, ref num3);
            DA.GetData<int>(6, ref num4);
            if (((brep == null) || ((edge == -1) || ((loop == -1) || ((num3 < 0) || ((num3 > 6) || (num4 < 0)))))) || (num4 > 7))
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)20, "input bad");
            }
            else
            {
                DA.SetData(1, brep.AddTrimDepricated(edge, loop, isTrimReversedEdge, flipTrim, (IsoStatus) num3, (BrepTrimType) num4));
                DA.SetData(0, brep);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("db83006c-6cba-48ce-a6e9-17eab169b2f9");
    }
}
