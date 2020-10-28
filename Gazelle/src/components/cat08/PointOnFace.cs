// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.BrepAdvanced
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class PointOnFace : GH_Component
    {
        public PointOnFace() : base(SD.Starter + "Point On Face", SD.Starter + "P on F", SD.CopyRight + "Get the index of the face this point is standing on. -1 if unsuccesful", SD.PluginTitle, SD.PluginCategory8)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", 0);
            pManager.AddPointParameter("Point", "P", "Point", 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Face Index", "Fi", "index of the face", 0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            Point3d point = Point3d.get_Unset();
            DA.GetData<Brep>(0, ref brep);
            DA.GetData<Point3d>(1, ref point);
            if ((brep == null) || (point == Point3d.get_Unset()))
            {
                this.AddRuntimeMessage(20, "input bad");
            }
            else
            {
                int num = brep.PointOnFace(point);
                DA.SetData(0, num);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("8e28da11-9b99-45e6-92a2-3a6d243732a9");
    }
}
