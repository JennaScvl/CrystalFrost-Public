using OpenMetaverse;

namespace CrystalFrost.Extensions
{
	/// <summary>
	/// Provides extension methods for the <see cref="AttachmentPoint"/> enum.
	/// </summary>
	public static class AttachmentPointExtensions
	{
		/// <summary>
		/// Determines whether the specified attachment point is a HUD attachment point.
		/// </summary>
		/// <param name="attachementPoint">The attachment point to check.</param>
		/// <returns>True if the attachment point is a HUD attachment point; otherwise, false.</returns>
		public static bool IsHudAttachmentPoint(this AttachmentPoint attachementPoint)
		{
			return attachementPoint >= AttachmentPoint.HUDCenter2
				&& attachementPoint <= AttachmentPoint.HUDBottomRight;
		}
	}
}
