using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenMetaverse;

namespace CrystalFrost.Assets.Animation
{
	/// <summary>
	/// Defines the possible hand poses for an animation.
	/// </summary>
	public enum EHandPose
	{
		HandPoseSpread = 0,
		HandPoseRelaxed,
		HandPosePoint,
		HandPoseFist,
		HandPoseRelaxedL,
		HandPosePointL,
		HandPoseFistL,
		HandPoseRelaxedR,
		HandPosePointR,
		HandPoseFistR,
		HandPoseSaluteR,
		HandPoseTyping,
		HandPosePeaceR,
		HandPosePalmR,
		NumHandPoses
	}

	/// <summary>
	/// Provides extension methods for the <see cref="EHandPose"/> enum.
	/// </summary>
	public static class EHandPoseExtensions
	{
		/// <summary>
		/// Converts a uint to an <see cref="EHandPose"/>.
		/// </summary>
		/// <param name="value">The uint value to convert.</param>
		/// <returns>The converted <see cref="EHandPose"/>.</returns>
		public static EHandPose FromUInt(uint value)
		{
			if (value >= (uint)EHandPose.NumHandPoses)
			{
				throw new ArgumentOutOfRangeException(nameof(value), "Invalid hand pose value.");
			}

			return (EHandPose)value;
		}
	}

	/// <summary>
	/// Represents the header of an animation.
	/// </summary>
	public struct AnimationHeader
	{
		public ushort Version { get; set; }
		public ushort SubVersion { get; set; }
		public int BasePriority { get; set; }
		public float Duration { get; set; }
		public string EmoteName { get; set; }
		public float LoopInPoint { get; set; }
		public float LoopOutPoint { get; set; }
		public int Loop { get; set; }
		public float EaseInDuration { get; set; }
		public float EaseOutDuration { get; set; }
		public EHandPose HandPose { get; set; }
		public uint NumJoints { get; set; }

		public override string ToString()
		{
			return $"Version: {Version}, SubVersion: {SubVersion}, BasePriority: {BasePriority}, Duration: {Duration}, EmoteName: {EmoteName}, LoopInPoint: {LoopInPoint}, LoopOutPoint: {LoopOutPoint}, Loop: {Loop}, EaseInDuration: {EaseInDuration}, EaseOutDuration: {EaseOutDuration}, HandPose: {HandPose}, NumJoints: {NumJoints}";
		}
	}

	/// <summary>
	/// Represents a keyframe in an animation.
	/// </summary>
	public struct AnimationKeyframe
	{
		public ushort Time { get; set; }
		public ushort X { get; set; }
		public ushort Y { get; set; }
		public ushort Z { get; set; }

		public override string ToString()
		{
			return $"Time: {Time}, X: {X}, Y: {Y}, Z: {Z}";
		}

		public AnimationKeyframe(ushort time, ushort x, ushort y, ushort z)
		{
			Time = time;
			X = x;
			Y = y;
			Z = z;
		}
	}

	/// <summary>
	/// Represents the animation data for a single joint.
	/// </summary>
	public struct AnimationJointData
	{
		public string JointName { get; set; }
		public int JointPriority { get; set; }

		public AnimationKeyframe[] RotationKeys { get; set; }
		public AnimationKeyframe[] PositionKeys { get; set; }

		public override string ToString()
		{
			return $"JointName: {JointName}, JointPriority: {JointPriority}, RotationKeys: {RotationKeys.Length}, PositionKeys: {PositionKeys.Length}";
		}
	}

	/// <summary>
	/// Represents a constraint in an animation.
	/// </summary>
	public struct AnimationConstraint
	{
		public byte ChainLength { get; set; }  // U8
		public byte ConstraintType { get; set; }  // U8 (0: point*, 1: plane)
		public string SourceVolume { get; set; }  // char[16]
		public OpenMetaverse.Vector3 SourceOffset { get; set; }  
		public string TargetVolume { get; set; }  // char[16]
		public OpenMetaverse.Vector3 TargetOffset { get; set; }  
		public OpenMetaverse.Vector3 TargetDir { get; set; }  
		public float EaseInStart { get; set; }  // F32
		public float EaseInStop { get; set; }  // F32
		public float EaseOutStart { get; set; }  // F32
		public float EaseOutStop { get; set; }  // F32

		public override string ToString()
		{
			return $"ChainLength: {ChainLength}, ConstraintType: {ConstraintType}, SourceVolume: {SourceVolume}, SourceOffset: {SourceOffset}, TargetVolume: {TargetVolume}, TargetOffset: {TargetOffset}, TargetDir: {TargetDir}, EaseInStart: {EaseInStart}, EaseInStop: {EaseInStop}, EaseOutStart: {EaseOutStart}, EaseOutStop: {EaseOutStop}";
		}
	}

	/// <summary>
	/// Represents a decoded animation, including its header, joint data, and constraints.
	/// </summary>
	public class DecodedAnimation
	{
		public UUID animationId;
		public AnimationHeader Header;
		public AnimationJointData[] JointData;
		public AnimationConstraint[] Constraints;
	}
}

