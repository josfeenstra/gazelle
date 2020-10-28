﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using SferedApi.Datatypes;

namespace SferedApi.Components.Geo
{
    public class ComponentGeoProjectText : GH_Component
    {
        // starting values 
        public double letterWidth;
        public double letterHeight;
        public double letterDepth;

        // constances 
        public const double ClosestPointPrecision = 10.0;
        public const double ProjectionTolerance = 0.000001;
        public const double PreProjectionHoverHeight = 1;

        // settings 
        public List<int> Settings;
        private string SETTINGS_DESCRIPTION = "Setting 0: Is Text Centered. If set to 1, text will be centered at plane point";


        // Letter Data (oldskool)
        // public JObject letterJsonData;

        // raw letter data / serialized letter data 
        private GH_Structure<IGH_Goo> letterRawData;

        // letter data new 
        private List<FontCustomCharacter> LetterDataList;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentGeoProjectText()
          : base(SD.Starter + "Project Text",
                    SD.Starter + "Project Text",
                    SD.CopyRight + "Project a Text to a brep",
                    SD.PluginTitle,
                    SD.PluginCategory5)
        {

            // default settings 
            Settings = new List<int>()
            {
                // setting 1
                0,

                // setting 2
                0,

                // setting 3
                0
            };
            
            // oldskool
            // letterJsonData = JObject.Parse(Properties.Resources.TextInsertJson);
        }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // main parameters
            pManager.AddTextParameter("Text", "T", "Text to project", GH_ParamAccess.item);
            pManager.AddBrepParameter("Brep", "B", "Brep to project onto", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Plane to project text insertion from ", GH_ParamAccess.item, Plane.WorldXY);

            // letter parameters
            pManager.AddNumberParameter("Width", " ", "Width of entire string(obsolete)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height of Letter", GH_ParamAccess.item, 2.4);
            pManager.AddNumberParameter("Depth", "D", "Depth of Letter", GH_ParamAccess.item, 0.5);
            
            // additional parameters
            pManager.AddIntegerParameter("Options", "O", "Additional settings. " + SETTINGS_DESCRIPTION, GH_ParamAccess.list );

            // temporary parameters
            pManager.AddGenericParameter("Character Data", "C", "temporary, put the alfabet curves in here", GH_ParamAccess.tree);

            // 
            pManager.AddNumberParameter("Nudge Distance", "N", "Nudge Distance", GH_ParamAccess.item, 0.5);
            pManager.AddIntegerParameter("Nudge Maximum tries", "M", "Nudge Maximum tries", GH_ParamAccess.item, 1);



            // make some things optional 
            for (int i =0; i < 10; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry","G","Geometry generated by the insert", GH_ParamAccess.list);
            pManager.AddCurveParameter("Projection curve","C","curves as projected on surface", GH_ParamAccess.list);
            pManager.AddGenericParameter("Debug", "D", "geometry for debugging", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT -------------------------------------------------------------------------------------------------
            string text = "";
            var brep = new Brep();
            var plane = new Plane();
            DA.GetData(0, ref text);
            DA.GetData(1, ref brep);
            DA.GetData(2, ref plane);
            var newSettings = new List<int>();

            // INPUT additional
            DA.GetData(3, ref letterWidth);
            DA.GetData(4, ref letterHeight);
            DA.GetData(5, ref letterDepth);
            DA.GetDataList(6, newSettings);

            // INPUT temporary
            DA.GetDataTree(7, out letterRawData);

            // INPUT debug extraordinary
            double NudgeDistance = 0;
            int NudgeMax = 0;
            DA.GetData(8, ref NudgeDistance);
            DA.GetData(9, ref NudgeMax);

            // test
            Message = "";

            // PROCESS -1 ---------------------------------- load character data (can be optimized)
            LetterDataList = new List<FontCustomCharacter>();
            foreach (var branch in letterRawData.Branches)
            {
                var c = new FontCustomCharacter(branch);
                LetterDataList.Add(c);
            }

            // interprete settings 
            for (int i = 0; i < newSettings.Count && i < Settings.Count; i++)
            {
                Settings[i] = newSettings[i]; 
            }

            // PROCESS 0 ----------------------------------- get correct positions and transformation data 

            // precalculations
            Vector3d projectionVector = plane.ZAxis * -1;   // <---- IS DIT ALTIJD CORRECT? -> meestal
            double fullWidthOfText = 0;
            plane.Translate(projectionVector * PreProjectionHoverHeight);
            var preProjectionCurves = new List<Curve>();
            double scaleFactor = 0;

            // PROCESS 1 ----------------------------------- get character geometry and move to the correct possition

            // per character in text
            for (int i = 0; i < text.Length; i++)
            {
                // get the letter object corresponding to this character  
                char character = text.ToLower().ToCharArray()[i];
                FontCustomCharacter c = null;
                foreach (var letterData in LetterDataList)
                {
                    if (letterData.Character == character)
                    {
                        c = letterData.Duplicate();
                        break;
                    }
                }

                if (c == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Character '" + character.ToString() + "' has no loaded geometry");
                    return;
                }

                // extract data
                List<Curve> charCurves = c.CurveList;
                Point3d charPoint = c.Plane.Origin;
                Plane LetterCurveOriginPlane = c.Plane;

                // each letter needs a different position, make transformation precalculations 
                scaleFactor = letterHeight / c.Height;
                var distanceToMid = fullWidthOfText;
                fullWidthOfText += c.Width;

                // create vectors to correct the position of the curves
                var xMovementToCharPlace = distanceToMid;                      // <-- DIT MOET GEFIXED WORDEN 
                var vector1 = new Vector3d(c.Width / 2, 0, 0);                 // move to the left / right
                var vector2 = new Vector3d(xMovementToCharPlace, 0, 0);        // move to this char's position in word

                // create the transformations necessairy to bring the static letter curves in the correct position, with the correct size
                var planeTransformation = Transform.PlaneToPlane(LetterCurveOriginPlane, plane);

                // dont scale 
                var scaleTransformation = Transform.Scale(LetterCurveOriginPlane, scaleFactor, scaleFactor, 1);

                // place these curves in the correct position for projection
                foreach (var curve in charCurves)
                {
                    curve.Translate(vector1 + vector2);
                    curve.Transform(scaleTransformation);
                    curve.Transform(planeTransformation);
                }

                // add them to tree for test
                foreach (var curve in charCurves)
                {
                    preProjectionCurves.Add(curve);
                }
            }

            // PROCESS 1.5 ------------------------------------------------------------------------------------  bend? 

            // place in center 
            bool centered = Settings[0] == 1;
            if (centered)
            {
                // the width of the entire text. move the curves
                // Point3d point = plane.Origin + boundingbox.Center * -1;

                Vector3d finalmovevec = plane.XAxis * (fullWidthOfText * scaleFactor * -0.5);
                foreach (var curve in preProjectionCurves)
                {
                    curve.Translate(finalmovevec);
                }

            }


            // PROCESS 2 -------------------------------------------------------------------------- project the curves  

            // the projection must be revamped, we cannot do this anymore on a per letter basis. 

            /* pseudo:
            - Project everything
            - intersect with brep edges. 
                - if intersect with naked edges -> quit or try again with smaller letter height
                - 
             
             */

            // NUDGE: 28-10-2018: made PROCESS 2 & 3 repeatable, for Nudge bugfix 
            Nudger nudger = new Nudger(NudgeDistance, plane);

            // variables to extract from tries loop
            Brep[] finalAllBreps = null;
            var finalProjectionCurves = new List<Curve>();
            var finalTest = new List<object>();

            // NUDGE: keep trying, until limmit or valid geometry. 
            for (int tries = 0; tries < NudgeMax; tries ++)
            {
                // PROJECTION
                var projectionCurves = new List<Curve>();
                foreach (var curve in preProjectionCurves)
                {
                    // NUDGE: edit curve 
                    var vec = nudger.GetStep(tries);
                    curve.Translate(vec);

                    // make the actual projection
                    var projectedCrvs = Curve.ProjectToBrep(curve, brep, projectionVector, SD.ProjectionTolerance);

                    Curve goodCurve = null;
                    if (projectedCrvs.Length == 0)
                    {
                        // no curves in list, something went wrong 
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Projection Failed! change tolerance?");
                        return;
                    }
                    else if (projectedCrvs.Length == 1)
                    {
                        // only 1 curve in list, select it 
                        goodCurve = projectedCrvs[0];
                    }
                    else
                    {
                        // multiple projections 
                        Message = "";
                        Message = "multiple projections";

                        // get closest curve 
                        var maxdistance = 100.0;
                        var minDistance = maxdistance;
                        foreach (var projectedCurve in projectedCrvs)
                        {
                            var distance = curve.PointAtStart.DistanceTo(projectedCurve.PointAtStart);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                goodCurve = projectedCurve;
                            }
                        }

                        // if its unassigned 
                        if (goodCurve == null)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "no goodcurve found. projection is weird.");
                            return;
                        }
                    }
                    // we filtered out a good curve 
                    projectionCurves.Add(goodCurve);
                }

                // CUT OPERATION
                var leftover = new List<Brep>();
                var holes = new List<Brep>();
                var test = new List<object>();
                var testMessages = new List<string>();
                bool response;
                if (brep.Faces.Count == 1)
                    response = Helpers.PerforateSurface(projectionCurves, brep, SD.Tolerance, out leftover, out holes, out test);
                else
                    response = Helpers.PerforateBrep(projectionCurves, brep, SD.Tolerance, out leftover, out holes, out test);

                // PROCESS 3 ----------------------------------------------------------------------- section with curves 

                if (!response)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Perforation returned negative.");
                }
                var allBreps = GenerateGeometry(projectionVector, projectionCurves, leftover, holes);

                // PROCESS 4 ------------------------------------------------------------------------ test validity



                // EDIT: cannot test for solid, if the original brep is not solid
                if (tries != NudgeMax - 1) // if this is not the last one 
                {
                    // test brep for correctness
                    if (!FingerprintTest(allBreps, brep, ref test))
                        continue;
                }


                // this is the last one, or the geometry is correct
                if (!FingerprintTest(allBreps, brep, ref test))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Geometry Incorrect. Maximum number of nudges used.");
                    test.Add("Geometry Incorrect. Maximum number of nudges used.");
                }
                else
                {
                    test.Add("Geometry is correct!");
                }

                test.Add("Number of Nudge Tries: " + tries.ToString());
                finalAllBreps = allBreps;
                finalProjectionCurves = projectionCurves;
                finalTest = test;
                break;
            }


            // OUTPUT ------------------------------------------------------------------------------------------------

            DA.SetDataList(0, finalAllBreps);
            DA.SetDataList(1, finalProjectionCurves);
            DA.SetDataList(2, finalTest);

        }

        // return true if the brep list matches the fingerprint. Fingerprint is defined as the naked edges of the brep  
        private bool FingerprintTest(Brep[] breps, Brep fingerprint, ref List<object> test)
        {
            // it needs to be 1 brep, thats for sure 
            if (breps.Length != 1)
            {
                test.Add("Not 1 brep");
                return false;
            }
            var brep = breps[0];

            // if the fingerprint is solid, brep must also be solid
            if (fingerprint.IsSolid)
            {
                if (brep.IsSolid)
                {
                    test.Add("Good: Both Solid");
                    return true;
                }
                else
                {
                    test.Add("Fingerprint Solid, brep is not");
                    return false;
                }
            }

            // if print is not solid, test each curve
            Curve[] fpNakedEdges = fingerprint.DuplicateNakedEdgeCurves(true, true);
            Curve[] brepNakedEdges = brep.DuplicateNakedEdgeCurves(true, true);
            
            if (brepNakedEdges.Length > fpNakedEdges.Length)
            {
                test.Add("Brep has more naked edges than start brep(fingerprint)");
                return false;
            }
            
            if (brepNakedEdges.Length < fpNakedEdges.Length)
            {
                test.Add("somehow, brep has less naked edges than start brep(fingerprint)");
                return false;
            }

            // correct
            test.Add("Good: Same number of naked edges.");
            return true;
        }


        private Brep[] GenerateGeometry(Vector3d projectionVector, List<Curve> projectionCurves, List<Brep> leftover, List<Brep> holes)
        {
            // list with all breps in it 
            var allBreps = new List<Brep>();

            // lower holes 
            var loweringVector = projectionVector * letterDepth;
            foreach (var hole in holes)
            {
                hole.Translate(loweringVector);
            }

            // make walls for holes 
            var holewalls = new List<Brep>();
            foreach (var curve in projectionCurves)
            {
                var lowercurve = curve.DuplicateCurve();
                lowercurve.Translate(loweringVector);
                var lofts = Brep.CreateFromLoft(new List<Curve>() { curve, lowercurve }, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);

                // make sure its 1 loft 
                var loft = lofts[0];

                holewalls.Add(loft);
            }

            // join everything together 
            allBreps.AddRange(holes);
            allBreps.AddRange(holewalls);
            allBreps.AddRange(leftover);

            // this is hopefully a single brep
            var singleBrep = Brep.JoinBreps(allBreps, SD.JoinTolerance);
            return singleBrep;
        }

        #region Local Helpers 

        // with charcurves hovering above the brep, project these curves on the brep, and create the solid letter geometry
        private bool ProjectCharacter(char character, List<Curve> charCurves, Vector3d projectVector, Brep brep, double depth, out List<IGH_Goo> resultGeo, out List<object> testGeo)
        {
            // fill these lists
            var curves1 = new List<Curve>();
            var curves2 = new List<Curve>();
            resultGeo = new List<IGH_Goo>();

            // test
            testGeo = new List<object>();

            // ik wil dat twee aanliggende surfaces dezelfde curve gebruiken. MAAR curves moeten eerst gejoined worden, dan geprojecteerd.


            // STEP 1 - PROJECTION
            foreach (var curve in charCurves)
            {
                // make the actual projection
                var projectedCrvs = Curve.ProjectToBrep(curve, brep, projectVector, ProjectionTolerance);
            }

            // return the geometry list 
            return true;
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
                return Properties.Resources.ProjectText;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("31d8b09a-f539-4ecb-846c-2a90d287e8b8"); }
        }
    }
}