using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace SferedApi
{
    public class GazelleInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Gazelle";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.Sfered_Iconified;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return Meta.Version;
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("af491c11-3861-4da4-b98b-abf8e7a7766a");
            }
        }

        public override string Version
        {
            get
            {
                //Return a string representing the version.
                
                return Meta.Version;
            }
        }
        public override string AssemblyVersion
        {
            get
            {
                //Return a string representing the assembly version. 
                return Meta.AssemblyVersion;
            }
        }

        public override string AuthorName
        {
            get
            {
                // Return a string identifying you or your company.
                return Meta.Description;
            }
        }
        public override string AuthorContact
        {
            get
            {
                // Return a string representing your preferred contact details.
                return "www.sfered.nl";
            }
        }
    }
}
