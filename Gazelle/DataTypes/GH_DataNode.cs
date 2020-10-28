
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SferedApi.Datatypes
{
    // link between the GH_Goo and DataNode 
    class GH_DataNode : GH_Goo<DataNode>
    {
        // base -> makes sense in GH_Param_DataNode
        public bool IsBase;

        // empty Constructor
        public GH_DataNode()
        {
            Value = new DataNode();
            IsBase = false;
        }

        // constructor with DataNode 
        public GH_DataNode(DataNode dataNode)
        {
            Value = new DataNode(dataNode);
            IsBase = false;
        }

        // constructor with Dict 
        public GH_DataNode(Dictionary<string, object> dataDict)
        {
            Value = new DataNode(dataDict);
            IsBase = false;
        }

        // constructor with copy 
        public GH_DataNode(GH_DataNode dataNode)
        {
            Value = new DataNode(dataNode.Value);
            IsBase = dataNode.IsBase;
        }

        // ----------------------------------------------------------------------- Formatters

        public override bool IsValid
        {
            get { return true; }
        }

        public override string TypeName
        {
            get { return "DataNode Object"; }
        }

        public override string TypeDescription
        {
            get { return SD.CopyRight + "A dictionary like collection of Key - Value pairs."; }
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_DataNode(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        // --------------------------------------------------------------------- serialize

        // Serialize this instance to a Grasshopper writer object
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // data of type text could not be converted to type datanode
            writer.SetBoolean("isBase", IsBase);
            // writer.SetString("DataNode", Value.GetJson());
            return base.Write(writer);
        }

        // Deserialize this instance from a Grasshopper reader object.
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // string json = reader.GetString("DataNode");
            // Value = new DataNode();
            // Value.SetJson(json);
            IsBase = reader.GetBoolean("isBase");
            return base.Read(reader); ;
        }
    }
}

