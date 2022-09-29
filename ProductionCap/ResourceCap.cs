namespace ProductionCap
{
    internal class ResourceCap
    {
        public string Name { get; set; }
        public int StopAt { get; set; }
        public int ResumeAt { get; set; }

        public ResourceCap()
        {
            this.Name = "";
            this.StopAt = int.MaxValue;
            this.ResumeAt = 1;
        }

        public ResourceCap(string Name, int StopAt, int ResumeAt)
        {
            this.Name = Name;
            this.StopAt = StopAt;
            this.ResumeAt = ResumeAt < 1 ? 1 : ResumeAt;
            this.ResumeAt = ResumeAt > StopAt ? StopAt : ResumeAt;
        }
    }
}