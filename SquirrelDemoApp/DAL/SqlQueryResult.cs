using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquirrelDemoApp.DAL
{
    public sealed class SqlQueryResult<TField1>
    {
        public TField1 Field1 { get; set; }
    }
}
