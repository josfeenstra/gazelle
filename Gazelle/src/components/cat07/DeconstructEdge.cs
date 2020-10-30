// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace Gazelle
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using Gazelle.Properties;
    using System;
    using System.Drawing;
    
    public class DeconstructEdge : GH_Component
    {
        public DeconstructEdge() : base(SD.Starter + "Deconstruct Edge", "DeEdge", SD.CopyRight + "Deconstruct A BrepEdge of a Brep", SD.PluginTitle, SD.PluginCategory7)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep", (GH_ParamAccess)0);
            pManager.AddIntegerParameter("Edge Index", "Ei", "index of trim", (GH_ParamAccess)0, 0);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve 3d", "C", "3d curve linked to the edge", (GH_ParamAccess)0);
            pManager.AddIntegerParameter("Face Indices", "Fi", "all adjacent faces", (GH_ParamAccess)1);
            pManager.AddIntegerParameter("Trim Indices", "Ti", "the trims this edge represents in adjacent faces", (GH_ParamAccess)1);
            pManager.AddIntegerParameter("Vertex indices", "Vi", "0 is start, 1 is end", (GH_ParamAccess)1);
            pManager.AddTextParameter("Valence", "Val", "EdgeAdjacency", (GH_ParamAccess)0);
            pManager.AddBooleanParameter("Reversed", "Rev", "IsProxyCurveReversed", (GH_ParamAccess)0);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            int num = -1;
            DA.GetData<Brep>(0, ref brep);
            DA.GetData<int>(1, ref num);
            if (brep == null)
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)20, "Input bad");
            }
            else if ((num < 0) || (num >= brep.Edges.Count))
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)10, "out of range");
            }
            else
            {
                BrepEdge edge = brep.Edges[num];
                DA.SetData(0, edge.DuplicateCurve());
                DA.SetDataList(1, edge.AdjacentFaces());
                DA.SetDataList(2, edge.TrimIndices());
                int[] numArray1 = new int[] { edge.StartVertex.VertexIndex, edge.EndVertex.VertexIndex };
                DA.SetDataList(3, numArray1);
                DA.SetData(4, edge.Valence.ToString());
                DA.SetData(5, edge.ProxyCurveIsReversed);
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("fc53b1fb-3950-4865-abcd-70f3de2b3bac");
    }
}
