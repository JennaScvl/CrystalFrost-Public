using OpenMetaverse;

namespace Temp
{
	/// <summary>
	/// This will eventually be used to delete objects that are killed
	/// Currently no objects are killed because we're just testing and
	/// trying to get mesh memory use sorted right now.
	/// </summary>
	public class KillObjectEventData
	{
		public object sender;
		public KillObjectEventArgs e;

		public KillObjectEventData(object sender, KillObjectEventArgs e)
		{
			this.sender = sender;
			this.e = e;
		}
	}

}