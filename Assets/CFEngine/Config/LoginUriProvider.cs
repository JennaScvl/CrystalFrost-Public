using Microsoft.Extensions.Options;

namespace CrystalFrost.Config
{
	/// <summary>
	/// Defines a provider for retrieving the login URI for the grid.
	/// </summary>
	public interface ILoginUriProvider
	{
		/// <summary>
		/// Gets the login URI.
		/// </summary>
		/// <returns>The login URI string.</returns>
		string GetLoginUri();
	}

	/// <summary>
	/// Provides the login URI for the grid by retrieving it from the configuration.
	/// </summary>
	public class LoginUriProvider : ILoginUriProvider
	{

		/// <summary>
		/// Gets the login URI from the application's configuration.
		/// </summary>
		/// <returns>The login URI string.</returns>
		public string GetLoginUri()
		{
            // TODO: add the grid stuff
            // For the moment, just use the value that was set in the config
            var gridConfig = Services.GetService<IOptions<GridConfig>>().Value;
            return gridConfig.LoginURI;
		}
	}
}
