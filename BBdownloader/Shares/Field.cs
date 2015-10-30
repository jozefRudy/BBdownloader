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
        public List<string[]> Overrides { get; set; }     
        public string Type { get; set; }
        public List<string> Transform { get; set; }
        public string requestType { get; set; }
        
        public Field()
        {
            Overrides = new List<string[]>();
            Transform = new List<string>();
        }

        public override string ToString()
        {
            string overrides = String.Empty;
            
            foreach (var item in Overrides)
	        {
		        overrides += item[0] + ":" + item[1] + ", ";
	        }
            if (Overrides.Count>0)
                overrides = overrides.Remove(overrides.Length - 2);
            return FieldNickName + ", " + overrides;
        }
    }
}
