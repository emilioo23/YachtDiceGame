using Microsoft.Extensions.DependencyInjection;
using YachtDice.Client.Logic.Audio;
using YachtDice.Utils;

namespace YachtDice.Composition
{
    /// <summary>
    /// Registers the shared components of the client in the dependency injection container. They live while the
    /// application is open and are only used from the thread of the windows.
    /// </summary>
    internal static class ClientServiceRegistration
    {
        /// <summary>
        /// Registers the components that the windows share.
        /// </summary>
        /// <param name="services">The collection of the container.</param>
        /// <returns>The same collection, with the components registered.</returns>
        public static IServiceCollection AddClientComponents(this IServiceCollection services)
        {
            services.AddSingleton<AudioSettings>();
            services.AddSingleton<MusicPlayer>();
            services.AddSingleton<ClientServices>();

            return services;
        }
    }
}
