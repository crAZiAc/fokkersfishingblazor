using System;
using FokkersFishing.Api.Models.Dtos;

namespace FokkersFishing.Api.Models.Entities
{
    public class CatchData : BaseEntity
    {
        public int CatchNumber { get; set; }
        public Guid CompetitionId { get; set; }
        public string UserEmail { get; set; }
        public string RegisterUserEmail { get; set; }
        public string Fish { get; set; }
        public double Length { get; set; }
        public DateTime CatchDate { get; set; }
        public DateTime LogDate { get; set; }
        public DateTime EditDate { get; set; }
        public int GlobalCatchNumber { get; set; }
        public string MeasurePhotoUrl { get; set; }
        public string CatchPhotoUrl { get; set; }
        public string MeasureThumbnailUrl { get; set; }
        public string CatchThumbnailUrl { get; set; }
        public CatchStatusEnum Status { get; set; }

        public CatchData()
        {
            PartitionKey = "Catches";
        }

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
                UserEmail = this.UserEmail,
                RegisterUserEmail = this.RegisterUserEmail,
                CatchPhotoUrl = this.CatchPhotoUrl,
                MeasurePhotoUrl = this.MeasurePhotoUrl,
                CatchThumbnailUrl = this.CatchThumbnailUrl,
                MeasureThumbnailUrl = this.MeasureThumbnailUrl,
                CompetitionId = this.CompetitionId,
                Status = this.Status
            };
        }
    }
}
