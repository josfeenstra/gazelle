// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class DetectInsertions : GH_Component
    {
        public DetectInsertions() : base(SD.Starter + "Detect Insertions", SD.Starter + "Detect", SD.CopyRight + "Partition 'inner' and 'outer' faces of a brep after splitting", SD.PluginTitle, SD.PluginCategory8)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Inner Face Indices", "In", "", 1);
            pManager.AddIntegerParameter("Outer Face Indices", "Out", "", 1);
            pManager.AddTextParameter("Log", "T", "Log", 1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            DA.GetData<Brep>(0, ref brep);
            if (brep == null)
            {
                this.AddRuntimeMessage(20, "input bad");
            }
            else
            {
                int[] numArray;
                int[] numArray2;
                BrepFunctions.DetectInsertions(brep, out numArray, out numArray2);
                DA.SetDataList(0, numArray);
                DA.SetDataList(1, numArray2);
                DA.SetDataList(2, BrepFunctions.Log);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("a3c4a067-44e9-434e-8cdc-4d10a9228c58");
    }
}