using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SferedApi.Datatypes
{
    public class GH_Param_BaseDataNode : GH_PersistentParam<IGH_Goo>
    {
        public static string name = "Base Data Node";
        public static string nickname = "DN";
        public static string description = "Used for quick access / quick linking";

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_Param_BaseDataNode()
            : base( SD.Starter + name,
                    nickname,
                    SD.CopyRight + description,
                    SD.PluginTitle,
                    SD.PluginCategory2)
        {
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                // david rutten has created this amazing bitmap. im keeping this because i wanna know how he does it
                Bitmap icon = new Bitmap(24, 24, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(icon);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);

                // the drawing
                g.FillEllipse(Brushes.Green, 4, 4, 16, 16);
                g.FillEllipse(Brushes.LightGreen, 7, 7, 5, 5);
                g.DrawEllipse(Pens.Black, 3, 3, 18, 18);
                g.DrawEllipse(Pens.Black, 4, 4, 16, 16);

                // finally 
                g.Dispose();
                return icon;
            }
        }

        public override string TypeName
        {
            get { return "BaseDataNode"; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fc990a08-9797-4727-9283-3852e9e2a744"); }
        }

        #region COPY PASTED FROM DATANODE 

        protected override IGH_Goo InstantiateT()
        {
            return new GH_ObjectWrapper();
        }

        /// <summary>
        /// Since our parameter is of type IGH_Goo, it will accept ALL data. 
        /// We need to remove everything now that is not, GH_Colour, GH_Curve or null.
        /// </summary>
        protected override void OnVolatileDataCollected()
        {
            for (int p = 0; p < m_data.PathCount; p++)
            {
                List<IGH_Goo> branch = m_data.Branches[p];
                for (int i = 0; i < branch.Count; i++)
                {
                    IGH_Goo goo = branch[i];

                    //We accept existing nulls.
                    if (goo == null) continue;

                    //We accept curves.
                    if (goo is Datatypes.GH_DataNode) continue;

                    //Tough luck, the data is beyond repair. We'll set a runtime error and insert a null.
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        string.Format("Data of type {0} could not be converted into type DataNode", goo.TypeName));
                    branch[i] = null;
                }
            }
        }

        protected override GH_GetterResult Prompt_Singular(ref IGH_Goo value)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Plural(ref List<IGH_Goo> values)
        {
            throw new NotImplementedException();
        }

        // NO MENU, WOULD BE VERY RECURSIVE AND WEIRD

        #endregion

    }
}