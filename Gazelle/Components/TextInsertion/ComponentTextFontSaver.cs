using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SferedApi.Components.TextInsertion
{
    public class ComponentTextFontSaver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComponentTextFontSaver class.
        /// </summary>
        public ComponentTextFontSaver()
          : base(   SD.Starter + "Font Saver",
                    SD.Starter + "Font",
                    SD.CopyRight + "Load a font into a format which can be used for insertion",
                    SD.PluginTitle,
                    SD.PluginCategory5)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Characters", "Ch", "Character which represents the curves. WARNING, TAKE APART THE LETTERS, DONT GIVE STRING", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddPlaneParameter("Plane", "P", "Plane on which the curves are drawn.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Width of curves.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height of curves.", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Serialized Data", "S", " Letter data is formatted in a way SferedApi components can understand ", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // standard values of param
            string character = "";
            List<Curve> curvelist = new List<Curve>();
            Plane plane = Plane.WorldXY;
            double width = 2.4;
            double height = 2.4;

            // INPUT 
            DA.GetData(0, ref character);
            DA.GetDataList(1, curvelist);
            DA.GetData(2, ref plane);
            DA.GetData(3, ref width);
            DA.GetData(4, ref height);

            // PROCESS
            char Character = character[0];

            // round width and height stuff. Take Ceilling. 
            double RoundWidth = Math.Ceiling(width * 10) / 10;
            double RoundHeight = Math.Ceiling(height * 10) / 10;
            var charObject = new FontCustomCharacter(Character, curvelist, plane, RoundWidth, RoundHeight);

            // turn item into list 
            DA.SetDataList(0, charObject.GetDataList());

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("70c85707-a291-49fc-97d7-274be3215e24"); }
        }
    }
}