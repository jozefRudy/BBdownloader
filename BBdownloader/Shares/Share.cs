using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBdownloader.Shares
{
    public class Share
    {
        List<IField> fields { get; set; }
        string name;

        public Share(string name, List<IField> fields)
        {
            this.name = name;
            this.fields = fields;            
        }
    }
}
