// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.BrepUtilities
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class BrepReorient : GH_Component
    {
        public BrepReorient() : base(SD.Starter + "Reorient Brep", SD.Starter + "Reorient", SD.CopyRight + "Reorient the Faces of a brep to the same direction.", SD.PluginTitle, SD.PluginCategory9)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep", 0);
            pManager.AddIntegerParameter("Face Index", "Fi", "Face Index of starting face", 0, 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", 0);
            pManager.AddTextParameter("Print Log", "L", "Log", 1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep original = null;
            int faceStart = 0;
            DA.GetData<Brep>(0, ref original);
            DA.GetData<int>(1, ref faceStart);
            original = BrepFunctions.ReorientFaces(original, faceStart);
            DA.SetData(0, original);
            DA.SetDataList(1, BrepFunctions.Log);
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("94ff1c13-0647-4050-baeb-c163656307a5");
    }
}
