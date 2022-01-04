using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FokkersFishing.Shared.Models;
using Newtonsoft.Json;

namespace FokkersFishing.Models
{
    public class CatchData : BaseEntity, ITableEntity
    {
        private DateTime m_catchDate;

        public int CatchNumber { get; set; }

        public string UserName { get; set; }

        public string Fish { get; set; }

        public double Length { get; set; }

        public DateTime CatchDate { get; set; }

        public Catch GetCatch()
        {
            return new Catch
            {
                CatchNumber = this.CatchNumber,
                CatchDate = this.CatchDate.ToLocalTime(),
                EditDate = this.EditDate.ToLocalTime(),
                Fish = this.Fish,
                GlobalCatchNumber = this.GlobalCatchNumber,
                Id = Guid.Parse(this.RowKey),
                Length = this.Length,
                LogDate = this.LogDate.ToLocalTime(),
                UserName = this.UserName
            };
        }

        public DateTime LogDate { get; set; }

        public DateTime EditDate { get; set; }

        public int GlobalCatchNumber { get; set; }

        public CatchData()
        {
            PartitionKey = "Catches";
        }
    } // end c
} // end ns
