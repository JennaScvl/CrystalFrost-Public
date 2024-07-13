using CrystalFrost.Assets.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalFrost.Assets.Animation
{
	public static class AnimationExtensions
	{
		private static string GetRelativePath(Transform root, Dictionary<string, Transform> bones, string targetJoint)
		{
			var path = targetJoint;

			while (targetJoint != root.name)
			{
				targetJoint = bones[targetJoint].parent.name;
				path = $"{targetJoint}/{path}";
			}

			return path;
		}

		public static AnimationClip ToUnityAnimationClip(this DecodedAnimation animation, Transform root, Dictionary<string, Transform> bones)
		{
			var clip = new AnimationClip();

			clip.legacy = true;
			clip.name = animation.Header.EmoteName;
			clip.wrapMode = WrapMode.Loop;

			var jointData = animation.JointData;
			
			for (int i = 0; i < jointData.Length; i++)
			{
				var joint = jointData[i];

				if ( !bones.ContainsKey(joint.JointName) )
				{
					Debug.LogWarning($"Joint: {joint.JointName} not found in bones dictionary");
					continue;
				}

				var positionCurveX = new AnimationCurve();
				var positionCurveY = new AnimationCurve();
				var positionCurveZ = new AnimationCurve();
				var rotationCurveX = new AnimationCurve();
				var rotationCurveY = new AnimationCurve();
				var rotationCurveZ = new AnimationCurve();
				var rotationCurveW = new AnimationCurve();


				// NOTE: TODO - Correct handedness, transformation(setting local position is not correct as its wrt to mPelvis)

				for (int j = 0; j < joint.PositionKeys.Length; j++)
				{
					var key = joint.PositionKeys[j];

					var Xo = key.X / (float)ushort.MaxValue * 10.0f - 5.0f;
					var Yo = key.Y / (float)ushort.MaxValue * 10.0f - 5.0f;
					var Zo = key.Z / (float)ushort.MaxValue * 10.0f - 5.0f;

					// transform handedness
					var X = Xo;
					var Y = Zo;
					var Z = -Yo;

					// transform position
					var tmp = (bones[joint.JointName].parent.position - root.position);

					X = X - tmp.x;
					Y = Y - tmp.y;
					Z = Z - tmp.z;

					positionCurveX.AddKey(new Keyframe( key.Time / (float)ushort.MaxValue * animation.Header.Duration, X));
					positionCurveY.AddKey(new Keyframe( key.Time / (float)ushort.MaxValue * animation.Header.Duration, Y));
					positionCurveZ.AddKey(new Keyframe( key.Time / (float)ushort.MaxValue * animation.Header.Duration, Z));
				}

				for (int j = 0; j < joint.RotationKeys.Length; j++)
				{
					var key = joint.RotationKeys[j];

					var Xo = key.X / (float)ushort.MaxValue *2.0f - 1.0f;
					var Yo = key.Y / (float)ushort.MaxValue *2.0f - 1.0f;
					var Zo = key.Z / (float)ushort.MaxValue *2.0f - 1.0f;
					var Wo = Mathf.Sqrt(1.0f - Xo*Xo - Yo*Yo - Zo*Zo);

					// transform handedness

					var X = -Xo;
					var Y = -Zo;
					var Z = -Yo;
					var W = Wo;

					rotationCurveX.AddKey(new Keyframe( key.Time / (float)ushort.MaxValue * animation.Header.Duration, X));
					rotationCurveY.AddKey(new Keyframe( key.Time / (float)ushort.MaxValue * animation.Header.Duration, Y));
					rotationCurveZ.AddKey(new Keyframe( key.Time / (float)ushort.MaxValue * animation.Header.Duration, Z));
					rotationCurveW.AddKey(new Keyframe( key.Time / (float)ushort.MaxValue * animation.Header.Duration, W));
				}

				var path = GetRelativePath(root, bones, joint.JointName);
				
				//Debug.LogError($"Joint: {joint.JointName}, Path: {path}");
				/*
				string positionKeys = "";
				for(var k = 0; k < positionCurveX.keys.Length; k++)
				{
					positionKeys += $"({positionCurveX.keys[k].time}, [{positionCurveX.keys[k].value}, {positionCurveY.keys[k].value}, {positionCurveZ.keys[k].value}]), ";
				}
				Debug.LogError($"PositionKeys: {positionKeys}");
				string rotationKeys = "";
				for(var k = 0; k < rotationCurveX.keys.Length; k++)
				{
					rotationKeys += $"({rotationCurveX.keys[k].time}, [{rotationCurveX.keys[k].value}, {rotationCurveY.keys[k].value}, {rotationCurveZ.keys[k].value}, {rotationCurveW.keys[k].value}]), ";
				}
				Debug.LogError($"RotationKeys: {rotationKeys}");
				*/


				clip.SetCurve(path, typeof(Transform), "localPosition.x", positionCurveX);
				clip.SetCurve(path, typeof(Transform), "localPosition.y", positionCurveY);
				clip.SetCurve(path, typeof(Transform), "localPosition.z", positionCurveZ);

				clip.SetCurve(path, typeof(Transform), "localRotation.x", rotationCurveX);
				clip.SetCurve(path, typeof(Transform), "localRotation.y", rotationCurveY);
				clip.SetCurve(path, typeof(Transform), "localRotation.z", rotationCurveZ);
				clip.SetCurve(path, typeof(Transform), "localRotation.w", rotationCurveW);
			}


			// Debug.LogError(animation.PositionKeys());

			return clip;
		}
	}
}