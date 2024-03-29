// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace Gazelle
{
    using Grasshopper.Kernel;
    using Grasshopper.Kernel.Types;
    using Rhino.Geometry;
    using Gazelle.Properties;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    
    public class ComponentGeoCutCurve : GH_Component
    {
        public int Mode;
        public List<string> ModeNames;
        
        public ComponentGeoCutCurve() : base(SD.Starter + "Cut Curve", SD.Starter + "Cut", SD.CopyRight + "Cut a curve into subcurves using various geometry:\nPoint -> cut at closest point on curve\nNumber -> cut at this t value\nPlane -> cut at intersection of this planeCurve -> cut at intersection of this other curve", SD.PluginTitle, SD.PluginCategory11)
        {
            List<string> list1 = new List<string>();
            list1.Add("Open Curve NOT SAVED");
            list1.Add("Closed Curve NOT SAVED");
            this.ModeNames = list1;
            this.Mode = 0;
        }
        
        //public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        //{
        //    GH_DocumentObject.Menu_AppendSeparator(menu);
        //    GH_DocumentObject.Menu_Appen[menu, "Choose Mode:", null, false];
        //    int num = 0;
        //    while (true)
        //    {
        //        if (num >= this.ModeNames.Count)
        //        {
        //            GH_DocumentObject.Menu_AppendSeparator(menu);
        //            base.AppendAdditionalMenuItems(menu);
        //            return;
        //        }
        //        string str = this.ModeNames[num];
        //        bool flag = num == this.Mode;
        //        ToolStripMenuItem item = GH_DocumentObject.Menu_Appen[menu, str, new EventHandler(this.MenuSetMode], true, flag);
        //        item.Name = num.ToString();
        //        num++;
        //    }
        //}
        
        //private void MenuSetMode(object sender, EventArgs e)
        //{
        //    int result = -1;
        //    int.TryParse((sender as ToolStripMenuItem).Name, out result);
        //    if (result == -1)
        //    {
        //        base.Message = "ERROR: SET MODE -1";
        //    }
        //    this.Mode = result;
        //    this.ExpireSolution(true);
        //}
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to cut.", (GH_ParamAccess)0);
            pManager.AddGenericParameter("Splitters", "S", "Numbers, Points or Planes to cut with", (GH_ParamAccess)1);
            pManager.AddBooleanParameter("Options", "O", "Bool 0: OnlyClosestPlaneCut \n Bool 1: clipSubdivision \n Bool 2: planeFixSide Bool 3: planeFixClosest ", (GH_ParamAccess)1);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("SubCurves", "C", "Resulting Subcurves", (GH_ParamAccess)1);
            pManager.AddPointParameter("Split Points", "P", "points of splitting", (GH_ParamAccess)1);
            pManager.AddNumberParameter("t-Values", "t", "Splitting points of original curve", (GH_ParamAccess)1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> list3;
            List<Point3d> list4;
            Curve[] curveArray;
            List<string> list5;
            GH_Curve curve = new GH_Curve();
            DA.GetData<GH_Curve>(0, ref curve);
            Curve curve2 = curve.Value.DuplicateCurve();
            List<IGH_Goo> splitters = new List<IGH_Goo>();
            DA.GetDataList<IGH_Goo>(1, splitters);
            List<GH_Boolean> options = new List<GH_Boolean>();
            DA.GetDataList<GH_Boolean>(2, options);
            if (!CurveFunctions.CutCurves(curve2, splitters, options, out list3, out list4, out curveArray, out list5))
            {
            }
            foreach (string str in list5)
            {
                this.AddRuntimeMessage((GH_RuntimeMessageLevel)(GH_RuntimeMessageLevel)1, str);
            }
            DA.SetDataList(0, curveArray);
            DA.SetDataList(1, list4);
            DA.SetDataList(2, list3);
        }
        
        protected override Bitmap Icon =>
            Resources.CutCurve;
        
        public override Guid ComponentGuid =>
            new Guid("359b021b-ebd3-42a4-aa96-33b87348f5a4");
    }
}
