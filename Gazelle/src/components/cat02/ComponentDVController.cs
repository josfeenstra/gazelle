using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Gazelle.Datatypes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


/*  Sfered Api: Node Contoller
 *  Functions:  Save and Load stringified jsons
 *              Store this locally as persistent data
 *              Choose a Json
 *              Meant for the DV jsons, but can be used for other things            
 *              
 *  TODO        - Synchronise automaticly 
 *              - Manage paths correctly. 
 * 
 * 
 */



namespace Gazelle
{
    public class ComponentDVController : GH_Component
    {
        // GUI
        // private AttributesButtonGeneral Button1;

        // mode
        public int Mode;
        public List<string> ModeNames;

        /// <summary>
        /// Initializes a new instance of the ComponentDVController class.
        /// </summary>
        public ComponentDVController()
          : base(SD.Starter + "Node DV Controller",
                    SD.Starter + "Controller",
                    SD.CopyRight + "Syncs the files at folder with locally stored jsons." +
                    "Ds = design selector(no need for multiple jsons converted to Node objects)" +
                    "Fp = Folder Path.Save and load jsons from this location. if none are given, use current directory / filename / as folder" +
                    "Fn = File Names.Files with these names are selected. if none are given, all files in Folder Path are used." +
                    "M = Memory.Stores the loaded jsons as Persistent Data.Dont connect it if you want to use the Persistent Data.",
                    SD.PluginTitle,
                    SD.PluginCategory2)
        {
            // default mode is dormant
            Mode = 0;
            ModeNames = new List<string>() { "Dormant", "Save", "Load" };
            UpdateMessage();
        }

        // update message (after changing mode for example)
        public void UpdateMessage(string additionalText = "")
        {
            Message = "Mode: " + ModeNames[Mode] +
                      "\n" + additionalText;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // ---
            var DES0 = "Ds = design selector. Selects the index of the wanted json. \n(no need for multiple jsons converted to Node objects)";
            var DES2 = "Fn = File Names. Files with these names are selected. \nif none are given, all files in Folder Path are used.";
            var DES3 = "M = Memory. Stores the loaded jsons as Persistent Data. \nDont connect it if you want to use the Persistent Data.";

            // ---
            pManager.AddIntegerParameter("Design Selector", "Design Selector", DES0, GH_ParamAccess.item, 0);
            pManager.AddTextParameter("File Paths", "File Paths", DES2, GH_ParamAccess.list);
            pManager.AddTextParameter("Data", "Data", DES3, GH_ParamAccess.list);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // single node as output
            pManager.AddGenericParameter("Node", "N", "Data Node", GH_ParamAccess.item);

            // after saving, get paths 
            pManager.AddGenericParameter("Paths", "P", "Paths, relevant in save mode", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // update message
            UpdateMessage();
            
            // get storage parameter
            var storage = Params.Input[2];
            GH_PersistentParam<GH_String> storageParam = storage as GH_PersistentParam<GH_String>;

            // Mode | save
            if (Mode == 1) 
            {
                // try to load volatile data 
                var saveData = new List<GH_String>();
                DA.GetDataList(2, saveData);
                if (saveData.Count == 0)
                {
                    // try to load persistent data 
                    if (storageParam.PersistentData.IsEmpty) return;
                    saveData = storageParam.PersistentData.Branches[0];
                }

                // create directory path to save all jsons to in
                string dirpath = OnPingDocument().FilePath;
                if (dirpath == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Save Document First!");
                    UpdateMessage("Save Document First!");
                    return;
                }
                dirpath = dirpath.Replace(".gh", "");  // remove .gh

                // if no name can be found, make a placeholder name 
                string NO_NAME = "Unnamed_";

                // per GH_String in data to save
                var paths = new List<string>();
                for (int i = 0; i < saveData.Count; i++)
                {
                    // construct a path out of the data in the json 
                    var gh_string = saveData[i];
                    var json = gh_string.Value;
                    string filename;

                    try
                    {
                        // try to create a nice name from the json 
                        var obj = JObject.Parse(json);
                        var manu = (string)obj["about"]["manufacturer_short"];
                        var model = (string)obj["about"]["model_name"];
                        var version = (string)obj["about"]["version"];
                        if (manu.Length == 0 || model.Length == 0 || version.Length == 0) throw new Exception();
                        filename = manu + "_" + model + "_v" + version;
                        UpdateMessage("Save Succesful.");
                    }
                    catch 
                    {
                        // cant find name data in json, give default name 
                        UpdateMessage("Default name used.");
                        filename = NO_NAME + i.ToString();
                    }    

                    // with filename, create dir and save  
                    var fullpath = Path.Combine(dirpath, filename + ".json");
                    FileInfo file = new FileInfo(fullpath);
                    file.Directory.Create(); // If the directory already exists, this method does nothing.
                    File.WriteAllText(fullpath, json);
                    paths.Add(fullpath);
                }

                // test
                DA.SetDataList(1, paths);
            }

            // Mode | load
            else if (Mode == 2)
            {
                // input |  
                var paths = new List<string>();
                DA.GetDataList(1, paths);

                storageParam.PersistentData.Clear();

                // Process
                foreach (var path in paths)
                {
                    // read and save 
                    string text = File.ReadAllText(path);
                    storageParam.PersistentData.Append(new GH_String(text));
                }
            }


            // Input 2 | get now relevant data 
            int designSelector = -1;
            DA.GetData(0, ref designSelector);
            if (storageParam.PersistentData.IsEmpty) return;
            var storedData = storageParam.PersistentData.Branches[0];

            // test
            DA.SetDataList(1, storedData);

            // Process 2 | select one of the stored values
            if (designSelector < 0 || designSelector >= storedData.Count) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "DS out of range: " + designSelector.ToString());
                return;
            }
            var selectedString = storedData[designSelector];

            // Output 2 | convert that string to node
            var outNode = new GH_DataNode();
            var response = outNode.Value.SetJson(selectedString.Value);
            if (response == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "couldn't convert dictionary");
                UpdateMessage("Couldn't convert dictionary.");
                return;
            }     
            
            DA.SetData(0, outNode);
        }

        /// <summary>
        /// fill the Menu
        /// </summary>
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            // new part
            Menu_AppendSeparator(menu);

            // select a node to base this node upon. Nodes with a base will automaticly be connected 
            Menu_AppendItem(menu, "Choose Mode:", null, false);
            for (int i = 0; i < ModeNames.Count; i++)
            {
                // for every mode, add an entry which can be selected 
                var nickname = ModeNames[i];
                bool selected = i == Mode;
                var item = Menu_AppendItem(menu, nickname, MenuSetMode, true, selected);

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
            UpdateMessage();
            ExpireSolution(true);
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
                return Properties.Resources.NodeController;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b44736a7-0881-4550-9b5c-2b986c08af50"); }
        }
    }
}