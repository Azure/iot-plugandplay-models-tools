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
        /// <summary>
        /// Do not process external dependencies
        /// </summary>
        Disabled,
        /// <summary>
        /// Enable external dependencies
        /// </summary>
        Enabled,
        /// <summary>
        /// Try to get external dependencies using .expand.json
        /// </summary>
        FromExpanded
    }
}
