// Generated by .NET Reflector from D:\Sfered\Gazelle\Remnants\SferedApi_brep_manipulation.dll
namespace Gazelle
{
    using Grasshopper.Kernel;
    using System;
    using System.Collections.Generic;
    
    public static class Debug
    {
        private static List<string> data = new List<string>();
        private static List<object> geometries = new List<object>();
        private static List<ConsolePrinter> listeners = new List<ConsolePrinter>();
        
        private static void ActivateListeners()
        {
            foreach (ConsolePrinter printer in listeners)
            {
                if (!printer.IsExpired)
                {
                    printer.OnSolutionExpired(true);
                    printer.IsExpired = true;
                }
            }
        }
        
        public static void Flush()
        {
            data = new List<string>();
            geometries = new List<object>();
        }
        
        public static void Geo(object geo)
        {
            geometries.Add(geo);
        }
        
        public static List<object> GetAllGeometry() => 
            geometries;
        
        public static List<string> GetAllStrings() => 
            data;
        
        public static void Log(string message)
        {
            data.Add("--- : " + message);
        }
        
        public static void Log(GH_Component sender, string message)
        {
            data.Add(sender.Name + " : " + message);
        }
        
        public static void SetListener(ConsolePrinter listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        public static void Update()
        {
            ActivateListeners();
        }
    }
}
