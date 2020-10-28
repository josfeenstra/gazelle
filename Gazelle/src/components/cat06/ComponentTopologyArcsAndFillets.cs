// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using SferedApi.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SferedApi.Components
{

    public class ArcsAndFillets : GH_Component
    {
        private static string DESCRIPTION1 = "The distance the 'midpoint' of the arc is moved. The first bulge value corresponds to arc between the first and second point, et cetera";
        
        public ArcsAndFillets() : base(SD.Starter + "Spline polyArc", SD.Starter + "S PolyArc", SD.CopyRight + "Creates a closed polyarc with filleted corners." + SD.PluginCategory6Description, SD.PluginTitle, SD.PluginCategory6)
        {
        }
        
        public NurbsCurve BuildArc(Point3d p1, Point3d p2, Vector3d vec, double r, ref List<object> assistors, ref Point3d inBetweenPoint)
        {
            assistors.Add(new Line(p1, p2));
            Point3d pointd = (p1 + p2) / 2.0;
            vec.Unitize();
            double num = r;
            vec *= num;
            pointd += vec;
            inBetweenPoint = pointd;
            Point3d pointd2 = ((pointd * 2.0) + p1) / 3.0;
            Point3d pointd3 = ((pointd * 2.0) + p2) / 3.0;
            Point3d[] pointdArray1 = new Point3d[] { p1, pointd2, pointd3, p2 };
            return NurbsCurve.Create(false, 3, pointdArray1);
        }
        
        public void BuildFillet(int index1, int index2, ref List<NurbsCurve> arcs, Point3d point, double radius, Plane plane, ref NurbsCurve C, ref List<object> assistors)
        {
            double num = 0.02;
            if (!(radius == 0.0))
            {
                NurbsCurve curve = arcs[index1];
                NurbsCurve curve2 = arcs[index2];
                Curve curve3 = null;
                curve3 = (arcs[index1].GetLength() >= arcs[index2].GetLength()) ? ((Curve) arcs[index2]) : ((Curve) arcs[index1]);
                if (curve3.GetLength() < radius)
                {
                    radius = curve3.GetLength() - 0.2;
                    if (radius < 0.2)
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "arc too small!index1: " + index1);
                        radius = 0.2;
                    }
                }
                plane.Origin = point;
                Circle circle = new Circle(plane, radius);
                double num2 = 0.001;
                CurveIntersections intersections = Intersection.CurveCurve(curve, circle.ToNurbsCurve(), num2, 0.001);
                if (intersections.Count > 1)
                {
                    throw new Exception("more than one intersection");
                }
                if (intersections.Count < 1)
                {
                    throw new Exception("NO intersection");
                }
                double num3 = intersections[0].ParameterA;
                arcs[index1] = (curve.Domain.T0 < num3) ? 
                    curve.Trim(curve.Domain.T0, num3).ToNurbsCurve() : 
                    curve.Trim(curve.Domain.T0, num3 + num).ToNurbsCurve();

                CurveIntersections intersections2 = Intersection.CurveCurve(curve2, circle.ToNurbsCurve(), num2, 0.001);
                if (intersections2.Count > 1)
                {
                    throw new Exception("more than one intersection");
                }
                if (intersections2.Count < 1)
                {
                    throw new Exception("NO intersection");
                }
                double num4 = intersections2[0].ParameterA;
                arcs[index2] = (num4 < curve2.Domain.T1) ? curve2.Trim(num4, curve2.Domain.T1).ToNurbsCurve() : curve2.Trim(num4 - num, curve2.Domain.T1).ToNurbsCurve();
                double num5 = (((point - arcs[index1].PointAtEnd) / 3.0) * 2.0).get_Length();
                Point3d pointd = arcs[index1].PointAtEnd + (arcs[index1].get_TangentAtEnd() * num5);
                double num6 = (((point - arcs[index2].PointAtStart) / 3.0) * 2.0).get_Length();
                Point3d pointd2 = arcs[index2].PointAtStart - (arcs[index2].get_TangentAtStart() * num6);
                Point3d[] pointdArray1 = new Point3d[] { arcs[index1].PointAtEnd, pointd, pointd2, arcs[index2].get_PointAtStart() };
                C = NurbsCurve.Create(false, 3, pointdArray1);
            }
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Control Points of the polyArc", 1);
            pManager.AddPlaneParameter("Coordinate System (Plane)", "S", "Coordinate System(Plane)", 0, Plane.get_WorldXY());
            pManager.AddNumberParameter("Arc Bulge Distance", "A", DESCRIPTION1, 1);
            pManager.AddNumberParameter("Fillet Radius", "F", "the fillet value of every point. a fillet of 0 will ignore fillets.", 1);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("PolyArc", "PA", "The Resulting PolyArc", 0);
            pManager.AddCurveParameter("Curves", "C", "all Pieces", 1);
            pManager.AddCurveParameter("Arcs", "A", "Arcs", 1);
            pManager.AddCurveParameter("Fillets", "F", "Fillets", 1);
            pManager.AddPointParameter("Control Points", "P", "Control Points of All Curves", 1);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> collection = new List<Point3d>();
            Plane plane = Plane.get_Unset();
            List<double> list2 = new List<double>();
            List<double> list3 = new List<double>();
            DA.GetDataList<Point3d>(0, collection);
            DA.GetData<Plane>(1, ref plane);
            DA.GetDataList<double>(2, list2);
            DA.GetDataList<double>(3, list3);
            if ((collection.Count != list2.Count) || (list2.Count != list3.Count))
            {
                throw new Exception("use equal lists!");
            }
            int count = collection.Count;
            List<object> assistors = new List<object>();
            DataTree<object> tree = new DataTree<object>();
            int num2 = 0;
            while (true)
            {
                if (num2 >= count)
                {
                    Vector3d[] vectordArray = new Vector3d[count];
                    List<Point3d> list5 = new List<Point3d>();
                    list5.AddRange(collection);
                    list5.Add(collection[0]);
                    Curve curve = new Polyline(list5).ToNurbsCurve();
                    int index = 0;
                    while (true)
                    {
                        double num4;
                        if (index >= count)
                        {
                            List<NurbsCurve> arcs = new List<NurbsCurve>();
                            int num5 = 0;
                            while (true)
                            {
                                if (num5 >= collection.Count)
                                {
                                    List<NurbsCurve> list7 = new List<NurbsCurve>();
                                    int num6 = 0;
                                    while (true)
                                    {
                                        if (num6 >= collection.Count)
                                        {
                                            List<NurbsCurve> list8 = new List<NurbsCurve>(arcs.Count + list7.Count);
                                            int num9 = 0;
                                            while (true)
                                            {
                                                if (num9 >= arcs.Count)
                                                {
                                                    Curve[] curveArray = Curve.JoinCurves((IEnumerable<Curve>) list8);
                                                    if (curveArray.Length != 1)
                                                    {
                                                        throw new Exception("fullCurve cant join all curves");
                                                    }
                                                    NurbsCurve curve2 = curveArray[0].ToNurbsCurve();
                                                    List<Point3d> list9 = (from x in curve2.get_Points() select x.get_Location()).ToList<Point3d>();
                                                    list9.RemoveAt(list9.Count - 1);
                                                    DA.SetData(0, curve2);
                                                    DA.SetDataList(1, list8);
                                                    DA.SetDataList(2, arcs);
                                                    DA.SetDataList(3, list7);
                                                    DA.SetDataList(4, list9);
                                                    return;
                                                }
                                                list8.Add(list7[num9]);
                                                list8.Add(arcs[num9]);
                                                num9++;
                                            }
                                        }
                                        int num8 = num6;
                                        bool flag7 = num6 < 1;
                                        int num7 = !flag7 ? (num6 - 1) : (collection.Count - 1);
                                        NurbsCurve c = null;
                                        this.BuildFillet(num7, num8, ref arcs, collection[num6], list3[num6], plane, ref c, ref assistors);
                                        list7.Add(c);
                                        num6++;
                                    }
                                }
                                Point3d pointd = collection[num5];
                                bool flag4 = num5 >= (collection.Count - 1);
                                Point3d pointd2 = !flag4 ? collection[num5 + 1] : collection[0];
                                Point3d inBetweenPoint = Point3d.get_Unset();
                                NurbsCurve item = this.BuildArc(pointd, pointd2, vectordArray[num5], list2[num5], ref assistors, ref inBetweenPoint);
                                if (!item.get_IsValid())
                                {
                                    throw new Exception("arc " + num5 + " is invalid");
                                }
                                arcs.Add(item);
                                num5++;
                            }
                        }
                        curve.ClosestPoint(collection[index], ref num4);
                        Vector3d vectord = curve.TangentAt(num4);
                        vectord.Rotate(1.5707963267948966, plane.get_ZAxis());
                        vectordArray[index] = vectord;
                        index++;
                    }
                }
                collection[num2] = plane.ClosestPoint(collection[num2]);
                num2++;
            }
        }
        
        protected override Bitmap Icon =>
            Resources.TopoArcsFillets;
        
        public override Guid ComponentGuid =>
            new Guid("392e64f4-1ec6-4879-82d5-303aac9bfccf");
        
        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ArcsAndFillets.<>c <>9 = new ArcsAndFillets.<>c();
            public static Func<ControlPoint, Point3d> <>9__4_0;
            
            internal Point3d <SolveInstance>b__4_0(ControlPoint x) => 
                x.get_Location();
        }
    }
}
