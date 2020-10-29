using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SferedApi
{ 
    internal static class GH_ComponentFunctions
    {
        public static List<string> log = new List<string>();

        public static void Print(this GH_Component component, string message)
        {
            log.Add(message);
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, message);
        }
        public static void Warn(this GH_Component component, string message)
        {
            log.Add(message);
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
        }
        public static void Error(this GH_Component component, string message)
        {
            log.Add(message);
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
        }
    }
}
