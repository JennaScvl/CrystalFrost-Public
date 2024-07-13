using Microsoft.Extensions.Options;

namespace CrystalFrost.Config
{
	public interface ILoginUriProvider
	{
		string GetLoginUri();
	}

	public class LoginUriProvider : ILoginUriProvider
	{

		public string GetLoginUri()
		{
            // TODO: add the grid stuff
            // For the moment, just use the value that was set in the config
            var gridConfig = Services.GetService<IOptions<GridConfig>>().Value;
            return gridConfig.LoginURI;
		}
	}
}
