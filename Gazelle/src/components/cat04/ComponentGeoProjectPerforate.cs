// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace Gazelle.Components.Geo
{
    using Grasshopper.Kernel;
    using Rhino.Geometry;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    
    public class ComponentGeoProjectPerforate : GH_Component
    {
        public ComponentGeoProjectPerforate() : 
            base(SD.Starter + "Project Perforate", 
                SD.Starter + "ProPerf", 
                SD.CopyRight + "Projects closed curves onto a brep (or surface), " +
                "and cuts the surface with those projections. \n" +
                "It assigns the cut geometry to the inner list if the surface is within de cut region", 
                SD.PluginTitle, 
                SD.PluginCategory4)
        {
        }
        
        private static int GetInitialSurface(
            BrepFace face, Brep splitted, double tolerance, out Point3d chosenPoint, ref List<object> test)
        {
            int num7;
            int num = -1;
            chosenPoint = Point3d.Unset;
            int num2 = splitted.Faces.Count;
            List<Curve> list = new List<Curve>();
            foreach (BrepFace face2 in splitted.Faces)
            {
                Curve curve = face2.OuterLoop.To2dCurve();
                if (curve == null)
                {
                    test.Add("CANT PULL CURVE");
                    continue;
                }
                list.Add(curve);
            }
            Random random = new Random(0);
            Point3d item = Point3d.Unset;
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
                            PointContainment containment = curve2.Contains(item, Plane.WorldXY, tolerance);
                            if (containment != 0)
                            {
                                if (containment != (PointContainment)3)
                                {
                                    if (containment == (PointContainment)1)
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
                                    if (containment == (PointContainment)2)
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
                                test.Add("pointContainment is unset, the curve or the point are unworkable!");
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
                    test.Add($"Tries : {num3}");
                    test.Add("PointOnTrim:");
                    test.Add(item);
                    chosenPoint = face.PointAt(item.X, item.Y);
                    test.Add("chosenPoint:");
                    test.Add((Point3d) chosenPoint);
                    num7 = num;
                }
                break;
            }
            return num7;
        }
        
        private static bool IsPointInCurves(Curve[] curves, Point3d point, Vector3d vector, double tolerance, ref List<object> test)
        {
            if (curves.Length == 0)
            {
                throw new Exception("No Curves Given For test");
            }
            if (!point.IsValid)
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
                        PointContainment containment2 = curve.Contains(point, plane, tolerance);
                        switch (containment2)
                        {
                            case 0:
                                test.Add("pointContainment is unset, the curve or the point are unworkable!");
                                break;
                            
                            case (PointContainment)1:
                                flag = !flag;
                                break;
                            
                            case (PointContainment)3:
                                test.Add("POINT IS ON THE EDGE OF ANOTHER POINT at the pulled curve operation");
                                break;
                            
                            default:
                                break;
                        }
                    }
                    test.Add($"isInside: {flag}");
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
            if (thisIndex < splitted.Faces.Count)
            {
                BrepFace face = splitted.Faces[thisIndex];
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
            Plane plane = new Plane(c.PointAtStart, d);
            Box box2 = Box.Unset;
            b.GetBoundingBox(plane, out box);
            Curve curve = Curve.ProjectToPlane(c, plane);
            Curve curve2 = curve.DuplicateCurve();
            curve2.Translate(d * box.Z.T0);
            Curve curve3 = curve.DuplicateCurve();
            curve3.Translate(d * box.Z.T1);
            Curve[] curveArray1 = new Curve[] { curve3, curve2 };
            List<Brep> list = Brep.CreateFromLoft(curveArray1, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList<Brep>();
            Curve[] curveArray2 = new Curve[] { curve3, curve2 };
            list.AddRange(Brep.CreatePlanarBreps(curveArray2));
            Brep[] brepArray = Brep.JoinBreps(list, tolerance);
            if (brepArray.Length != 1)
            {
                throw new Exception("extrution is not closed.");
            }
            return brepArray[0];
        }
        
        public static void ProjectAndPerforateBrep(List<Curve> C, Brep B, Vector3d D, double pointTol, ref List<Brep> allSurfacesInside, ref List<Brep> allSurfacesOutside, ref List<Curve> outCurves, ref List<object> allTests)
        {
            List<Curve> objA = C;
            Brep brep = B;
            Vector3d d = D;
            d.Unitize();
            if (object.ReferenceEquals(objA, null))
            {
                throw new Exception("no Curves");
            }
            int num2 = 0;
            while (true)
            {
                if (num2 >= objA.Count)
                {
                    if (brep == null)
                    {
                        throw new Exception("no Brep");
                    }
                    double splitTol = 0.01;
                    for (int i = 0; i < brep.Faces.Count; i++)
                    {
                        List<int> list1 = new List<int>();
                        list1.Add(i);
                        Brep b = brep.DuplicateSubBrep(list1);
                        b.Faces.ShrinkFaces();
                        List<Brep> surfaceInside = new List<Brep>();
                        List<Brep> surfaceOutside = new List<Brep>();
                        List<Curve> pCurves = new List<Curve>();
                        List<object> test = new List<object>();
                        allTests.Add($"--- Brep.Faces[{i}] ---");
                        ProjectAndPerforateSurface(objA, b, d, splitTol, pointTol, ref surfaceInside, ref surfaceOutside, ref pCurves, ref test);
                        allSurfacesInside.AddRange(surfaceInside);
                        allSurfacesOutside.AddRange(surfaceOutside);
                        outCurves.AddRange(pCurves);
                        allTests.AddRange(test);
                    }
                    return;
                }
                if (!objA[num2].IsValid)
                {
                    throw new Exception($"curve {num2} is not valid");
                }
                num2++;
            }
        }
        
        public static void ProjectAndPerforateSurface(List<Curve> C, Brep B, Vector3d D, double splitTol, double pointTol, ref List<Brep> surfaceInside, ref List<Brep> surfaceOutside, ref List<Curve> pCurves, ref List<object> test)
        {
            Curve[] curves = C.ToArray();
            Brep brep = B;
            Vector3d vector = D;
            vector.Unitize();
            if (brep.Faces.Count > 1)
            {
                throw new Exception("brep has multiple faces");
            }
            foreach (Curve curve in curves)
            {
                if (!curve.IsClosed)
                {
                    throw new Exception("not all curves are closed");
                }
            }
            if (!vector.IsValid)
            {
                throw new Exception("vector is not valid");
            }
            int length = curves.Length;
            pCurves = new List<Curve>();
            int index = 0;
            while (true)
            {
                if (index >= length)
                {
                    BrepFace face = brep.Faces.FirstOrDefault<BrepFace>();
                    Brep splitted = face.Split(pCurves, splitTol);
                    if (splitted == null)
                    {
                        test.Add("found no intersections");
                        splitted = brep;
                    }
                    Point3d chosenPoint = Point3d.Unset;
                    int thisIndex = GetInitialSurface(face, splitted, pointTol * 2.0, out chosenPoint, ref test);
                    bool thisFaceShouldBe = IsPointInCurves(curves, chosenPoint, vector, pointTol, ref test);
                    List<bool> spaceMapper = new List<bool>();
                    foreach (BrepFace face2 in splitted.Faces)
                    {
                        spaceMapper.Add(false);
                    }
                    MapSpace(splitted, spaceMapper, thisFaceShouldBe, thisIndex, null);
                    for (int i = 0; i < splitted.Faces.Count; i++)
                    {
                        BrepFace face3 = splitted.Faces[i];
                        if (spaceMapper[i])
                        {
                            surfaceInside.Add(face3.DuplicateFace(false));
                        }
                        else
                        {
                            surfaceOutside.Add(face3.DuplicateFace(false));
                        }
                    }
                    return;
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
                    pCurves.Add(item);
                    num5++;
                }
            }
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep to project curves onto", 0);
            pManager.AddCurveParameter("Curves", "C", "Curves to project and cut with. Must be closed curves", (GH_ParamAccess)1);
            pManager.AddVectorParameter("Vector", "D", "Projection Direction", 0, Vector3d.ZAxis);
            pManager.AddIntegerParameter("Options", "F", "First Projection Option. \n 0 = make all projections, \n 1 = only make the first hit per (sur)face count. \n 2 = only make the first hit of the entire brep count", 0, 0);
            pManager.AddNumberParameter("Tolerance", "T", "the tolerance of closest point operations within code.", 0, 0.0001);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Inner faces", "Bi", "(sur)faces inside the curve projections", (GH_ParamAccess)1);
            pManager.AddBrepParameter("Outer faces", "Bo", "(sur)faces outside the curve projections", (GH_ParamAccess)1);
            pManager.AddCurveParameter("Projected Curves", "C", "the curve projections", (GH_ParamAccess)1);
            pManager.AddGenericParameter("test", "t", "Data regarding the project and cut procedure. If the result is unexpected, look here to see what went wrong", (GH_ParamAccess)1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep b = null;
            List<Curve> c = new List<Curve>();
            Vector3d d = Vector3d.Unset;
            int num = -1;
            double pointTol = 0.0001;
            DA.GetData<Brep>(0, ref b);
            DA.GetDataList<Curve>(1, c);
            DA.GetData<Vector3d>(2, ref d);
            DA.GetData<int>(3, ref num);
            DA.GetData<double>(4, ref pointTol);
            List<Brep> allSurfacesInside = new List<Brep>();
            List<Brep> allSurfacesOutside = new List<Brep>();
            List<Curve> outCurves = new List<Curve>();
            List<object> allTests = new List<object>();
            ProjectAndPerforateBrep(c, b, d, pointTol, ref allSurfacesInside, ref allSurfacesOutside, ref outCurves, ref allTests);
            DA.SetDataList(0, allSurfacesInside);
            DA.SetDataList(1, allSurfacesOutside);
            DA.SetDataList(2, outCurves);
            DA.SetDataList(3, allTests);
        }
        

        public override Guid ComponentGuid =>
            new Guid("5602cad1-16a1-4000-a86e-ab2c17488bb1");
    }
}
