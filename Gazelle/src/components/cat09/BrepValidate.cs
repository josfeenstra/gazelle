// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.BrepBasics
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class BrepValidate : GH_Component
    {
        public BrepValidate() : base(SD.Starter + "Brep Validate", SD.Starter + "Validate", SD.CopyRight + "Check if a brep is valid, open, closed, etc.", SD.PluginTitle, SD.PluginCategory9)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("V", "V", "Boolean. if true, brep is valid", 0);
            pManager.AddTextParameter("text", "T", "text explaining whats wrong", 0);
            pManager.AddBooleanParameter("flags", "Fb", "text explaining is valid tolerances and flags", 0);
            pManager.AddTextParameter("flags", "F", "text explaining is valid tolerances and flags", 0);
            pManager.AddBooleanParameter("S", "S", "Is Brep Solid", 0);
            pManager.AddCurveParameter("N", "N", "Naked edges", 1);
            pManager.AddBooleanParameter("O", "O", "Has Correct Face Orientations", 0);
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
                string str;
                string str2;
                Curve[] curveArray = brep.DuplicateNakedEdgeCurves(true, true);
                bool flag = brep.get_IsSolid() || (curveArray.Length != 0);
                DA.SetData(0, brep.IsValidWithLog(ref str));
                DA.SetData(1, str);
                DA.SetData(2, brep.IsValidTolerancesAndFlags(ref str2));
                DA.SetData(3, str2);
                DA.SetData(4, brep.get_IsSolid());
                DA.SetDataList(5, curveArray);
                DA.SetData(6, flag);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("53b749cb-2459-441b-8059-365d735ff357");
    }
}
