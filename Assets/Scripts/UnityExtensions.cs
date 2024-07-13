using OpenMetaverse.Imaging;
using OpenMetaverse.ImportExport.Collada14;
using Unity.VisualScripting;
using UnityEngine;
using static OpenMetaverse.Imaging.ManagedImage;

namespace UnityEngine.Extensions
{
	public static class Extensions
	{

		public static OpenMetaverse.Quaternion ToLMV(this Quaternion q1)
		{
			return new OpenMetaverse.Quaternion(-q1.x, -q1.z, -q1.y, q1.w);
		}

		// Extension method for Matrix4x4 to convert from right-handed to left-handed coordinate system
		public static Matrix4x4 ConvertRHtoLH(this Matrix4x4 rightHandedMatrix)
		{
			// Inverting the Z-axis (scale and translation)
			Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));
			Matrix4x4 convertedMatrix = scaleMatrix * rightHandedMatrix * scaleMatrix;

			// Adjusting the rotation part of the matrix
			convertedMatrix.m02 = -convertedMatrix.m02;
			convertedMatrix.m12 = -convertedMatrix.m12;
			convertedMatrix.m20 = -convertedMatrix.m20;
			convertedMatrix.m21 = -convertedMatrix.m21;

			return convertedMatrix;
		}

		// Convert an array of Matrix4x4 from right-handed to left-handed coordinate system
		public static Matrix4x4[] ConvertRHtoLH(this Matrix4x4[] rightHandedMatrices)
		{
			Matrix4x4[] convertedMatrices = new Matrix4x4[rightHandedMatrices.Length];
			for (int i = 0; i < rightHandedMatrices.Length; i++)
			{
				convertedMatrices[i] = rightHandedMatrices[i].ConvertRHtoLH();
			}
			return convertedMatrices;
		}

		// Extension method to convert Vector3 from right-handed to left-handed coordinate system
		public static Vector3 ConvertRHtoLH(this Vector3 rightHandedVector)
		{
			// For a basic conversion between right-handed and left-handed coordinate systems,
			// where the primary difference is the direction of the Z-axis, you simply negate the Z component.
			// If your conversion rules are different, please adjust the code accordingly.
			return new Vector3(rightHandedVector.x, rightHandedVector.z, rightHandedVector.y);
		}

		public static Quaternion ConvertRHtoLH(this Quaternion q1)
		{
			return new Quaternion(-q1.x, -q1.z, -q1.y, q1.w);
		}



	}
}