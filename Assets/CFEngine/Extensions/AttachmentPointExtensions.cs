using OpenMetaverse;

namespace CrystalFrost.Extensions
{
	public static class AttachmentPointExtensions
	{
		public static bool IsHudAttachmentPoint(this AttachmentPoint attachementPoint)
		{
			return attachementPoint >= AttachmentPoint.HUDCenter2
				&& attachementPoint <= AttachmentPoint.HUDBottomRight;
		}
	}
}
