using System;
using System.Collections.Generic;
using System.Text;

namespace FokkersFishing.Shared.Models
{
    public class BigThree
    {
        public string Name { get; set; }
        public Catch Pike { get; set; }
        public Catch Bass { get; set; }
        public Catch Zander { get; set; }

        public double TotalLength
        {
            get
            {
                double l = 0;
                if (Pike != null)
                {
                    l += Pike.Length;
                }
                if (Bass != null)
                {
                    l += Bass.Length;
                }
                if (Zander != null)
                {
                    l += Zander.Length;
                }
                return l;
            }
        }
    } // end c
} // end ns
