using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace SferedApi.Components.Developer
{
    public class ComponentDevProcessFont : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ComponentDevProcessFont()
          : base(SD.Starter + "FONT text converter",
                    SD.Starter + "",
                    SD.CopyRight + "",
                    SD.PluginTitle,
                    SD.PluginCategory0)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text to convert", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("C", "C", "Curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var text = "";
            DA.GetData(0, ref text);
            var curves = GetTextCurves(text , Plane.WorldXY , "Cleanwork");
            DA.SetDataList(0, curves);
        }


        public Curve[] GetTextCurves(string content, Plane pl, string font = "Cleanwork", double size = 2.4, bool bold = false, bool italics = false)
        {
            // output 
            Curve[] curves = null;

            //TxtLines
            //written by Giulio Piacentino
            //version 2009 06 11
            //Other tools and updates at www.giuliopiacentino.com/grasshopper-tools/

            // make sure stuff is correct
            if (size == 0)
                size = 1;
            if (string.IsNullOrEmpty(font) || size <= 0 || string.IsNullOrEmpty(content) || !pl.IsValid)
            {
                return null;
            }

            // perform operation
            var te = RhinoDoc.ActiveDoc.Objects.AddText(content, pl, size, font, bold, italics);
            Rhino.DocObjects.TextObject txt = RhinoDoc.ActiveDoc.Objects.Find(te) as Rhino.DocObjects.TextObject;

            if (txt != null)
            {
                // output
                var tt = txt.Geometry as TextEntity;
                curves = tt.Explode();
            }

            // close
            RhinoDoc.ActiveDoc.Objects.Delete(te, true);
            return curves;
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
                return Properties.Resources.Image1;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c521e68e-b422-4631-b4e5-336c078a9744"); }
        }
    }
}