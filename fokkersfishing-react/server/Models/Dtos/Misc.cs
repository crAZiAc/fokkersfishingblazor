using System;
using System.Text.Json.Serialization;

namespace FokkersFishing.Api.Models.Dtos
{
    public class Fish
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string GenericName { get; set; }
        public string Kind { get; set; }
        public bool Predator { get; set; }
        public bool IncludeInCompetition { get; set; }
    }

    public class FisherMan
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public double TotalLength { get; set; }
        public int FishCount { get; set; }
        public string UserEmail { get; set; }
    }

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
                if (Pike != null) l += Pike.Length;
                if (Bass != null) l += Bass.Length;
                if (Zander != null) l += Zander.Length;
                return l;
            }
        }
    }

    public class BigThreeWinner
    {
        public string Name { get; set; }
        public string Fish { get; set; }
        public double TotalLength { get; set; }
    }

    public class Ranking
    {
        public string TeamName { get; set; }
        public int Rank { get; set; }
        public double Score { get; set; }
        public bool Big3 { get; set; }
    }

    public class TeamScore
    {
        public string Fish { get; set; }
        public double TotalLength { get; set; }
        public int FishCount { get; set; }
        public string TeamName { get; set; }
        public int Ranking { get; set; }
    }

    public class CompetitionStats
    {
        public int FishCaught { get; set; }
        public double TotalLength { get; set; }
    }

    public class IndividualScore
    {
        public string Fish { get; set; }
        public CatchStatusEnum Status { get; set; }
        public string FisherMan { get; set; }
        public double Length { get; set; }
    }

    public class Photo
    {
        public Guid Id { get; set; }
        public byte[] ImageContent { get; set; }
        public PhotoTypeEnum PhotoType { get; set; }
    }

    public class UploadPhotoResponse
    {
        public string PhotoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class AddUserToRole
    {
        public string RoleName { get; set; }
        public string UserEmail { get; set; }
    }

    public class UpdatePasswordRequest
    {
        public string UserEmail { get; set; }
        public string NewPassword { get; set; }
    }
}
