// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace Gazelle
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using Gazelle.Properties;
    using System;
    using System.Drawing;
    
    public class AddVertex : GH_Component
    {
        public AddVertex() : base(SD.Starter + "AddVertex", "Vertex", SD.CopyRight ?? "", SD.PluginTitle, SD.PluginCategory7)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", (GH_ParamAccess)0);
            pManager.AddPointParameter("Point", "P", "'point geo", (GH_ParamAccess)0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "New Brep with the addition", (GH_ParamAccess)0);
            pManager.AddIntegerParameter("Vertex Index", "Vi", "Vertex index", (GH_ParamAccess)0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            Point3d point = Point3d.Unset;
            DA.GetData<Brep>(0, ref brep);
            DA.GetData<Point3d>(1, ref point);
            if ((brep == null) || !point.IsValid)
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)20, "input bad");
            }
            else
            {
                DA.SetData(1, brep.AddVertex(point));
                DA.SetData(0, brep);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("2f6bbc22-6762-4401-ad96-3b029c1ffa42");
    }
}
