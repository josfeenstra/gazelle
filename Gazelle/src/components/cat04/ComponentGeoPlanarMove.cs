// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.Geo
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class ComponentGeoPlanarMove : GH_Component
    {
        public ComponentGeoPlanarMove() : base(SD.Starter + "Move In Plane", SD.Starter + "Move", SD.CopyRight + "Move an object with a vector oriented to a plane", SD.PluginTitle, SD.PluginCategory4)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "G", "", 0);
            pManager.AddVectorParameter("Translation Vector", "T", "Vector move the Geometry with", 0, new Vector3d(0.0, 0.0, 10.0));
            pManager.AddPlaneParameter("Translation Plane", "P", "Plane to move object in", 0, Plane.get_WorldXY());
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Transformed Geometry", "G", "Transformed Geometry", 0);
            pManager.AddTransformParameter("Transformation", "X", "Transformation Data", 0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase base2 = null;
            Vector3d vectord = Vector3d.get_Unset();
            Plane plane = Plane.get_Unset();
            DA.GetData<GeometryBase>(0, ref base2);
            DA.GetData<Vector3d>(1, ref vectord);
            DA.GetData<Plane>(2, ref plane);
            Vector3d vectord4 = plane.ZAxis * vectord.get_Z();
            Transform transform = Transform.Translation(((plane.get_XAxis() * vectord.get_X()) + (plane.get_YAxis() * vectord.get_Y())) + vectord4);
            if (!base2.Transform(transform))
            {
                throw new Exception("transformation failed.");
            }
            DA.SetData(0, base2);
            DA.SetData(1, transform);
        }
        
        protected override Bitmap Icon =>
            Resources.MovePlanar;
        
        public override Guid ComponentGuid =>
            new Guid("a1e19b67-f72e-4d65-bac5-c427d4455d26");
    }
}
