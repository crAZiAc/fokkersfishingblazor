using System;
using System.Text.Json.Serialization;

namespace FokkersFishing.Api.Models.Dtos
{
    public class Catch
    {
        private DateTime m_catchDate;

        public Guid Id { get; set; }
        public Guid CompetitionId { get; set; }
        public int CatchNumber { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string RegisterUserEmail { get; set; }
        public string RegisterUserName { get; set; }
        public string Fish { get; set; }
        public double Length { get; set; }

        public DateTime CatchDate
        {
            get => m_catchDate.ToLocalTime();
            set => m_catchDate = value;
        }

        // Non-persistent helper property
        public string TeamName { get; set; }
        public DateTime LogDate { get; set; }
        public DateTime EditDate { get; set; }
        public int GlobalCatchNumber { get; set; }
        public string MeasurePhotoUrl { get; set; }
        public string CatchPhotoUrl { get; set; }
        public string MeasureThumbnailUrl { get; set; }
        public string CatchThumbnailUrl { get; set; }
        public CatchStatusEnum Status { get; set; }

        [JsonIgnore]
        public bool CaughtInCompetition =>
            this.CompetitionId != Guid.Parse("00000000-0000-0000-0000-000000000000");
    }
}
