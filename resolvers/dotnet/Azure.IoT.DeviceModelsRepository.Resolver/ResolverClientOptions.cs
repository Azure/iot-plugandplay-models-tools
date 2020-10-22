namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    public class ResolverClientOptions
    {
        public ResolverClientOptions()
        {
            DependencyResolution = DependencyResolutionOption.Enabled;
        }

        public ResolverClientOptions(DependencyResolutionOption resolutionOption)
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
