// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.Geo
{
    using Grasshopper.Kernel;
    using Grasshopper.Kernel.Data;
    using Rhino.Geometry;
    using SferedApi;
    using SferedApi.Properties;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    
    public class ComponentGeoProjectText : GH_Component
    {
        public double letterWidth;
        public double letterHeight;
        public double letterDepth;
        public const double ClosestPointPrecision = 10.0;
        public const double ProjectionTolerance = 1E-06;
        public const double PreProjectionHoverHeight = 1.0;
        public List<int> Settings;
        private string SETTINGS_DESCRIPTION;
        private GH_Structure<IGH_Goo> letterRawData;
        private List<FontCustomCharacter> LetterDataList;
        
        public ComponentGeoProjectText() : base(SD.Starter + "Project Text", SD.Starter + "Project Text", SD.CopyRight + "Project a Text to a brep", SD.PluginTitle, SD.PluginCategory5)
        {
            this.SETTINGS_DESCRIPTION = "Setting 0: Is Text Centered. If set to 1, text will be centered at plane point";
            List<int> list1 = new List<int>();
            list1.Add(0);
            list1.Add(0);
            list1.Add(0);
            this.Settings = list1;
        }
        
        private bool FingerprintTest(Brep[] breps, Brep fingerprint, ref List<object> test)
        {
            bool flag2;
            if (breps.Length != 1)
            {
                test.Add("Not 1 brep");
                flag2 = false;
            }
            else
            {
                Brep brep = breps[0];
                if (fingerprint.get_IsSolid())
                {
                    if (brep.get_IsSolid())
                    {
                        test.Add("Good: Both Solid");
                        flag2 = true;
                    }
                    else
                    {
                        test.Add("Fingerprint Solid, brep is not");
                        flag2 = false;
                    }
                }
                else
                {
                    Curve[] curveArray = fingerprint.DuplicateNakedEdgeCurves(true, true);
                    Curve[] curveArray2 = brep.DuplicateNakedEdgeCurves(true, true);
                    if (curveArray2.Length > curveArray.Length)
                    {
                        test.Add("Brep has more naked edges than start brep(fingerprint)");
                        flag2 = false;
                    }
                    else if (curveArray2.Length < curveArray.Length)
                    {
                        test.Add("somehow, brep has less naked edges than start brep(fingerprint)");
                        flag2 = false;
                    }
                    else
                    {
                        test.Add("Good: Same number of naked edges.");
                        flag2 = true;
                    }
                }
            }
            return flag2;
        }
        
        private Brep[] GenerateGeometry(Vector3d projectionVector, List<Brep> leftover, List<Brep> holes)
        {
            List<Brep> list = new List<Brep>();
            List<Brep> collection = new List<Brep>();
            Brep[] brepArray = Brep.JoinBreps(holes, 0.001);
            List<Curve> list3 = new List<Curve>();
            foreach (Brep brep in brepArray)
            {
                Curve[] curveArray = Curve.JoinCurves(brep.DuplicateNakedEdgeCurves(true, false), 0.001);
                list3.AddRange(curveArray);
            }
            list3 = Curve.JoinCurves(list3, 0.001).ToList<Curve>();
            Vector3d vectord = projectionVector * this.letterDepth;
            foreach (Brep brep2 in holes)
            {
                brep2.Translate(vectord);
            }
            foreach (Curve curve in list3)
            {
                Curve item = curve.DuplicateCurve();
                item.Translate(vectord);
                List<Curve> list1 = new List<Curve>();
                list1.Add(curve);
                list1.Add(item);
                Brep[] brepArray4 = Brep.CreateFromLoft(list1, Point3d.get_Unset(), Point3d.get_Unset(), 0, false);
                Brep brep3 = brepArray4[0];
                collection.Add(brep3);
            }
            list.AddRange(brepArray);
            list.AddRange(collection);
            list.AddRange(leftover);
            return Brep.JoinBreps(list, 0.001);
        }
        
        private bool ProjectCharacter(char character, List<Curve> charCurves, Vector3d projectVector, Brep brep, double depth, out List<IGH_Goo> resultGeo, out List<object> testGeo)
        {
            List<Curve> list = new List<Curve>();
            List<Curve> list2 = new List<Curve>();
            resultGeo = new List<IGH_Goo>();
            testGeo = new List<object>();
            foreach (Curve curve in charCurves)
            {
                Curve[] curveArray = Curve.ProjectToBrep(curve, brep, projectVector, 1E-06);
            }
            return true;
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text to project", 0);
            pManager.AddBrepParameter("Brep", "B", "Brep to project onto", 0);
            pManager.AddPlaneParameter("Plane", "P", "Plane to project text insertion from ", 0, Plane.get_WorldXY());
            pManager.AddNumberParameter("Width", " ", "Width of entire string(obsolete)", 0);
            pManager.AddNumberParameter("Height", "H", "Height of Letter", 0, 2.4);
            pManager.AddNumberParameter("Depth", "D", "Depth of Letter", 0, 0.5);
            pManager.AddIntegerParameter("Options", "O", "Additional settings. " + this.SETTINGS_DESCRIPTION, 1);
            pManager.AddGenericParameter("Character Data", "C", "temporary, put the alfabet curves in here", 2);
            pManager.AddNumberParameter("Nudge Distance", "N", "Nudge Distance", 0, 0.5);
            pManager.AddIntegerParameter("Nudge Maximum tries", "M", "Nudge Maximum tries", 0, 1);
            for (int i = 0; i < 10; i++)
            {
                pManager.get_Param(i).set_Optional(true);
            }
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Geometry generated by the insert", 1);
            pManager.AddCurveParameter("Projection curve", "C", "curves as projected on surface", 1);
            pManager.AddGenericParameter("Debug", "D", "geometry for debugging", 1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Vector3d vectord;
            List<Curve> list2;
            string str = "";
            Brep b = new Brep();
            Plane aPlane = new Plane();
            DA.GetData<string>(0, ref str);
            DA.GetData<Brep>(1, ref b);
            DA.GetData<Plane>(2, ref aPlane);
            List<int> list = new List<int>();
            DA.GetData<double>(3, ref this.letterWidth);
            DA.GetData<double>(4, ref this.letterHeight);
            DA.GetData<double>(5, ref this.letterDepth);
            DA.GetDataList<int>(6, list);
            DA.GetDataTree<IGH_Goo>(7, ref this.letterRawData);
            double aStepDistance = 0.0;
            int num2 = 0;
            DA.GetData<double>(8, ref aStepDistance);
            DA.GetData<int>(9, ref num2);
            base.set_Message("");
            this.LetterDataList = new List<FontCustomCharacter>();
            foreach (List<IGH_Goo> list5 in this.letterRawData.get_Branches())
            {
                FontCustomCharacter item = new FontCustomCharacter(list5);
                this.LetterDataList.Add(item);
            }
            int num5 = 0;
            while (true)
            {
                if ((num5 < list.Count) && (num5 < this.Settings.Count))
                {
                    this.Settings[num5] = list[num5];
                    num5++;
                    continue;
                }
                vectord = aPlane.get_ZAxis() * -1.0;
                double num3 = 0.0;
                aPlane.Translate(vectord * 1.0);
                list2 = new List<Curve>();
                double num4 = 0.0;
                int index = 0;
                while (true)
                {
                    if (index < str.Length)
                    {
                        char ch = str.ToLower().ToCharArray()[index];
                        FontCustomCharacter objA = null;
                        foreach (FontCustomCharacter character3 in this.LetterDataList)
                        {
                            if (character3.Character == ch)
                            {
                                objA = character3.Duplicate();
                                break;
                            }
                        }
                        if (!object.ReferenceEquals(objA, null))
                        {
                            List<Curve> curveList = objA.CurveList;
                            Point3d pointd = objA.Plane.get_Origin();
                            Plane plane = objA.Plane;
                            num4 = this.letterHeight / objA.Height;
                            num3 += objA.Width;
                            Vector3d vectord2 = new Vector3d(objA.Width / 2.0, 0.0, 0.0);
                            Vector3d vectord3 = new Vector3d(num3, 0.0, 0.0);
                            Transform transform = Transform.PlaneToPlane(plane, aPlane);
                            Transform transform2 = Transform.Scale(plane, num4, num4, 1.0);
                            foreach (Curve curve in curveList)
                            {
                                curve.Translate(vectord2 + vectord3);
                                curve.Transform(transform2);
                                curve.Transform(transform);
                            }
                            foreach (Curve curve2 in curveList)
                            {
                                list2.Add(curve2);
                            }
                            index++;
                        }
                        else
                        {
                            this.AddRuntimeMessage(10, "Character '" + ch.ToString() + "' has no loaded geometry");
                            return;
                        }
                        continue;
                    }
                    else if (this.Settings[0] == 1)
                    {
                        Vector3d vectord4 = aPlane.get_XAxis() * ((num3 * num4) * -0.5);
                        foreach (Curve curve3 in list2)
                        {
                            curve3.Translate(vectord4);
                        }
                    }
                    break;
                }
                break;
            }
            Nudger nudger = new Nudger(aStepDistance, aPlane);
            Brep[] brepArray = null;
            List<Curve> list3 = new List<Curve>();
            List<object> list4 = new List<object>();
            int finalStep = 0;
            while (true)
            {
                if (finalStep >= num2)
                {
                    break;
                }
                foreach (Curve curve4 in list2)
                {
                    Vector3d step = nudger.GetStep(finalStep);
                    curve4.Translate(step);
                }
                List<Brep> allSurfacesOutside = new List<Brep>();
                List<Brep> allSurfacesInside = new List<Brep>();
                List<Curve> outCurves = new List<Curve>();
                List<object> allTests = new List<object>();
                List<string> list11 = new List<string>();
                ComponentGeoProjectPerforate.ProjectAndPerforateBrep(list2, b, vectord, 0.001, ref allSurfacesInside, ref allSurfacesOutside, ref outCurves, ref allTests);
                List<Curve> list12 = Curve.JoinCurves(outCurves, 0.01).ToList<Curve>();
                if (!true)
                {
                    this.AddRuntimeMessage(0xff, "Perforation returned negative.");
                }
                Brep[] breps = this.GenerateGeometry(vectord, allSurfacesOutside, allSurfacesInside);
                if ((finalStep == (num2 - 1)) || this.FingerprintTest(breps, b, ref allTests))
                {
                    if (this.FingerprintTest(breps, b, ref allTests))
                    {
                        allTests.Add("Geometry is correct!");
                    }
                    else
                    {
                        this.AddRuntimeMessage(10, "Geometry Incorrect. Maximum number of nudges used.");
                        allTests.Add("Geometry Incorrect. Maximum number of nudges used.");
                    }
                    allTests.Add("Number of Nudge Tries: " + finalStep.ToString());
                    brepArray = breps;
                    list3 = list2;
                    list4 = allTests;
                    break;
                }
                finalStep++;
            }
            DA.SetDataList(0, brepArray);
            DA.SetDataList(1, list3);
            DA.SetDataList(2, list4);
        }
        
        protected override Bitmap Icon =>
            Resources.ProjectText;
        
        public override Guid ComponentGuid =>
            new Guid("31d8b09a-f539-4ecb-846c-2a90d287e8b8");
    }
}