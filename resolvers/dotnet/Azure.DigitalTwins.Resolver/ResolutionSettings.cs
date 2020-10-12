namespace Azure.DigitalTwins.Resolver
{
    public class ResolutionSettings
    {
        public ResolutionSettings()
        {
            CalculateDependencies = true;
            UsePreCalculatedDependencies = false;
        }

        public ResolutionSettings(bool calculateDependencies, bool usePreComputedDependencies)
        {
            CalculateDependencies = calculateDependencies;
            UsePreCalculatedDependencies = usePreComputedDependencies;
        }

        public bool CalculateDependencies { get; }
        public bool UsePreCalculatedDependencies { get; }
    }
}
