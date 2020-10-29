using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using System.Windows.Forms;
using SferedApi.Datatypes;
using System.Linq;

namespace SferedApi
{
    
    // Written by David Rutten, edited by Jos Feenstra
    public class GH_Param_DataNode : GH_PersistentParam<IGH_Goo>
    {
        public static string name = "Data Node";
        public static string nickname = "N";
        public static string description = "A way of transporting Data Node objects";

        public GH_Param_DataNode()
                : base( SD.Starter + name,
                        nickname,
                        SD.CopyRight + description,
                        SD.PluginTitle,
                        SD.PluginCategory1)
        {
            
        }


        #region properties
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }
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
                g.FillEllipse(Brushes.Red, 4, 4, 16, 16);
                g.FillEllipse(Brushes.HotPink, 7, 7, 5, 5);
                g.DrawEllipse(Pens.Maroon, 3, 3, 18, 18);
                g.DrawEllipse(Pens.Maroon, 4, 4, 16, 16);

                // finally
                g.Dispose();
                return icon;
            }
        }

        // oeps misscien moet ik dit ding opnieuw aanmaken
        public override Guid ComponentGuid
        {
            get { return new Guid("{DAF87443-CB0E-40B4-A0DD-1854364547DD}"); }
        }

        public override string TypeName
        {
            get {   return "DataNode"; }
        }


        #endregion

        /// <summary>
        /// Since IGH_Goo is an interface rather than a class, we HAVE to override this method. 
        /// For IGH_Goo parameters it's usually good to return a blank GH_ObjectWrapper.
        /// </summary>
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
                    if (goo is GH_DataNode)
                    {
                        // try to change nickname if applicable.
                        // NOTE: this should actually happen in a "on parameters change" event, but i cant see how to access such a thing
                        // if (Sources.Count >= 1)
                        //     NickName = Sources[0].NickName;
                        continue;
                    }
        
                    //Tough luck, the data is beyond repair. We'll set a runtime error and insert a null.
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        string.Format("Data of type {0} could not be converted into type DataNode", goo.TypeName));
                    branch[i] = null;

                    //As a side-note, we are not using the CastTo methods here on goo. If goo is of some unknown 3rd party type
                    //which knows how to convert itself into a curve then this parameter will not work with that. 
                    //If you want to know how to do this, ask.
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

        // serialization 


        // --------------------------------------------------------------------------- MENU
        
        // variables needed for menu functionality
        List<GH_Param_BaseDataNode> PossibleBaseNodes;
        GH_Param_BaseDataNode BaseNode;

        // get datanodes with base boollean activated
        private List<GH_Param_BaseDataNode> MenuGetBaseNodes()
        {
            // find all sliders
            var baseNodes = new List<GH_Param_BaseDataNode>();
            foreach (IGH_DocumentObject docObject in OnPingDocument().Objects)
            {
                // try to convert it to a DataNode 
                var node = docObject as GH_Param_BaseDataNode;
                if (node != null)
                {
                    baseNodes.Add(node);
                }
            }

            // sort it by nickname
            return baseNodes.OrderBy(x => x.NickName).ToList();
        }

        /// <summary>
        /// fill the Menu
        /// </summary>
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            PossibleBaseNodes = MenuGetBaseNodes();

            // new part
            Menu_AppendSeparator(menu);

            // select a node to base this node upon. Nodes with a base will automaticly be connected 
            Menu_Appen[menu, "Choose Base:", null, false];
            for (int i = -1; i < PossibleBaseNodes.Count; i++)
            {
                // add special first item
                var nickname = "";
                if (i == -1)
                {
                    nickname = "None";
                }
                else
                    nickname = PossibleBaseNodes[i].NickName;

                bool selected = ((i == -1 && BaseNode == null) || (i > -1 && BaseNode == PossibleBaseNodes[i]));
                var item = Menu_Appen[menu, nickname, MenuSetBase, true, selected];
                item.Name = i.ToString();
            }
            Menu_AppendSeparator(menu);

            // end new part 
            Menu_AppendSeparator(menu);
            base.AppendAdditionalMenuItems(menu);
        }

        private void MenuSetBase(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            int i = -1;
            int.TryParse(item.Name, out i);
            try
            {
                // set the new selection
                if (i >= 0)
                {
                    BaseNode = PossibleBaseNodes[i];
                    rewire();
                }
                else
                {
                    BaseNode = null;
                    rewire();
                }       
            }
            catch
            {
                // give errror and unset selected id 
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "something went wrong with getting the index" + item.Name);
                BaseNode = null;
                rewire();
            }
            
        }

        private GH_DataNode GetValue()
        {

            // now try to flip the underlying thing 
            for (int p = 0; p < m_data.PathCount; p++)
            {
                List<IGH_Goo> branch = m_data.Branches[p];
                for (int i = 0; i < branch.Count; i++)
                {
                    IGH_Goo goo = branch[i];
                    if (goo is Datatypes.GH_DataNode)
                    {
                        var goodie = (Datatypes.GH_DataNode)goo;
                        return goodie;
                    }
                }
            }
            // failure 
            return null;
        }

        // activated by using the custom menu
        private void rewire()
        {
            // remove all sources, and if a basenode is specified, link this object to that basenode. 
            RemoveAllSources();
            if (BaseNode != null)
            {
                AddSource(BaseNode);
                WireDisplay = GH_ParamWireDisplay.hidden;
                NickName = BaseNode.NickName;
            }
            ExpireSolution(true);
        }
    }
}