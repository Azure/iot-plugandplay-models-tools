using Azure.Core;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    public sealed class ResolverClientOptions: ClientOptions
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
        /// Do not process external dependencies.
        /// </summary>
        Disabled,
        /// <summary>
        /// Enable external dependencies.
        /// </summary>
        Enabled,
        /// <summary>
        /// Try to get external dependencies using .expanded.json.
        /// </summary>
        TryFromExpanded
    }
}
