using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Shared.Models
{
    public class Catch
    {
        public Guid Id { get; set; }

        public int CatchNumber { get; set; }

        public string UserName { get; set; }

        public string Fish { get; set; }
        
        public double Length { get; set; }
        
        public DateTime CatchDate { get; set; }

        public DateTime LogDate { get; set; }
        
        public DateTime EditDate { get; set; }

        public int GlobalCatchNumber { get; set; }

    } // end c
} // end ns
