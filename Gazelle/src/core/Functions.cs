using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;

namespace Gazelle
{
    class Helpers
    {
        // DONT JUST DELETE THIS: it explains how to use jsons 
        private bool ApplyMapToCurves(JArray jArray, string key, Brep brep, Vector3d vector, double tolerance)
        {

            foreach (JArray sublist in jArray)
            {
                // try to fill this curve with various approaches
                // Curve curve;
                if (sublist.Type == JTokenType.Integer)
                {
                    // single int found 

                }
                else if (sublist.Type == JTokenType.Array)
                {
                    // list found 
                    foreach (int subitem in sublist)
                    {
                        //var str += " , " + subitem.ToString();
                    }

                }
                else
                {
                    // conversion failed
                    // AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " json couldnt be read.");
                    return false;
                }
            }

            // FAILED
            return false;
        }

        /// <summary>
        /// turn a [1,2,3,4] list into a [[1,2] [2,3] [3,4]] list of lists
        /// </summary>
        public static List<List<T>> Pairify<T>(List<T> list)
        {
            var pairs = new List<List<T>>();
            if (list.Count < 2) return null; // cannot pairify if there are no pairs
            for (int i = 0; i < list.Count - 1; i++)
            {
                pairs.Add(new List<T>() { list[i], list[i + 1] });
            }

            return pairs;
        }

        /// <summary>
        /// convert json string into a full fletched dictionary
        /// </summary>
        /// <param name="json"> the json string </param>
        /// <returns> a filled dictionary </returns>
        public static Dictionary<string, string> JsonToDict(string json)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            return dict;
        }

        /// <summary>
        /// This is used to make holes on a brep with multiple faces. All cutter curves must be on surface, use projection operations
        /// </summary>
        public static bool PerforateBrep(List<Curve> cutters, Brep brep, double Tolerance, out List<Brep> leftover, out List<Brep> holes, out List<object> test)
        {
            // allocate outlists
            leftover = new List<Brep>();
            holes = new List<Brep>();
            var couldBeBoths = new List<Brep>();
            test = new List<object>();

            // test if cutters are indeed closed
            foreach (Curve cutter in cutters)
            {
                if (cutter.IsClosed == false)
                {
                    test.Add("one of the curves is not closed!");
                    return false;
                }
            }

            // per face of the brep
            for (int face_i = 0; face_i < brep.Faces.Count; face_i++)
            {
                // get the face 
                BrepFace face = brep.Faces[face_i];
                var debugSuffix = "Face " + face_i.ToString() + ": ";

                // get partial outlists
                var partialLeftover = new List<Brep>();
                var partialHoles = new List<Brep>();

                // get all types of edges out of the brep
                List<BrepEdge> intEdges = new List<BrepEdge>();
                List<BrepEdge> extEdges = new List<BrepEdge>();
                foreach (int faceindex in face.AdjacentEdges())
                {
                    // get edge, and check its topography
                    var edge = brep.Edges[faceindex];
                    switch (edge.Valence)
                    {
                        case EdgeAdjacency.Interior:
                            intEdges.Add(edge);
                            break;
                        case EdgeAdjacency.Naked:
                            extEdges.Add(edge);
                            break;
                        case EdgeAdjacency.None:
                        case EdgeAdjacency.NonManifold:
                            test.Add(debugSuffix + "None or nonmanifold edges detected.");
                            return false;
                    }
                }

                /* now, per cutter curve: 
                 *     if it crosses the edges of this face:
                 *         > trim it to a new closest curve on the surface
                 *         > add this trim to the personal cut list of this face 
                 *     else if this curve is on the face:
                 *         > add this curve to the personal cut list of this face
                */
                List<Curve> partialCutters = new List<Curve>();
                foreach (Curve cutter in cutters)
                {
                    // get intersection events 
                    var allEvents = new List<IntersectionEvent>();
                    foreach (var interiorEdge in intEdges)
                    {
                        CurveIntersections events = Intersection.CurveCurve(cutter, interiorEdge, SD.IntersectTolerance, SD.OverlapTolerance);
                        if (events != null && events.Count > 0)
                        {
                            allEvents.AddRange(events);
                        }
                    }
                    foreach (var exteriorEdge in extEdges)
                    {
                        CurveIntersections events = Intersection.CurveCurve(cutter, exteriorEdge, SD.IntersectTolerance, SD.OverlapTolerance);
                        if (events != null && events.Count > 0)
                        {
                            test.Add(debugSuffix + "intersection with naked edge!!");
                            allEvents.AddRange(events);
                        }
                    }

                    // if an intersection is found with this cutter 
                    if (allEvents.Count > 0)
                    {
                        // test if there really are two intersections
                        if (allEvents.Count % 2 != 0)
                        {
                            test.Add(debugSuffix + "ERROR: An intersection with a closed curve should give two intersections, found " + allEvents.Count.ToString() + ", perforation cannot continue.");
                            return false;
                        }

                        // for now, just add the entire cutter. The overlap problems can more easely be solved within the single surface cut process
                        partialCutters.Add(cutter);

                    }
                    else
                    {
                        // no cuts found with this curve. 

                        // still add the cutter if its fully on the surface 
                        if (IsPointOnSurface(cutter.PointAtStart, face, SD.Tolerance))
                            partialCutters.Add(cutter);
                    }
                }

                // if no partialcutters are found, dont do the perforate action 
                var faceAsBrep = face.ToBrep();
                if (partialCutters.Count < 1)
                {
                    test.Add(debugSuffix + "no cutters found");
                    
                    // cant be sure if this part is leftover, or fully a hole. 
                    couldBeBoths.Add(faceAsBrep);
                    continue;

                }
                else
                {
                    // test
                    test.Add(debugSuffix + "number of cutter curves: " + partialCutters.Count.ToString());

                }
                // prepare for the perforation action
                var partialTestGeo = new List<object>();
                bool s = PerforateSurface(partialCutters, faceAsBrep, Tolerance, out partialLeftover, out partialHoles, out partialTestGeo);

                // use to perforate. 
                leftover.AddRange(partialLeftover);
                holes.AddRange(partialHoles);
                test.AddRange(partialTestGeo);
            }

            // assign the "could be both" to either leftover or holes 
            foreach(Brep aBrep in couldBeBoths)
            {
                // TODO 

                // FOR NOW JUST ADD IT 
                leftover.Add(aBrep);


                foreach(var edge in aBrep.Edges)
                {
                    // test with leftover edges 

                }
            }


            // VERZIN NOG IETS SLIM OM ZO MIN MOGELIJK AAN DE ORGINELE BREP TE VERANDEREN, EN ALLEEN MAAR DE SPECIFIEKE DELEN MET EEN INSERT TE VERVAGEN!! 

            // success
            return true;
        }

        private static bool IsPointOnSurface(Point3d point, BrepFace surface, double tolerance)
        {
            // if distance from startpoint cutter to the face is close to zero, we also include curve 
            surface.ClosestPoint(point, out double u, out double v);
            var testPoint = surface.PointAt(u, v);
            var pointFaceRelation = surface.IsPointOnFace(u, v);   // use this for potential debugging
            var distance = point.DistanceTo(testPoint);
            if (distance < tolerance)
            {
                return true;
            }
            return false;
        }



        /// <summary>
        /// This is used to make holes on a brep with a single face. All cutter curves must be on surface, use projection operations
        /// </summary>
        public static bool PerforateSurface(List<Curve> curves, Brep brep, double Tolerance, out List<Brep> leftover, out List<Brep> holes, out List<object> test)
        {
            // process
            leftover = new List<Brep>();
            holes = new List<Brep>();

            // test
            test = new List<object>();

            // -------- PHASE 1 --------

            // get this face's outerloop, and other data
            var face = brep.Faces.FirstOrDefault();
            var trim = Curve.JoinCurves(face.OuterLoop.Trims).FirstOrDefault();

            // get pulled curves for comparrison 
            var pulledCurves = new List<Curve>();
            foreach (Curve curve in curves)
            {
                var pulledCurve = face.Pullback(curve, Tolerance);
                if (pulledCurve == null)
                    test.Add("CANT PULL CURVE");
                else    
                    pulledCurves.Add(pulledCurve);
            }

            // get a point on the surface 
            // int MAX_TRIES = 100;
            // for(int i = 0; i < MAX_TRIES; I++)
            // {
            //     TEST OF HET PUNT NIET OP PULLED CURVES ZIT, ANDERS DOE EEN RANDOM 0.5 IMPROVEMENT 
            // }
            var PointInTrimCenter = new Point3d(face.Domain(0).ParameterAt(0.5),
                                    face.Domain(1).ParameterAt(0.5),
                                    0);

            // -------- PHASE 2 --------

            // the split itself 
            Brep splitted = face.Split(curves, Tolerance);
            
            // -------- PHASE 3 --------

            // now we need to determine what is positive space, and what is negative space. we use a mapper to determine this. True means positive, false means negative space 
            List<bool> spaceMapper = new List<bool>();
            foreach(BrepFace splittedFace in splitted.Faces)
            {
                spaceMapper.Add(false);
            }

            // -------- PHASE 4 --------

            // get the surface on which the center point is lying
            int chosenID = -1;
            foreach (BrepFace splittedFace in splitted.Faces)
            {
                var pfr = splittedFace.IsPointOnFace(PointInTrimCenter.X, PointInTrimCenter.Y);
                if (pfr == PointFaceRelation.Boundary)
                {
                    // weve got a problem
                    test.Add("MIDDLE POINT IS ON EDGE OF (PROBABLY) TWO SURFACES");
                    // adding more precision to ispointonface or pulledcurve.contains 
                }

                if (pfr == PointFaceRelation.Interior)
                {
                    chosenID = splittedFace.FaceIndex;
                }
            }
            // test some more 
            if (chosenID == -1)
                test.Add("point is not on any surface");

            // test
            // test.Add(splitted.Faces[chosenID].DuplicateFace(false));
            test.Add(PointInTrimCenter);
            // -------- PHASE 5 --------

            // if it is a hole / negative space , original bool should be false 
            // else, if its leftover geometry / positive space, bool should be true 

            // find out if this point is on positive or negative space. Start out true, (if point is in no cutter, this part is not cut and thus positive geometry)
            bool originalBool = true;

            foreach (Curve pulledCurve in pulledCurves)
            {
                // point containment test
                var pc = pulledCurve.Contains(PointInTrimCenter);
                switch (pc)
                {
                    case PointContainment.Unset:
                        // TODO some error
                        test.Add("PC point is unset!!");
                        break;
                    case PointContainment.Coincident:
                        // TODO some other error
                        test.Add("POINT IS ON THE EDGE OF ANOTHER POINT at the pulled curve operation");
                        break;
                    case PointContainment.Inside:
                        // flip the switch 
                        originalBool = !originalBool;
                        break;

                }

                // TEST 
                test.Add(pulledCurve);
                test.Add(pc.ToString());
            }

            // -------- PHASE 6 --------

            // go through all faces, and flip them from positive to negative space  

            //       PHASE 2   PHASE 3      PHASE 5       PHASE 4
            MapSpace(splitted, spaceMapper, originalBool, chosenID);

            // apply the map
            for (int i = 0; i < splitted.Faces.Count; i++)
            {
                var thisFace = splitted.Faces[i];
                if (spaceMapper[i])
                    leftover.Add(thisFace.DuplicateFace(false));
                else
                    holes.Add(thisFace.DuplicateFace(false));
                
            }

            // success
            return true;
        }

        private static void MapSpace(Brep splitted, List<bool> spaceMapper, bool thisFaceShouldBe, int thisIndex = 0, List<int> alreadyMapped = null)
        {
            // RECURSIVE 
            if (alreadyMapped == null)
            {
                alreadyMapped = new List<int>();
            }

            // stop recursion 
            if (thisIndex >= splitted.Faces.Count)
                return;

            // get this face
            var thisFace = splitted.Faces[thisIndex];
            spaceMapper[thisIndex] = thisFaceShouldBe;
            alreadyMapped.Add(thisIndex);

            foreach (int adjacentFaceIndex in thisFace.AdjacentFaces())
            {
                // if this face is not already mapped, map it to the opposite value of my value
                if (!alreadyMapped.Contains(adjacentFaceIndex))
                    MapSpace(splitted, spaceMapper, !thisFaceShouldBe, adjacentFaceIndex, alreadyMapped);
            }

            // success 
            return;
        }

        private static void PreProcessTrims(Curve trim, List<Curve> curves, out List<Curve> outLeftovers, out List<Curve> outHoles)
        {
            // POSSITVE AND NEGATIVE SPACE 

            // this method 

            // allocate output lists
            var curvesAtCenter = new List<Curve>();
            var curvesOnEdge = new List<Curve>();

            // 1. get all intersections 
            bool IsStartPointTrimWithinACutter = false;
            var allEvents = new List<IntersectionEvent>();
            foreach (Curve curve in curves)
            {
                RegionContainment relation = Curve.PlanarClosedCurveRelationship(trim, curve, Plane.WorldXY, SD.CurveRelationshipTolerance);
                switch(relation)
                {
                    case RegionContainment.BInsideA:

                        // directly add those to the first list
                        curvesAtCenter.Add(curve);
                        break;
                    case RegionContainment.MutualIntersection:

                        // THIS IS REALLY IMPORTANT LATER DOWN THE LINE | if point is within 2 cutters, it means space has become positive again 
                        var test = curve.Contains(trim.PointAtStart);
                        if (test == PointContainment.Inside || test == PointContainment.Coincident)
                            IsStartPointTrimWithinACutter = !IsStartPointTrimWithinACutter;

                        // curves intersect. Cut the curve with the trim 
                        var curveTrims = CutCurveWithCurves(curve, new List<Curve>() { trim });
                        var curveTrimsInside = new List<Curve>();
                        
                        // cull the curves outside of the trim
                        foreach(var curveTrim in curveTrims)
                        {
                            var containment = trim.Contains(curveTrim.PointAtNormalizedLength(0.5));
                            if (containment == PointContainment.Inside)
                                curveTrimsInside.Add(curveTrim);
                        }
                        var curveTrimsInsideJoined = Curve.JoinCurves(curveTrimsInside);

                        curvesOnEdge.AddRange(curveTrimsInsideJoined);
                        break;
                    case RegionContainment.Disjoint:

                        // TODO | Error 

                        break;
                    case RegionContainment.AInsideB:

                        // TODO | Error this case should not happen 

                        break;
                }
            }

            // 2. split the trim with these parts
            var splitedTrimCurves = CutCurveWithCurves(trim, curves);

            // 3. get the even / odd indexed curves & join
            int nth; int wrongNth;
            if (IsStartPointTrimWithinACutter)  { nth = 0; wrongNth = 1; }
            else                                { nth = 1; wrongNth = 0; }

            var subselection = splitedTrimCurves.Where((x, i) => i % 2 == nth);
            var subselectionOther = splitedTrimCurves.Where((x, i) => i % 2 == wrongNth);

            var HolesEdgesOnly = new List<Curve>();
            var LeftoversEdgesOnly = new List<Curve>();

            HolesEdgesOnly.AddRange(curvesOnEdge);
            HolesEdgesOnly.AddRange(subselection);
            LeftoversEdgesOnly.AddRange(curvesOnEdge);
            LeftoversEdgesOnly.AddRange(subselectionOther);

            var HolesIncomplete = Curve.JoinCurves(HolesEdgesOnly);
            var LeftoverIncomplete = Curve.JoinCurves(HolesEdgesOnly);

            // 4. now that all cutter curves are known, find out positive negative spacing
            // curvesAtCenter;


            outHoles = new List<Curve>();
            outLeftovers = new List<Curve>();


            outHoles.AddRange(HolesIncomplete);
            // outLeftovers.AddRange(LeftoverIncomplete);
        }

        // test needed for perforate 
        private static bool CurveMatch(BrepTrimList testCurves, List<Curve> controlCurves)
        {
            foreach (var controlCurve in controlCurves)
            {
                // check all trims
                bool doAllTrimsMatch = false;
                foreach (var testCurve in testCurves)
                {
                    // test for null
                    if (controlCurve == null || testCurve == null)
                    {
                        doAllTrimsMatch = false;
                        break;
                    }

                    

                    // this if statement could be expanded, but it would make it slower
                    if (controlCurve.Contains(testCurve.PointAtStart) == PointContainment.Coincident &&
                        controlCurve.Contains(testCurve.PointAtEnd) == PointContainment.Coincident)
                    {
                        // this trim matches
                        doAllTrimsMatch = true;
                    }
                    else
                    {
                        // this trim does not match
                        doAllTrimsMatch = false;
                        break;
                    }
                }

                // if all trims match, doAllTrimsMatch is true;
                if (doAllTrimsMatch)
                    return true; // match
                else
                    continue;    // no match here
            }

            // no match anywere
            return false;
        }

        // sorter helper function
        private int chooseLargestPoint(Point3d point1, Point3d point2)
        {
            // defaults if one or two are invalid 
            if (point1 == null || point2 == null)
                return 0;
            else if (point1 == null)
                return 1;
            else if (point2 == null)
                return -1;

            // both points are valid. Sort based upon X
            if (point1.X > point2.X)
                return 1;
            else if (point1.X < point2.X)
                return -1;

            // x values are identical, continue with Y
            if (point1.Y > point2.Y)
                return 1;
            else if (point1.Y < point2.Y)
                return -1;

            // Y values are identical, continue with z
            if (point1.Z > point2.Z)
                return 1;
            else if (point1.Z < point2.Z)
                return -1;

            // points are the exact same 
            return 0;
        }

        // curvetrimmer 
        private static Curve[] CutCurveWithCurves(Curve curve, List<Curve> cutterCurves)
        {
            // make intersections
            var GH_Cutters = new List<IGH_Goo>();
            foreach (var cutter in cutterCurves)
            {
                GH_Cutters.Add(new GH_Curve(cutter));
            }
            List<double> tValues;
            List<Point3d> splitPoints;
            Curve[] subCurves;
            List<string> errorMessage;
            bool succes = CurveFunctions.CutCurves(
                curve,
                GH_Cutters,
                new List<GH_Boolean>() {
                    new GH_Boolean(false),
                    new GH_Boolean(false)
                },
                out tValues, out splitPoints, out subCurves, out errorMessage);
            if (!succes)
            {
                // TODO Doe iets als het niet goed gaat
                return null;
            }

            // make smart subcollection
            return subCurves;
        }





        //---------------------------



        // do curves match?
        public enum CurveRelation
        {
            Equal,
            Overlap,
            Unequal

        };

        public CurveRelation CompareCurves(Curve curve1, Curve curve2)
        {
            // this if statement could be expanded, but it would make it slower
            
            
            if (curve1.Contains(curve2.PointAtStart) == PointContainment.Coincident &&
                curve1.Contains(curve2.PointAtEnd) == PointContainment.Coincident)
            {
                // this trim matches
                return CurveRelation.Overlap;
            }
            else
            {
                // this trim does not match
                return CurveRelation.Unequal;
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------------

    // for font system
    class FontCustomCharacter : object
    {
        // field indicators 
        static int CurveListStartIndex = 10;

        // meta data
        public char Character;
        public Plane Plane;
        public double Width;
        public double Height;
        
        // all curves which make up the character in 2d
        public List<Curve> CurveList;

        public FontCustomCharacter(List<IGH_Goo> list)
        {
            // first 10 fields of list is metadata
            Character = (list[0] as GH_String).Value[0];
            Plane     = (list[1] as GH_Plane).Value;
            Width     = (list[2] as GH_Number).Value;
            Height    = (list[3] as GH_Number).Value;

            // starting at 10, its curves
            CurveList = new List<Curve>();

            if (CurveListStartIndex < list.Count)
            {
                for (int i = CurveListStartIndex; i < list.Count; i++)
                {
                    CurveList.Add((list[i] as GH_Curve).Value.DuplicateCurve());
                }
            }
        }

        public FontCustomCharacter Duplicate()
        {
            return new FontCustomCharacter(GetDataList());
        }

        public string print()
        {

            return Character.ToString() + Plane.ToString() + Width.ToString() + Height.ToString() + CurveList.ToString();
        }

        // BEUN deserialization 
        public FontCustomCharacter(List<object> list)
        {
            // first 10 fields of list is metadata
            Character = (list[0] as string)[0];       // list item is string, so get the first item
            Plane = (Plane) list[1];
            Width = (double) list[2];
            Height =(double) list[3];

            // starting at 10, its curves
            CurveList = new List<Curve>();
            for (int i = CurveListStartIndex; i < list.Count; i++)
            {
                CurveList.Add((list[i] as Curve).DuplicateCurve());
            }
        }

        public FontCustomCharacter(char character, List<Curve> curveList, Plane plane, double width, double height)
        {
            Character = character;
            Plane = plane;
            Width = width;
            Height = height;
            CurveList = curveList;
        }

        // this is a BEUN way of serializing data 
        public List<object> GetDataList()
        {
            // fill list with my data
            var list = new List<object>()
            {
                Character.ToString(),
                Plane,
                Width,
                Height,

                // placeholders 
                null,
                null,
                null,
                null,
                null,
                null
            };

            // starting at 10, its curves
            foreach (var curve in CurveList)
            {
                list.Add(curve);
            }

            return list;

        }

    }

    // ----------------------------------------------------------------------------------------------------------
    // 28 - 10 - 2018 : helpers for nudge operations: carefully moving things around so geometry might be closed


    
    class Nudger
    {
        // public int CurrentStep;
        public double StepDistance;
        public Vector3d XDir;
        public Vector3d YDir;
        public Point3d Origin;

        enum Side
        {
            Top,
            Right,
            Bottom,
            Left
        }

        public Nudger(double aStepDistance, Plane aPlane)
        {
            StepDistance = aStepDistance;
            XDir = aPlane.XAxis;
            YDir = aPlane.YAxis;
            Origin = aPlane.Origin;
        }

        public Vector3d GetStep(int FinalStep)
        {
            // data to keep track of rectangle movement 
            int rectangle = 0;
            //int nextRectangle = 0;
            Side side = Side.Right;
            int sideLength = 0;
            int sideStep = 1;        

            int x = 0;
            int y = 0;
            for (int step = 0; step < FinalStep; step++)
            {
                // check which side we're on, and add accordingly 
                if (side == Side.Top)
                    x += 1;
                if (side == Side.Left)
                    y += -1;
                if (side == Side.Bottom)
                    x += -1;
                if (side == Side.Right)
                    y += 1;

                // finaly, cycle trough sides
                sideStep++;
                if (sideStep >= sideLength)
                {
                    if (side == Side.Top)
                    {
                        sideStep = 0;
                        side = Side.Left;
                    }
                    else if (side == Side.Left)
                    {
                        sideStep = 0;
                        side = Side.Bottom;
                    }
                    else if (side == Side.Bottom)
                    {
                        sideStep = 0;
                        side = Side.Right;
                    }
                    else if (side == Side.Right)
                    {
                        if (sideStep == sideLength) continue; // do it one more time

                        rectangle += 8;             // next rec perimeter has 8 more tiles 
                                                    //nextRectangle = step + rectangle;
                        sideLength = rectangle / 4; // perimeter divided by four 
                        sideStep = 1;               // first step is set by moving up
                        side = Side.Top;
                    }
                }
            }
            // turn x and y change to something representable 
            Point3d point = new Point3d(Origin);
            Vector3d xVector = XDir * x * StepDistance;
            Vector3d yVector = YDir * y * StepDistance;
            Vector3d MoveVector = xVector + yVector;
            return MoveVector;
        }
    }
}
