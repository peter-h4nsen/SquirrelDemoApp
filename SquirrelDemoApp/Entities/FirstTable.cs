using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDemoApp.Entities
{
    public sealed class FirstTable
    {
        private FirstTable() { }

        public FirstTable(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public int ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
    }
}
