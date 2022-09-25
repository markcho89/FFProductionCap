namespace ProductionCap
{
    internal class ResourceCap
    {
        public string Name { get; set; }
        public int Amount { get; set; }

        public ResourceCap()
        {
            this.Name = "";
            this.Amount = int.MaxValue;
        }

        public ResourceCap(string Name, int Amount)
        {
            this.Name = Name;
            this.Amount = Amount;
        }
    }
}