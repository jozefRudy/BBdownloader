﻿using System;
using System.Collections.Generic;
using System.Linq;
using BBdownloader.Extension_Methods;

namespace BBdownloader.Shares
{
    public class Field: IField
    {
        public string FieldNickName { get; set; }
        public string FieldName { get; set; }                
        public SortedDictionary<string, string> Overrides { get; set; }     
        public string Type { get; set; }
        public List<string> Transform { get; set; }
        public string requestType { get; set; }
        
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
            return FieldNickName + ", " + this.requestType + ", " + overrides;
        }
      
        public int CompareTo(IField secondField)
        {
            if (secondField == null)
                return -1;

            if (String.Compare(this.requestType, secondField.requestType) == 0)
            {          
                if (Enumerable.SequenceEqual(this.Overrides,secondField.Overrides))
                    return 0;
                else
                    return this.Overrides.Compare(secondField.Overrides);
            }
            return this.requestType.CompareTo(secondField.requestType);            
        }

    }
}
