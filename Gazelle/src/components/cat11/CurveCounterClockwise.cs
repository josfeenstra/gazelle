// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.CurveAdvanced
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    
    public class CurveCounterClockwise : GH_Component
    {
        public CurveCounterClockwise() : base(SD.Starter + "Counter Clockwise", SD.Starter + "CCW", SD.CopyRight + "if the curve is not counter clockwise, make it so", SD.PluginTitle, SD.PluginCategory11)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve", 0);
            pManager.AddPlaneParameter("Plane", "P", "Plane", 0, Plane.WorldXY);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve", 0);
            pManager.AddBooleanParameter("Bool", "B", "True if this curve has been flipped.", 0);
        }
        
        private Curve RunScript(Curve curve, Plane plane, out bool hasFlipped)
        {
            hasFlipped = false;
            if (!curve.get_IsClosed())
            {
                throw new Exception("A curve is not closed...");
            }
            if (curve.ClosedCurveOrientation(plane) != 1)
            {
                hasFlipped = true;
                curve.Reverse();
            }
            return curve;
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            bool flag;
            Plane plane = Plane.get_Unset();
            DA.GetData<Curve>(0, ref curve);
            DA.GetData<Plane>(1, ref plane);
            curve = this.RunScript(curve, plane, out flag);
            DA.SetData(0, curve);
            DA.SetData(1, flag);
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("b31d5904-c23f-4ef1-babd-8f38b8b6ea89");
    }
}
