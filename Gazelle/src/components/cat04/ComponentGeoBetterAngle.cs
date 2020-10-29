// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.Geo
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Drawing;
    
    public class ComponentGeoBetterAngle : GH_Component
    {
        public ComponentGeoBetterAngle() : base(SD.Starter + "Vector Angle Improved", SD.Starter + "Angle", SD.CopyRight + "Get the angle between Vector A and B. Instead of the calculation going clockwise, it will give a negative value.", SD.PluginTitle, SD.PluginCategory4)
        {
        }
        
        private double CalculateRealAngle(Vector3d A, Vector3d B, Plane P)
        {
            double num = Vector3d.VectorAngle(A, B, P);
            if (num > 3.1415926535897931)
            {
                num -= 6.2831853071795862;
            }
            return Math.Round(num, 3);
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector A", "A", "The first vector", 0);
            pManager.AddVectorParameter("Vector B", "B", "The second vector", 0);
            pManager.AddPlaneParameter("Plane", "P", "plane", 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Radians", "R", "the angle in radians", 0);
            pManager.AddNumberParameter("Degree", "D", "the angle in degrees", 0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Vector3d a = Vector3d.Unset;
            Vector3d b = Vector3d.Unset;
            Plane p = Plane.Unset;
            DA.GetData<Vector3d>(0, ref a);
            DA.GetData<Vector3d>(1, ref b);
            DA.GetData<Plane>(2, ref p);
            double num = this.CalculateRealAngle(a, b, p);
            double num2 = Math.Round((double) ((num * 180.0) / 3.1415926535897931), 3);
            DA.SetData(0, num);
            DA.SetData(1, num2);
        }
        
        protected override Bitmap Icon =>
            Resources.BetterAngle;
        
        public override Guid ComponentGuid =>
            new Guid("6cfc2f0b-d2d1-4e9c-9943-264cac9fb939");
    }
}
