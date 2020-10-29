using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;
using Rhino.Geometry.Intersect;


/*
 * 
 * 
 * TODO closest curve setting in interface
 * 
 * 
 * 
 * 
 * 
 */

namespace SferedApi.Components.Geo
{
    public class ComponentGeoQuickSubCurves : GH_Component
    {
        // mode
        public int Mode;
        public List<string> ModeNames;

        /// <summary>
        /// Initializes a new instance of the ComponentGeoQuickSubCurves class.
        /// </summary>
        public ComponentGeoQuickSubCurves()
          : base(   SD.Starter + "Cut Curve",
                    SD.Starter + "Cut",
                    SD.CopyRight +  "Cut a curve into subcurves using various geometry:\n" +
                                    "Point -> cut at closest point on curve\n" + 
                                    "Number -> cut at this t value\n" + 
                                    "Plane -> cut at intersection of this plane" + 
                                    "Curve -> cut at intersection of this other curve",
                    SD.PluginTitle,
                    SD.PluginCategory4)
        {
            ModeNames = new List<string>() { "Open Curve NOT SAVED", "Closed Curve NOT SAVED" };
            Mode = 0;
        }
  
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to cut.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Splitters", "S", "Numbers, Points or Planes to cut with", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Options", "O", "Bool 0: onlyClosestIntersectionPlane \n Bool 1: clipSubdivision \n", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("SubCurves", "C", "Resulting Subcurves", GH_ParamAccess.list);
            pManager.AddPointParameter("Split Points", "P", "points of splitting", GH_ParamAccess.list);
            pManager.AddNumberParameter("t-Values", "t", "Splitting points of original curve", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // input
            var curvePar = new GH_Curve();
            DA.GetData(0, ref curvePar);
            var curve = curvePar.Value.DuplicateCurve();

            var splitters = new List<IGH_Goo>();
            DA.GetDataList(1, splitters);

            var options = new List<GH_Boolean>();
            DA.GetDataList(2, options);

            // process
            List<double> tValues;
            List<Point3d> splitPoints;
            Curve[] subCurves;
            List<string> errorMessage;
            bool succes = CutCurves(curve, splitters, options, out tValues, out splitPoints, out subCurves, out errorMessage);
            if (!succes)
            {

            }
            foreach (var message in errorMessage)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, message);
            }

            // output
            DA.SetDataList(0, subCurves);
            DA.SetDataList(1, splitPoints);
            DA.SetDataList(2, tValues);
        }

        public static bool CutCurves(Curve curve, List<IGH_Goo> splitters, List<GH_Boolean> options, out List<double> tValues, out List<Point3d> splitPoints, out Curve[] subCurves, out List<string> errorMessage)
        {

            // -----------------------------------------------------------------------step 0: interprete options

            // OPTION 0: settings for plane intersection 
            bool onlyClosestIntersectionPlane = false; // only pick the closest intersection with the curve, based upon the middle of the plane 

            // OPTION 1: if true: example: Curve with domain 0 to 1, with splitpoints at 1.5 and 1.6 will be turned into subcurves [0 to 0.98], [0.98 to 0.99] and [0.99 to 1.0]. DEFAULT TRUE
            bool clipSubdivision = true;

            // OPTION 2: hier wilde ik iets maken voor closest curve problemen, maar dit blijft erg lastig om van binnen te doen, 
            // kan beter buiten deze functie gehandeled worden, om zeker te zijn van een consistent resultaat

            // maybe make this a setting too
            double ensureSubdivisionMargin = 0.01;

            for (int i = 0; i < options.Count; i++)
            {
                if (i == 0)
                    onlyClosestIntersectionPlane = options[0].Value;
                if (i == 1)
                    clipSubdivision = options[1].Value;
            }

            // for debugging
            errorMessage = new List<string>();

            // ---------------------------------------------------------------------- step 1: get t values from goo

            // get specific cutters 
            var cutNumbers = new List<double>();
            var cutPoints = new List<Point3d>();
            var cutPlanes = new List<Plane>();
            var cutCurves = new List<Curve>();

            // all cutters as t 
            tValues = new List<double>();
            splitPoints = new List<Point3d>();
            subCurves = null;

            // iterate trough the splitters 
            for (int i = 0; i < splitters.Count; i++)
            {
                IGH_Goo goo = splitters[i];

                // We accept existing nulls.
                if (goo == null) continue;

                // add to number if number
                if (goo is GH_Number)
                {
                    // add number to list of number
                    var aDouble = (goo as GH_Number).Value;
                    cutNumbers.Add(aDouble);

                    // TODO Check Validity???

                    // add to total list 
                    tValues.Add(aDouble);
                }

                // add to point if point
                else if (goo is GH_Point)
                {
                    // add point to a list with all points
                    var aPoint = (goo as GH_Point).Value;
                    cutPoints.Add(aPoint);

                    // add the t value of its closest point on curve 
                    curve.ClosestPoint(aPoint, out double tValue);
                    tValues.Add(tValue);
                }

                // add to plane if plane
                else if (goo is GH_Plane)
                {
                    // add plane to list of all planes 
                    var aPlane = (goo as GH_Plane).Value;
                    cutPlanes.Add(aPlane);

                    // TODO check tolerance problems
                    var intersections = Intersection.CurvePlane(curve, aPlane, SD.IntersectTolerance);

                    if (intersections != null)
                    {

                        // apply setting
                        if (onlyClosestIntersectionPlane)
                        {
                            // find the closest t value
                            double tDistance = double.PositiveInfinity;
                            double tClosest = double.NaN;
                            foreach (var intersection in intersections)
                            {
                                // find out if this intersection is the closest to the centre of the plane 
                                var distance = aPlane.Origin.DistanceTo(intersection.PointA);
                                if (distance < tDistance)
                                {
                                    tDistance = distance;
                                    tClosest = intersection.ParameterA;
                                }
                            }

                            // now add the found closest t value 
                            tValues.Add(tClosest);
                        }
                        else
                        {
                            foreach (var intersection in intersections)
                            {
                                // only the curve intersection is relevant 
                                tValues.Add(intersection.ParameterA);
                            }
                        }
                    }
                    else
                    {
                        // no intersections found between plane and curve. assign closest curve end/start t value from origin instead
                        errorMessage.Add("Made Plane Adjustments");
                        var closestPoint = Rhino.Collections.Point3dList.ClosestPointInList(new List<Point3d>() { curve.PointAtStart, curve.PointAtEnd }, aPlane.Origin);
                        if (closestPoint == curve.PointAtStart)
                            tValues.Add(curve.Domain.T0); // startpoint -> domain start
                        else
                            tValues.Add(curve.Domain.T1); // endpoint -> domain end
                    }
                }
                else if (goo is GH_Curve)
                {
                    // add intersection point with the curve 
                    var cutterCurve = (goo as GH_Curve).Value;

                    if (!curve.IsClosed || !cutterCurve.IsClosed)
                    {
                        // TODO error messages 
                        return false;
                    }

                    cutCurves.Add(cutterCurve);

                    // per intersection

                    // TODO WHAT IF NO INTERSECTIONS

                    var intersections = Intersection.CurveCurve(curve, cutterCurve, SD.IntersectTolerance, SD.OverlapTolerance);
                    foreach (var intersection in intersections)
                    {
                        if (intersection.IsOverlap)
                        {
                            // domain intersection
                            tValues.Add(intersection.OverlapA.Min);
                            tValues.Add(intersection.OverlapA.Max);
                        }
                        else
                        {
                            // a normal point intersection
                            tValues.Add(intersection.ParameterA);
                        }
                    }




                }
                else
                {
                    // Tough luck, the data is beyond repair. We'll set a runtime error
                    errorMessage.Add(string.Format("Data of type {0} could not be converted into type DataNode. Index = {1}", goo.TypeName, i.ToString()));
                    return false;     
                }
            }

            // test 
            var conversionCheck = string.Format("Numbers: {0}. Planes: {1}. Points: {2}. Curves: {3}",
                cutNumbers.Count.ToString(), cutPlanes.Count.ToString(), cutPoints.Count.ToString(), cutCurves.Count.ToString());
            errorMessage.Add(conversionCheck);
            if (tValues.Count == 0)
            {
                errorMessage.Add("No intersections.");
            }

            // ------------------------------------------------------------------------- step 2: cut the curves

            // apply setting: ensure subdivision by clipping t values
            if (clipSubdivision)
            {
                // dit gaat ervanuit dat de lowerbound en upperbound ongeveer 100 * margin ver uit elkaar liggen

                // get bounds
                var margin = ensureSubdivisionMargin;
                var tLowerbound = curve.Domain.Min + margin;
                var tUpperbound = curve.Domain.Max - margin;

                // check for lowerbound problems 
                for (int i = 0; i < tValues.Count; i++)
                {
                    if (tValues[i] < tLowerbound)
                    {
                        tValues[i] = tLowerbound;
                        tLowerbound = tValues[i] + margin;
                    }
                }

                // check for upperbound problems 
                for (int i = tValues.Count - 1; i >= 0; i -= 1)
                {
                    // count from back to front 
                    if (tValues[i] > tUpperbound)
                    {
                        tValues[i] = tUpperbound;
                        tUpperbound = tValues[i] - margin;
                    }
                }
            }

            // get splitting points, just as an export product
            foreach (var tValue in tValues)
            {
                splitPoints.Add(curve.PointAt(tValue));
            }

            // make subcurves
            subCurves = curve.Split(tValues);

            // success
            return true;
        }

        #region menu

        /// <summary>
        /// fill the Menu
        /// </summary>
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            // new part
            Menu_AppendSeparator(menu);

            // select a node to base this node upon. Nodes with a base will automaticly be connected 
            Menu_Appen[menu, "Choose Mode:", null, false];
            for (int i = 0; i < ModeNames.Count; i++)
            {
                // for every mode, add an entry which can be selected 
                var nickname = ModeNames[i];
                bool selected = i == Mode;
                var item = Menu_Appen[menu, nickname, MenuSetMode, true, selected];

                // at the "click" event, i want to know which item is selected
                item.Name = i.ToString();
            }

            // end new part 
            Menu_AppendSeparator(menu);
            base.AppendAdditionalMenuItems(menu);
        }

        // gets activated by one of the selected items 
        private void MenuSetMode(object sender, EventArgs e)
        {
            // try to get the name of the item clicked.
            var item = sender as ToolStripMenuItem;
            int i = -1;
            int.TryParse(item.Name, out i);
            if (i == -1) Message = "ERROR: SET MODE -1";

            // set the mode to this new index
            Mode = i;

            // update 
            ExpireSolution(true);
        }
        #endregion



        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.CutCurve;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("359b021b-ebd3-42a4-aa96-33b87348f5a4"); }
        }
    }
}