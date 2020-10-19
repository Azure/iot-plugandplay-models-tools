namespace Azure.DigitalTwins.Resolver
{
    public class ResolverClientSettings
    {
        public ResolverClientSettings()
        {
            DependencyResolution = DependencyResolutionOption.Enabled;
        }

        public ResolverClientSettings(DependencyResolutionOption resolutionOption)
        {
            DependencyResolution = resolutionOption;
        }

        public DependencyResolutionOption DependencyResolution { get; }
    }

    public enum DependencyResolutionOption
    {
        Disabled,
        Enabled,
        FromExpanded
    }
}
