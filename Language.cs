using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecountercli
{
    internal class Language
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string[] Extensions { get; set; } = Array.Empty<string>();
    }
}
