using System;
using System.Collections.Generic;

namespace BBdownloader.Shares
{
    public interface IField : IComparable<IField>
    {
        string FieldName { get; set; }
        string FieldNickName { get; set; }
        SortedDictionary<string,string> Overrides { get; set; }
        string Type { get; set; }
        List<string> Transform { get; set; }
        string requestType { get; set; }
    }

}
