namespace Azure.DigitalTwins.Resolver
{
    public class FetchResult
    {
        public string Definition { get; set; }
        public string Path { get; set; }
        public bool PreCalculated
        {
            get { return Path.EndsWith("expanded.json"); }
        }
    }
}