using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBdownloader.Extension_Methods;

namespace BBdownloader.Shares
{
    public class Field: IField, IComparable<IField>
    {
        public string FieldNickName { get; set; }
        public string FieldName { get; set; }                
        public SortedDictionary<string, string> Overrides { get; set; }     
        public string Type { get; set; }
        public List<string> Transform { get; set; }
        public string requestType { get; set; }
        public string periodicitySelection { get; set; }
        
        public Field()
        {
            Overrides = new SortedDictionary<string,string>();
            Transform = new List<string>();
        }

        public override string ToString()
        {
            string overrides = String.Empty;
            
            foreach (var item in Overrides)
	        {
		        overrides += item.Key + ":" + item.Value + ", ";
	        }
            if (Overrides.Count>0)
                overrides = overrides.Remove(overrides.Length - 2);
            return FieldNickName + ", " + overrides;
        }
      
        public int CompareTo(IField secondField)
        {
            if (String.Compare(this.requestType, secondField.requestType) == 1)
            {               
                if (this.Overrides.Compare(secondField.Overrides) == 0)                   
                {
                    return this.periodicitySelection.CompareTo(secondField.periodicitySelection);
                }
                return this.Overrides.Compare(secondField.Overrides);
            }

            return this.requestType.CompareTo(secondField.requestType);            
        }

    }
}
