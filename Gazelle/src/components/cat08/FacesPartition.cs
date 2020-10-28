// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    
    public class FacesPartition : GH_Component
    {
        public FacesPartition() : base(SD.Starter + "Partition Faces", "Partition", SD.CopyRight + "Choose faces by integers, and build a new brep from them. also returns te brep without these faces ", SD.PluginTitle, SD.PluginCategory8)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", 0);
            List<int> list1 = new List<int>();
            list1.Add(0);
            pManager.AddIntegerParameter("Face indices", "Fi", "Edge Index", 1, list1);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "In", "New Brep including these faces", 0);
            pManager.AddBrepParameter("Brep", "Ex", "New Brep excluding these faces", 0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            List<int> faces = new List<int>();
            DA.GetData<Brep>(0, ref brep);
            DA.GetDataList<int>(1, faces);
            if (brep == null)
            {
                this.AddRuntimeMessage(20, "input bad");
            }
            else
            {
                Brep[] brepArray = brep.PartitionFaces(faces);
                DA.SetData(0, brepArray[0]);
                DA.SetData(1, brepArray[1]);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("0e98c9f7-da44-4bde-8138-f531a7d364b0");
    }
}