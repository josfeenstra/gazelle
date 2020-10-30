// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace Gazelle.Components.Surface
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using Gazelle;
    using Gazelle.Properties;
    using System;
    using System.Drawing;
    
    public class PatchHexagon : GH_Component
    {
        public PatchHexagon() : base(SD.Starter + "Hexagon Surface", SD.Starter + "Hex", SD.CopyRight + "Build a Hexagon shape surface", SD.PluginTitle, SD.PluginCategory9)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            string[] textArray1 = new string[] { "A", "B", "C", "1", "2", "3" };
            foreach (string str in textArray1)
            {
                pManager.AddCurveParameter("Curve " + str, str, "", (GH_ParamAccess)0);
            }
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", (GH_ParamAccess)1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve a = null;
            Curve b = null;
            Curve c = null;
            Curve d = null;
            Curve e = null;
            Curve f = null;
            DA.GetData<Curve>(0, ref a);
            DA.GetData<Curve>(1, ref b);
            DA.GetData<Curve>(2, ref c);
            DA.GetData<Curve>(3, ref d);
            DA.GetData<Curve>(4, ref e);
            DA.GetData<Curve>(5, ref f);
            Brep[] brepArray = SurfaceFunctions.PatchHexagon(a, b, c, d, e, f, SurfaceFunctions.PatchHexagonStyle.CenterTriangle);
            DA.SetDataList(0, brepArray);
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("cc347769-41f3-4a3e-9de6-76c38959ee21");
    }
}
