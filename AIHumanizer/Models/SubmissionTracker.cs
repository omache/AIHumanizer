namespace AIHumanizer.Models
{
    public class SubmissionTracker
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public DateTime LastSubmissionTime { get; set; }
        public int SubmissionCount { get; set; }
    }

}
