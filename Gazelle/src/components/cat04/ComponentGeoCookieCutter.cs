// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace SferedApi.Components.Geo
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using SferedApi.Properties;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    
    public class ComponentGeoCookieCutter : GH_Component
    {
        public List<object> testData;
        
        public ComponentGeoCookieCutter() : base(SD.Starter + "CookieCutter", SD.Starter + "Cutter", SD.CopyRight + "Projects closed curves onto a brep (or surface), and cuts the surface with those projections. \n", SD.PluginTitle, SD.PluginCategory10)
        {
            this.testData = new List<object>();
        }
        
        private int GetInitialSurface(BrepFace face, Brep splitted, out Point3d chosenPoint)
        {
            int num7;
            int num = -1;
            chosenPoint = Point3d.get_Unset();
            int num2 = splitted.get_Faces().get_Count();
            List<Curve> list = new List<Curve>();
            foreach (BrepFace face2 in splitted.get_Faces())
            {
                Curve curve = face2.get_OuterLoop().To2dCurve();
                if (curve == null)
                {
                    this.testData.Add("CANT PULL CURVE");
                    continue;
                }
                list.Add(curve);
            }
            Random random = new Random(0);
            Point3d item = Point3d.get_Unset();
            int num3 = 0;
            int num4 = 0x3e8;
            bool flag = true;
            while (true)
            {
                if (flag)
                {
                    Interval interval = face.Domain(1);
                    item = new Point3d(face.Domain(0).ParameterAt(random.NextDouble()), interval.ParameterAt(random.NextDouble()), 0.0);
                    num = -1;
                    flag = false;
                    int num5 = 0;
                    while (true)
                    {
                        if (num5 < num2)
                        {
                            Curve curve2 = list[num5];
                            PointContainment containment = curve2.Contains(item, Plane.get_WorldXY(), 0.001);
                            if (containment != 0)
                            {
                                if (containment != 3)
                                {
                                    if (containment == 1)
                                    {
                                        if (num == -1)
                                        {
                                            num = num5;
                                        }
                                        else if (curve2.GetLength() < list[num].GetLength())
                                        {
                                            num = num5;
                                        }
                                    }
                                    if (containment == 2)
                                    {
                                    }
                                    num5++;
                                    continue;
                                }
                                num3++;
                                flag = true;
                            }
                            else
                            {
                                this.testData.Add("pointContainment is unset, the curve or the point are unworkable!");
                                return -1;
                            }
                        }
                        if (num == -1)
                        {
                            num3++;
                            flag = true;
                        }
                        if (num3 < num4)
                        {
                            break;
                        }
                        throw new Exception($"the 'get a random point' procedure is past try nr. {num4}");
                    }
                    continue;
                }
                else
                {
                    if (num == -1)
                    {
                        throw new Exception("chosen ID = -1, something went wrong in process 2");
                    }
                    this.testData.Add($"Tries : {num3}");
                    this.testData.Add("PointOnTrim:");
                    this.testData.Add(item);
                    chosenPoint = face.PointAt(item.get_X(), item.get_Y());
                    this.testData.Add("chosenPoint:");
                    this.testData.Add((Point3d) chosenPoint);
                    num7 = num;
                }
                break;
            }
            return num7;
        }
        
        private bool IsPointInCurves(Curve[] curves, Point3d point, Vector3d vector)
        {
            if (curves.Length == 0)
            {
                throw new Exception("No Curves Given For test");
            }
            if (!point.get_IsValid())
            {
                throw new Exception("Point is not valid");
            }
            Plane plane = new Plane(point, vector);
            int length = curves.Length;
            Curve[] curveArray = new Curve[curves.Length];
            int index = 0;
            while (true)
            {
                if (index >= length)
                {
                    bool flag = false;
                    foreach (Curve curve in curveArray)
                    {
                        PointContainment containment2 = curve.Contains(point, plane, 0.001);
                        switch (containment2)
                        {
                            case 0:
                                this.testData.Add("pointContainment is unset, the curve or the point are unworkable!");
                                break;
                            
                            case 1:
                                flag = !flag;
                                break;
                            
                            case 3:
                                this.testData.Add("POINT IS ON THE EDGE OF ANOTHER POINT at the pulled curve operation");
                                break;
                            
                            default:
                                break;
                        }
                    }
                    this.testData.Add($"isInside: {flag}");
                    return flag;
                }
                curveArray[index] = Curve.ProjectToPlane(curves[index], plane);
                index++;
            }
        }
        
        private static void MapSpace(Brep splitted, List<bool> spaceMapper, bool thisFaceShouldBe, int thisIndex = 0, List<int> alreadyMapped = null)
        {
            if (object.ReferenceEquals(alreadyMapped, null))
            {
                alreadyMapped = new List<int>();
            }
            if (thisIndex < splitted.get_Faces().get_Count())
            {
                BrepFace face = splitted.get_Faces().get_Item(thisIndex);
                spaceMapper[thisIndex] = thisFaceShouldBe;
                alreadyMapped.Add(thisIndex);
                foreach (int num2 in face.AdjacentFaces())
                {
                    if (!alreadyMapped.Contains(num2))
                    {
                        MapSpace(splitted, spaceMapper, !thisFaceShouldBe, num2, alreadyMapped);
                    }
                }
            }
        }
        
        private Brep OldGetExtrutionShape(Curve c, Brep b, Vector3d d, double tolerance)
        {
            Box box;
            Plane plane = new Plane(c.get_PointAtStart(), d);
            Box box2 = Box.get_Unset();
            b.GetBoundingBox(plane, ref box);
            Curve curve = Curve.ProjectToPlane(c, plane);
            Curve curve2 = curve.Duplicate();
            curve2.Translate(d * box.get_Z().get_T0());
            Curve curve3 = curve.Duplicate();
            curve3.Translate(d * box.get_Z().get_T1());
            Curve[] curveArray1 = new Curve[] { curve3, curve2 };
            List<Brep> list = Brep.CreateFromLoft(curveArray1, Point3d.get_Unset(), Point3d.get_Unset(), 3, false).ToList<Brep>();
            Curve[] curveArray2 = new Curve[] { curve3, curve2 };
            list.AddRange(Brep.CreatePlanarBreps(curveArray2));
            Brep[] brepArray = Brep.JoinBreps(list, tolerance);
            if (brepArray.Length != 1)
            {
                throw new Exception("extrution is not closed.");
            }
            return brepArray[0];
        }
        
        public Results ProjectAndPerforateBrep(List<Curve> C, Brep B, Vector3d D, double pointTol)
        {
            List<Curve> objA = C;
            Brep brep = B;
            Vector3d d = D;
            d.Unitize();
            if (object.ReferenceEquals(objA, null))
            {
                throw new Exception("no Curves");
            }
            int num = 0;
            while (true)
            {
                if (num >= objA.Count)
                {
                    if (brep == null)
                    {
                        throw new Exception("no Brep");
                    }
                    Results results = new Results();
                    for (int i = 0; i < brep.get_Faces().get_Count(); i++)
                    {
                        List<int> list1 = new List<int>();
                        list1.Add(i);
                        Brep b = brep.DuplicateSubBrep(list1);
                        b.get_Faces().ShrinkFaces();
                        this.testData.Add($"--- Brep.Faces[{i}] ---");
                        Results other = this.ProjectAndPerforateSurface(objA, b, d);
                        results.Add(other);
                    }
                    return results;
                }
                if (!objA[num].get_IsValid())
                {
                    throw new Exception($"curve {num} is not valid");
                }
                num++;
            }
        }
        
        public Results ProjectAndPerforateSurface(List<Curve> C, Brep B, Vector3d D)
        {
            Curve[] curves = C.ToArray();
            Brep brep = B;
            Vector3d vector = D;
            vector.Unitize();
            Results results = new Results();
            if (brep.get_Faces().get_Count() > 1)
            {
                throw new Exception("brep has multiple faces");
            }
            foreach (Curve curve in curves)
            {
                if (!curve.get_IsClosed())
                {
                    throw new Exception("not all curves are closed");
                }
            }
            if (!vector.get_IsValid())
            {
                throw new Exception("vector is not valid");
            }
            int length = curves.Length;
            int index = 0;
            while (true)
            {
                if (index >= length)
                {
                    BrepFace face = brep.get_Faces().FirstOrDefault<BrepFace>();
                    Brep splitted = face.Split(results.ProjectedCurves, 0.001);
                    if (splitted == null)
                    {
                        this.testData.Add("found no intersections");
                        splitted = brep;
                    }
                    Point3d chosenPoint = Point3d.get_Unset();
                    int thisIndex = this.GetInitialSurface(face, splitted, out chosenPoint);
                    bool thisFaceShouldBe = this.IsPointInCurves(curves, chosenPoint, vector);
                    List<bool> spaceMapper = new List<bool>();
                    foreach (BrepFace face2 in splitted.get_Faces())
                    {
                        spaceMapper.Add(false);
                    }
                    MapSpace(splitted, spaceMapper, thisFaceShouldBe, thisIndex, null);
                    for (int i = 0; i < splitted.get_Faces().get_Count(); i++)
                    {
                        BrepFace face3 = splitted.get_Faces().get_Item(i);
                        if (spaceMapper[i])
                        {
                            results.SurfacesInside.Add(face3.DuplicateFace(false));
                        }
                        else
                        {
                            results.SurfacesOutside.Add(face3.DuplicateFace(false));
                        }
                    }
                    return results;
                }
                Curve[] curveArray4 = Curve.ProjectToBrep(curves[index], brep, vector, 0.001);
                int num5 = 0;
                while (true)
                {
                    if (num5 >= curveArray4.Length)
                    {
                        index++;
                        break;
                    }
                    Curve item = curveArray4[num5];
                    results.ProjectedCurves.Add(item);
                    num5++;
                }
            }
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep to project curves onto", 0);
            pManager.AddCurveParameter("Curves", "C", "Curves to project and cut with. Must be closed curves", 1);
            pManager.AddVectorParameter("Vector", "D", "Projection Direction", 0, Vector3d.get_ZAxis());
            pManager.AddIntegerParameter("Options", "F", "First Projection Option. \n 0 = make all projections, \n 1 = only make the first hit per (sur)face count. \n 2 = only make the first hit of the entire brep count", 0, 0);
            pManager.AddNumberParameter("Tolerance", "T", "the tolerance of closest point operations within code.", 0, 0.0001);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Inner faces", "Bi", "(sur)faces inside the curve projections", 1);
            pManager.AddBrepParameter("Outer faces", "Bo", "(sur)faces outside the curve projections", 1);
            pManager.AddCurveParameter("Projected Curves", "C", "the curve projections", 1);
            pManager.AddGenericParameter("test", "t", "Data regarding the project and cut procedure. If the result is unexpected, look here to see what went wrong", 1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep b = null;
            List<Curve> c = new List<Curve>();
            Vector3d d = Vector3d.get_Unset();
            int num = -1;
            double pointTol = 0.001;
            DA.GetData<Brep>(0, ref b);
            DA.GetDataList<Curve>(1, c);
            DA.GetData<Vector3d>(2, ref d);
            DA.GetData<int>(3, ref num);
            DA.GetData<double>(4, ref pointTol);
            Results results = this.ProjectAndPerforateBrep(c, b, d, pointTol);
            DA.SetDataList(0, results.SurfacesInside);
            DA.SetDataList(1, results.SurfacesOutside);
            DA.SetDataList(2, results.ProjectedCurves);
            DA.SetDataList(3, this.testData);
        }
        
        protected override Bitmap Icon =>
            Resources.Perforate;
        
        public override Guid ComponentGuid =>
            new Guid("718e3ab9-b8ba-43e8-a969-06904f4a9c97");
        
        public class Results
        {
            public List<Brep> SurfacesInside = new List<Brep>();
            public List<Brep> SurfacesOutside = new List<Brep>();
            public List<Curve> ProjectedCurves = new List<Curve>();
            
            public void Add(ComponentGeoCookieCutter.Results other)
            {
                this.SurfacesInside.AddRange(other.SurfacesInside);
                this.SurfacesOutside.AddRange(other.SurfacesOutside);
                this.ProjectedCurves.AddRange(other.ProjectedCurves);
            }
        }
    }
}