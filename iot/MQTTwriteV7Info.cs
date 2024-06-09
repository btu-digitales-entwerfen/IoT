using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace iot
{
    public class MQTTwriteV7Info : GH_AssemblyInfo
    {
        public override string Name => "MQTTwriteV7";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("DA5C4D38-BC18-411C-851B-6AF131C7F9AA");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}