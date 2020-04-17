using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebLancerHelper.WebLancer.Objects
{
    class Category
    {
        static public List<Category> Categories = new List<Category> { };
        public string Name { get; set; }
        public string Href { get; set; }

    }
}
