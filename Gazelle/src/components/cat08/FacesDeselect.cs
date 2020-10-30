// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace Gazelle
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using Gazelle.Properties;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    
    public class FacesDeselect : GH_Component
    {
        public FacesDeselect() : base(SD.Starter + "Deselect Faces", "Deselect", SD.CopyRight + "Choose faces by integers, and remove them from the brep", SD.PluginTitle, SD.PluginCategory8)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", (GH_ParamAccess)0);
            List<int> list1 = new List<int>();
            list1.Add(0);
            pManager.AddIntegerParameter("Face indices", "Fi", "Edge Index", (GH_ParamAccess)1, list1);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "Ex", "New Brep excluding these faces", (GH_ParamAccess)0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            List<int> faces = new List<int>();
            DA.GetData<Brep>(0, ref brep);
            DA.GetDataList<int>(1, faces);
            if (brep == null)
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)20, "input bad");
            }
            else
            {
                DA.SetData(0, brep.DeselectFaces(faces));
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("8468bc4f-163d-4d45-b79b-857f7b86b86d");
    }
}
