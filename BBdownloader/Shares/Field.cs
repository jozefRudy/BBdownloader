using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBdownloader.Shares
{
    public class Field: IField
    {
        public string FieldNickName { get; set; }
        public string FieldName { get; set; }
        public List<string> Overrides { get; set; }
        
    }
}
