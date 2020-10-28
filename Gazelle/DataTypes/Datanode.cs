using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Grasshopper.Kernel.Types;

namespace SferedApi.Datatypes
{
    // NOTE: not to be confused with the GH_DataNode class
    class DataNode : Object
    {
        // 5 different ways of accessing the same data: as a dynamic object, as a ExpandoOjbect, as a Dictionary, as a flat EXpandoObject and as a Flat Dictionary
        public dynamic o;
        public ExpandoObject O { get { return o; } }
        public IDictionary<string, object> Dict
        {
            get => (IDictionary<String, Object>)o;
            set => o = value;
        }

        // the flat items relink the dynamic items in such a way that the DataNode objects in between the ExpandoObjects are filtered out. Python understands this, as well as Json 
        private dynamic Flat_o;
        private ExpandoObject Flat_O { get { return Flat_o; } }
        private IDictionary<string, object> FlatDict
        {
            get => (IDictionary<String, Object>)Flat_o;
        }

        // additional data 

        // --------------------------------------------------------------------- Constructors 

        public DataNode()
        {
             o = new ExpandoObject();
        }
        public DataNode(Dictionary<string, object> dict)
        {
            SetDict(dict);
        }
        public DataNode(DataNode dataNode)
        {
            // omdat iets gek werkt met dictionaries overnemen, dan maar deze omweg 
            dynamic o2 = new ExpandoObject();
            foreach (var kvp in (IDictionary<string, object>)dataNode.o)
            {
                ((IDictionary<string, object>)o2).Add(kvp);
            }
            o = o2;
        }

        // ------------------------------------------------------------ Basic Functionalities

        /// <summary>
        /// set the dynamic object of this node by specifying a dictionary
        /// 
        /// STRUCTURE:
        /// DataNode -- .o --> (ExpandoObject) --.key--> (GH_DataNode) --Value--> (DataNode) --.o--> .....
        /// 
        /// </summary>
        public void SetDict(Dictionary<string, object> dict)
        {
            o = new ExpandoObject();
            foreach (var item in dict)
            {
                // test type, make new one if dict
                if (item.Value is Dictionary<string, object>)
                {
                    var newNode = new GH_DataNode((Dictionary<string, object>)item.Value);
                    Dict.Add(item.Key, newNode);
                }
                else
                {
                    Dict.Add(item.Key, item.Value);
                }  
            }
        }

        public ExpandoObject GetFlatObject()
        {
            if (Flat_o != null)
                return Flat_O;
            Flat_o = new ExpandoObject();
            
            foreach(var item in Dict)
            {
                // test type, make new one if dict
                if (item.Value is GH_DataNode)
                {
                    // add expandobject found at that GH_DataNode : WARNING : VALUE.VALUE IS CORRECT 
                    var thatnode = (GH_DataNode)item.Value;
                    FlatDict.Add(item.Key, thatnode.Value.GetFlatObject()); 
                }
                else
                {
                    FlatDict.Add(item.Key, item.Value);
                }
            }
            
            return Flat_O;
        }


        public IDictionary<string, object> GetFlatDict()
        {
            if (FlatDict != null)
                GetFlatObject();
            return FlatDict;
        }

        /// <summary>
        /// try to get the ExpandoObject for implementation in things such as python 
        /// </summary>
        public object TryGetDataObject()
        {
            return O;
        }
        /// <summary>
        /// 
        /// </summary>
        internal bool HasKey(string key)
        {
            return Dict.ContainsKey(key);
        }
        /// <summary>
        /// 
        /// </summary>
        internal object Get(string key)
        {
            return Dict[key];
        }

        internal void Add(string key, object value)
        {
            if (Dict.ContainsKey(key))
                Dict[key] = value;
            else
                Dict.Add(key, value);
        }

        // ------------------------------------------------------------------------ converters 

        private string jsonStart = "{";
        private string jsonEnd   = "}";
        private char itemEnd   = ',';
        private string tab       = "  ";
        private string newLine   = Environment.NewLine;

        internal string GetJson(string indent="")
        {
            // json string to fill
            string json = "";
            json += jsonStart;

            // loop trough your items 
            int i = 1;
            foreach (var item in Dict)
            {
                string newindent = indent + tab;           

                // choose how the data should be converted
                string value = ""; 
                if (item.Value is GH_DataNode)
                {
                    var thatnode = (GH_DataNode)item.Value;
                    value = thatnode.Value.GetJson(newindent);
                }
                else if (item.Value is string)
                    value = "\"" + (string)item.Value + "\"";
                else if (item.Value is GH_String)
                {
                    GH_String temp = (GH_String)item.Value;
                    value = "\"" + temp.Value + "\"";
                }
                else
                    value = "\"" + "CANNOT JSONIFY THIS OBJECT" + "\"";

                // write a new line in the json 
                json += newLine + newindent + "\"" + item.Key + "\": " + value;

                // write comma, just not at the end 
                if (i >= Dict.Count)
                    break;

                i++;
                json += itemEnd;
            }
            // end the json (part)
            json += newLine + indent + jsonEnd;
            return json;
        }

        internal Dictionary<string, object> SetJson(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    json, new JsonConverter[] { new MyConverter() });

                SetDict(obj);

                // can be used to test
                return obj;
            }
            catch
            {
                return null;
            }

        }
        // trying to get a way to print error messages 
        public void popup(string message)
            {
            }
        }

    // im a script kiddie, blindly copy pasting stuff from StackOverflow
    class MyConverter : CustomCreationConverter<IDictionary<string, object>>
    {
        public override IDictionary<string, object> Create(Type objectType)
        {
            return new Dictionary<string, object>();
        }

        public override bool CanConvert(Type objectType)
        {
            // in addition to handling IDictionary<string, object>
            // we want to handle the deserialization of dict value
            // which is of type object
            return objectType == typeof(object) || base.CanConvert(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject
                || reader.TokenType == JsonToken.Null)
                return base.ReadJson(reader, objectType, existingValue, serializer);

            // if the next token is not an object
            // then fall back on standard deserializer (strings, numbers etc.)
            return serializer.Deserialize(reader);
        }
    }
}