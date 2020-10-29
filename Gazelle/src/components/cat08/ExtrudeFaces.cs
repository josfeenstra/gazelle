// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    
    public class ExtrudeFaces : GH_Component
    {
        public ExtrudeFaces() : base(SD.Starter + "Extrude Faces", SD.Starter + "Extr", SD.CopyRight + "Extrude Faces of a brep", SD.PluginTitle, SD.PluginCategory8)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", (GH_ParamAccess)0);
            pManager.AddIntegerParameter("Face index", "Fi", "face Index", (GH_ParamAccess)1, 0);
            pManager.AddVectorParameter("Vector", "V", "direction and distance of extrution", (GH_ParamAccess)0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Extruded brep", (GH_ParamAccess)0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            List<int> faces = new List<int>();
            Vector3d direction = Vector3d.Unset;
            DA.GetData<Brep>(0, ref brep);
            DA.GetDataList<int>(1, faces);
            DA.GetData<Vector3d>(2, ref direction);
            if ((brep == null) || (direction == Vector3d.Unset))
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)20, "input bad");
            }
            else
            {
                brep = brep.ExtrudeFaces(faces, direction);
                DA.SetData(0, brep);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("5305705d-75fc-4c90-8c60-b46f210e81ba");
    }
}
