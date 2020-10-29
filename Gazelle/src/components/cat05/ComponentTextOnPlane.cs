// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.TextInsertion
{
    using Grasshopper.Kernel;
    using Grasshopper.Kernel.Data;
    using Grasshopper.Kernel.Types;
    using Rhino.Geometry;
    using SferedApi;
    using SferedApi.Properties;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    
    public class ComponentTextOnPlane : GH_Component
    {
        public ComponentTextOnPlane() : base(
            SD.Starter + "Text On Plane", 
            SD.Starter + "Text", 
            SD.CopyRight + "Get the letters and numbers of a certain font as curves on a plane.", 
            SD.PluginTitle, 
            SD.PluginCategory5)
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text to project", 0);
            pManager.AddPlaneParameter("Plane", "P", "Plane to project text insertion from ", 0, Plane.WorldXY);
            pManager.AddGenericParameter("Font Data", "F", "the font to use", (GH_ParamAccess)2);
            pManager.AddNumberParameter("Height", "H", "Height of Letter", 0, 2.4);
            pManager.AddBooleanParameter("Option Centered", "C", "if true, text will be centered on plane", 0, true);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Projection curves", "C", "curves as projected on plane", (GH_ParamAccess)1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> structure;
            string str = "";
            Plane plane = new Plane();
            double num = 2.4;
            bool flag = false;
            DA.GetData<string>(0, ref str);
            DA.GetData<Plane>(1, ref plane);
            DA.GetDataTree<IGH_Goo>(2, out structure);
            DA.GetData<double>(3, ref num);
            DA.GetData<bool>(4, ref flag);
            List<FontCustomCharacter> list = new List<FontCustomCharacter>();
            foreach (List<IGH_Goo> list3 in structure.Branches)
            {
                FontCustomCharacter item = new FontCustomCharacter(list3);
                list.Add(item);
            }
            double num2 = 0.0;
            List<Curve> list2 = new List<Curve>();
            double num3 = 0.0;
            int index = 0;
            while (true)
            {
                if (index < str.Length)
                {
                    char ch = str.ToLower().ToCharArray()[index];
                    FontCustomCharacter objA = null;
                    foreach (FontCustomCharacter character3 in list)
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
                        Point3d pointd = objA.Plane.Origin;
                        Plane plane2 = objA.Plane;
                        num3 = num / objA.Height;
                        num2 += objA.Width;
                        Vector3d vectord = new Vector3d(objA.Width / 2.0, 0.0, 0.0);
                        Vector3d vectord2 = new Vector3d(num2, 0.0, 0.0);
                        Transform transform = Transform.PlaneToPlane(plane2, plane);
                        Transform transform2 = Transform.Scale(plane2, num3, num3, 1.0);
                        foreach (Curve curve in curveList)
                        {
                            curve.Translate(vectord + vectord2);
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
                        this.AddRuntimeMessage((GH_RuntimeMessageLevel)10, "Character '" + ch.ToString() + "' has no loaded geometry");
                        return;
                    }
                    continue;
                }
                else if (flag)
                {
                    Vector3d vectord3 = plane.XAxis * ((num2 * num3) * -0.5);
                    foreach (Curve curve3 in list2)
                    {
                        curve3.Translate(vectord3);
                    }
                }
                break;
            }
            DA.SetDataList(0, list2);
        }
        
        protected override Bitmap Icon =>
            Resources.ProjectText;
        
        public override Guid ComponentGuid =>
            new Guid("1bcdaac7-5234-4487-b6ef-79b4f19f8c43");
    }
}
