using OpenMetaverse;
using Vector3 = UnityEngine.Vector3;
using OMVVector3 = OpenMetaverse.Vector3;
using CrystalFrost.Extensions;

namespace Bunny
{
	public static class HUDHelper
	{
		public static bool IsHUD(Primitive prim)
		{
			return prim.IsAttachment
				&& prim.PrimData.AttachmentPoint.IsHudAttachmentPoint();
		}

		public static Vector3 GetHUDPosition(Primitive prim)
		{
			//return new Vector3(6, prim.Position.Y, prim.Position.Z);
			return GetHUDPosition(prim.Position);
			//return new Vector3(6f, 6f, 6f);
		}
		public static Vector3 GetHUDPosition(OMVVector3 vec)
		{
			//return new Vector3(6, prim.Position.Y, prim.Position.Z);
			return new Vector3(vec.X, vec.Z, vec.Y);
			//return new Vector3(6f, 6f, 6f);
		}
	}
}
