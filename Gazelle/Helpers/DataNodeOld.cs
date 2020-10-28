using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using GH = Grasshopper;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;
using System.Dynamic;
using Microsoft.CSharp;

namespace SferedApi.Temp
{
    // The class derived from DynamicObject.
    public class DataNodeOld : DynamicObject
    {


        // The inner dictionary.
        Dictionary<string, object> dictionary;

        public DataNodeOld()
        {
            dictionary = new Dictionary<string, object>();
        }

        public DataNodeOld(Dictionary<string, object> _dictionary)
        {
            dictionary = new Dictionary<string, object>(_dictionary);
        }




        // ---------------------------------------

        public Dictionary<string, object> GiveDict()
        {
            return dictionary;
        } 

        // This property returns the number of elements
        // in the inner dictionary.
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        // If you try to get a value of a property 
        // not defined in the class, this method is called.
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            string name = binder.Name.ToLower();

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return dictionary.TryGetValue(name, out result);
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            dictionary[binder.Name.ToLower()] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }
    }


}