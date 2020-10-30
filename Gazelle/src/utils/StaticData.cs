using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gazelle
{
    // meta data
    class Meta
    {
        // version control (not how it should be)
        private static int MajorVersion = 0;
        private static int MinorVersion = 5;
        private static int BuildNumber = 0;
        private static int Revision = 0;
        public static string Version = MajorVersion +"."+MinorVersion +"."+BuildNumber+"."+Revision;
        public static string AssemblyVersion = Version;

        // data
        public static string Name = "Gazelle";
        public static string Description = "Gazelle: Parametric Product Design Toolbox by Sfered.";
    }

    // Static Data class for often occuring string and things like that 
    class SD
    {
        // info for components
        public static string Starter     = "GZ ";
        public static string CopyRight   = "(c) Sfered. Version" + Meta.Version;
        public static string PluginTitle = Meta.Name;

        // categories for components
        public static string PluginCategoryMin1 = "Developer Highlight";
        public static string PluginCategory0 = "Developer";
        public static string PluginCategory1 = "Node Basics";
        public static string PluginCategory2 = "Node Advanced";
        public static string PluginCategory3 = "Misc.";
        public static string PluginCategory4 = "Geometry";
        public static string PluginCategory5 = "Text";
        public static string PluginCategory6 = "Spline";
        public static string PluginCategory7 = "Brep";
        public static string PluginCategory8 = "Brep Advanced";
        public static string PluginCategory9 = "Brep Utilities";
        public static string PluginCategory11 = "Curve";
        public static string PluginCategory10 = "Experiment";

        public static string PluginCategory6Description = "\nThis Component is part of the 'Spline' components. \nThese components default to a system which uses 4 control points to create a 3rd degree basic spline, in similair fashion to Adobe Illustrator. \nThis greatly speeds up the workflow of parametric product design.";

        // classic data structure keys 
        public static string ClassicKeys0 = "designVec";
        public static string ClassicKeys1 = "glass";
        public static string ClassicKeys2 = "rim";
        public static string ClassicKeys3 = "bridge";
        public static string ClassicKeys4 = "hinge";
        public static string ClassicKeys5 = "temple"; 
        public static List<string> ClassicKeys = new List<string>()
        {
            SD.ClassicKeys0,
            SD.ClassicKeys1,
            SD.ClassicKeys2,
            SD.ClassicKeys3,
            SD.ClassicKeys4,
            SD.ClassicKeys5
        };
        // meta keys of the json (to access the classic data + the classic map)
        public static string JsonMetaKeys0 = "main";
        public static string JsonMetaKeys1 = "mainMap";
        public static string JsonMetaKeys2 = "about";
        public static string JsonMetaKeys3 = "extra";
        public static List<string> JsonMetaKeys = new List<string>()
        {
            SD.JsonMetaKeys0,
            SD.JsonMetaKeys1,
            SD.JsonMetaKeys2,
            SD.JsonMetaKeys3
        };

        // static tolerance values 
        public const double Tolerance = 0.001;
        public const double JoinTolerance = 0.001;
        public const double OverlapTolerance = 0.001;
        public const double IntersectTolerance = 0.001;
        public const double ProjectionTolerance = 0.001;
        public const double CurveRelationshipTolerance = 0.001;
        public const double PointTolerance = 0.001;
    }
}
