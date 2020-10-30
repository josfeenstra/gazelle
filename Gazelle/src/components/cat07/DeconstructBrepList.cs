// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Gazelle.Properties;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Gazelle
{
    public class DeconstructBrepList : GH_Component
    {
        public DeconstructBrepList() : base(SD.Starter + "Deconstruct Brep Lists", "DeBrepLists", SD.CopyRight + "Expose the 5 lists of major topology elements within a brep. Sending the true elements into the output only works with custom parameters.We chose to use a list of indices instead. Data within can be accessed using the Deconstruct [elements] Components\n \n As a layer of abstraction, we chose to combine topology and geometry:\n - Vertex 'holds' the points\n - Edge   'holds' the 3d curve \n - Trim   'holds' the 2d curve\n - Loops  'holds' no geometry\n - Faces  'holds' the surfaces", SD.PluginTitle, SD.PluginCategory7)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep", (GH_ParamAccess)0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Faces", "Fi", "Face Indices", (GH_ParamAccess)1);
            pManager.AddIntegerParameter("Loops", "Li", "Loop Indices", (GH_ParamAccess)1);
            pManager.AddIntegerParameter("Trims", "Ti", "Trim Indices", (GH_ParamAccess)1);
            pManager.AddIntegerParameter("Edges", "Ei", "Edge Indices", (GH_ParamAccess)1);
            pManager.AddIntegerParameter("Vertices", "Vi", "Vertex Indices", (GH_ParamAccess)1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            DA.GetData<Brep>(0, ref brep);
            if (brep == null)
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)(GH_RuntimeMessageLevel)20, "Input bad");
            }
            else
            {
                DA.SetDataList(0, from item in brep.Faces select item.FaceIndex);
                DA.SetDataList(1, from item in brep.Loops select item.LoopIndex);
                DA.SetDataList(2, from item in brep.Trims select item.TrimIndex);
                DA.SetDataList(3, from item in brep.Edges select item.EdgeIndex);
                DA.SetDataList(4, from item in brep.Vertices select item.VertexIndex);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("cfebde88-6a39-462c-968e-fa3b684b9eaa");
    }
}
