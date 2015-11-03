using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBdownloader.Extension_Methods
{
    public static class ExtensionMethods
    {
         public static List<dynamic> RemoveDates(this IEnumerable<SortedList<DateTime, dynamic>> inList)
         {
             var outList = new List<dynamic>();

             foreach (var l in inList)
             {
                 if (l != null)
                     outList.Add(l.ElementAt(0).Value);                
             }
             return outList;
         }

        public static SortedList<DateTime, dynamic>  price2ret(this SortedList<DateTime, dynamic> inList)
        {
            if (inList.Count < 2)
                return inList;

            var prices = new dynamic[inList.Count];
            prices = inList.Values.ToArray();

            var returns = new float[inList.Count];

            returns[0] = 0;

            int i;

            for (i = 1; i < returns.Length; i++)
            {
                returns[i] = prices[i] / prices[i - 1] - 1;
            }

            var outList = new SortedList<DateTime, dynamic>();           

            i = -1;
            foreach (var kvp in inList)
            {
                i++;
                outList.Add(kvp.Key, returns[i]);
            }

            return outList;
        }

        public static SortedList<DateTime, dynamic> ret2price(this SortedList<DateTime, dynamic> inList, dynamic lastPrice)
        {
            if (inList.Count < 2)
                return inList;

            var returns = new dynamic[inList.Count];
            returns = inList.Values.ToArray();

            var prices = new float[inList.Count];

            prices[prices.Length - 1] = lastPrice;

            int i;

            for (i = prices.Length - 2; i > 0; i--)
            {
                prices[i] = prices[i + 1] / (1 + returns[i + 1]);
            }

            var outList = new SortedList<DateTime, dynamic>();

            i = -1;
            foreach (var kvp in inList)
            {
                i++;
                outList.Add(kvp.Key, prices[i]);
            }

            return outList;
        }
        /// <summary>
        /// merge 2 series 
        /// if some dates coincide, prefer 2nd by default ( prefer = 1 )
        /// </summary>
        /// <returns></returns>
        public static SortedList<DateTime, dynamic> merge(this SortedList<DateTime, dynamic> first, SortedList<DateTime, dynamic> second, int prefer = 1)
        {
            SortedList<DateTime, dynamic> primary;
            SortedList<DateTime, dynamic> secondary;
            SortedList<DateTime, dynamic> outList;

            if (prefer == 1)
            {
                primary = second;
                secondary = first;
            }
            else
            {
                primary = first;
                secondary = second;
            }

            if (primary!=null)
                outList = new SortedList<DateTime, dynamic>(primary);
            else
                outList = new SortedList<DateTime, dynamic>();

            foreach (var kvp in secondary)
            {
                if (!outList.ContainsKey(kvp.Key))
                {
                    outList.Add(kvp.Key, kvp.Value);
                }
            }

            return outList;
        }

    }
}
