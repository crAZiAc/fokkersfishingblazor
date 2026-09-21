namespace FokkersFishing.Api.Models.Dtos
{
    public enum CatchStatusEnum
    {
        Approved = 1,
        Pending = 2,
        Rejected = 3
    }

    public enum PhotoTypeEnum
    {
        Catch,
        Measure
    }

    public static class Constants
    {
        public const int REQUIRED_FISH_LENGTH = 1;
        public const string NO_BIG3_COLOR = "#ffc107";
        public const string BIG3_COLOR = "green";
        public const int NUMBER_OF_BIG3 = 3;
    }
}
