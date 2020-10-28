// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi;
    using SferedApi.Properties;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    
    public class EmbedCurves : GH_Component
    {
        public EmbedCurves() : base(SD.Starter + "Embed Curves", SD.Starter + "Embed", SD.CopyRight, SD.PluginTitle, SD.PluginCategory8)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", 0);
            pManager.AddCurveParameter("Curves", "C", "Closest curves with counter-clockwise orientation.", 1);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "", 0);
            pManager.AddIntegerParameter("Edge Indices", "Ei", "edge indices per face", 2);
            pManager.AddIntegerParameter("Face Indices", "Fi", "Indices of faces this curve interacts with", 1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brep = null;
            List<Curve> curves = new List<Curve>();
            DA.GetData<Brep>(0, ref brep);
            DA.GetDataList<Curve>(1, curves);
            if (brep != null)
            {
                List<List<int>> list2;
                List<int> list3;
                using (List<Curve>.Enumerator enumerator = curves.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        Curve current = enumerator.Current;
                        if (((current == null) || !current.get_IsValid()) || !current.get_IsClosed())
                        {
                            this.AddRuntimeMessage(20, "curve is invalid, missing, or not closed.");
                            return;
                        }
                    }
                }
                BrepFunctions.EmbedCurves(ref brep, curves, out list2, out list3);
                DA.SetData(0, brep);
                DA.SetDataTree(1, GrasshopperFunctions.ToTree<int>((IEnumerable<IEnumerable<int>>) list2));
                DA.SetDataList(2, list3);
            }
            else
            {
                this.AddRuntimeMessage(20, "input bad");
            }
        }
        
        protected override Bitmap Icon =>
            Resources.Sfered_Iconified;
        
        public override Guid ComponentGuid =>
            new Guid("100b7ea2-d680-4a00-9f6f-642e50756d45");
    }
}