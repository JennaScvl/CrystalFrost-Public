using OMVVector3 = OpenMetaverse.Vector3;
using UnityVector3 = UnityEngine.Vector3;
using OMVVector2 = OpenMetaverse.Vector2;
using UnityVector2 = UnityEngine.Vector2;
using OMVQuaternion = OpenMetaverse.Quaternion;
using UnityQuaternion = UnityEngine.Quaternion;
using OMVColor4 = OpenMetaverse.Color4;
using UnityColor4 = UnityEngine.Color;

namespace CrystalFrost.Extensions
{
    public static class Extensions
    {
        public static UnityVector3 ToUnity(this OMVVector3 v3)
        {
            return new UnityVector3(v3.X, v3.Z, v3.Y);
        }

        public static OMVVector3 FromUnity(this UnityVector3 v3)
        {
            return new OMVVector3(v3.x, v3.z, v3.y);
        }

        public static UnityVector3 ToVector3(this OMVVector3 v3)
        {
            return v3.ToUnity();
        }

        public static UnityVector2 ToUnity(this OMVVector2 v2)
        {
            return new UnityVector3(v2.X, 1f - v2.Y); ;
        }

        public static UnityVector2 ToVector2(this OMVVector2 v2)
        {
            return v2.ToUnity();
        }

        public static UnityQuaternion ToUnity(this OMVQuaternion q1)
        {
            return new UnityQuaternion(-q1.X, -q1.Z, -q1.Y, q1.W);
        }

        public static UnityColor4 ToUnity(this OMVColor4 c1)
        {
            return new UnityColor4(c1.R, c1.G, c1.B, c1.A);
        }

		public static UnityEngine.Matrix4x4 ToMatrix4x4(this float[] floatArray)
		{
			if (floatArray == null || floatArray.Length != 16)
				return UnityEngine.Matrix4x4.identity;

			return new UnityEngine.Matrix4x4(
					new UnityEngine.Vector4(floatArray[0], floatArray[1], floatArray[2], floatArray[3]),
					new UnityEngine.Vector4(floatArray[4], floatArray[5], floatArray[6], floatArray[7]),
					new UnityEngine.Vector4(floatArray[8], floatArray[9], floatArray[10], floatArray[11]),
					new UnityEngine.Vector4(floatArray[12], floatArray[13], floatArray[14], floatArray[15])
				);
		}

		/// <summary>
		/// Converts the given inverseBindMatrix from an OpenSim-based coordinate system to support Unity's coordinate system.
		/// </summary>
		/// <param name="m">The inverseBindMatrix to be converted.</param>
		/// <returns>The adjusted inverseBindMatrix compatible with Unity's coordinate system.</returns>
		public static UnityEngine.Matrix4x4 InverseBindMatrixArrayHandConversion(this UnityEngine.Matrix4x4 m)
		{
			UnityEngine.Matrix4x4 adjustedInverseBindMatrices = new UnityEngine.Matrix4x4();

			var position = m.GetColumn(3);
			position = new UnityEngine.Vector3(position.x, position.z, position.y);

			var rotation = m.rotation;
			rotation = new UnityEngine.Quaternion(-rotation.x, -rotation.z, -rotation.y, rotation.w);

			var scale = m.lossyScale;
			scale = new UnityEngine.Vector3(scale.x, scale.z, scale.y);

			adjustedInverseBindMatrices.SetTRS(position, rotation, scale);

			return adjustedInverseBindMatrices;
		}
	}
}