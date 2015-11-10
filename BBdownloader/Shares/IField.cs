using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBdownloader.Shares
{
    public interface IField
    {
        string FieldName { get; set; }
        string FieldNickName { get; set; }
        List<string[]> Overrides { get; set; }
        string Type { get; set; }
        List<string> Transform { get; set; }
        string requestType { get; set; }
        string periodicitySelection { get; set; }
    }

}
