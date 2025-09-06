using System;
using UnityEngine;

namespace CrystalFrost.Assets.Animation
{
	/// <summary>
	/// Defines an interface for decoding animation assets.
	/// </summary>
	public interface IAnimationDecoder
	{
		/// <summary>
		/// Decodes the specified animation request.
		/// </summary>
		/// <param name="request">The animation request to decode.</param>
		void Decode(AnimationRequest request);
	}

	/// <summary>
	/// Decodes animation assets from their raw format into a structured format.
	/// </summary>
	public class AnimationDecoder : IAnimationDecoder
	{
		private readonly IDecodedAnimationQueue _readyAnimationQueue;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationDecoder"/> class.
		/// </summary>
		/// <param name="readyAnimationQueue">The queue for decoded animations.</param>
		public AnimationDecoder(IDecodedAnimationQueue readyAnimationQueue)
		{
			_readyAnimationQueue = readyAnimationQueue;
		}
		
		/// <summary>
		/// Decodes the specified animation request and enqueues it when ready.
		/// </summary>
		/// <param name="request">The animation request to decode.</param>
		public void Decode(AnimationRequest request)
		{
			TranscodeFacetedAnimationAtDetailLevel(request);
		}

		private int ParseHeader(byte[] data, int offset, out AnimationHeader header)
		{
			header = new AnimationHeader();

			// little endian
			if (!BitConverter.IsLittleEndian)
			{
				Debug.LogError("Big endian not supported");
				return -1;
			}

			header.Version = BitConverter.ToUInt16(data, offset);
			offset += 2;

			header.SubVersion = BitConverter.ToUInt16(data, offset);
			offset += 2;

			header.BasePriority = BitConverter.ToInt32(data, offset);
			offset += 4;

			header.Duration = BitConverter.ToSingle(data, offset);
			offset += 4;

			int stringLength = 0;
			while (data[offset + stringLength] != 0)
			{
				stringLength++;
			}
			header.EmoteName = System.Text.Encoding.UTF8.GetString(data, offset, stringLength);
			offset += stringLength + 1;

			header.LoopInPoint = BitConverter.ToSingle(data, offset);
			offset += 4;

			header.LoopOutPoint = BitConverter.ToSingle(data, offset);
			offset += 4;

			header.Loop = BitConverter.ToInt32(data, offset);
			offset += 4;

			header.EaseInDuration = BitConverter.ToSingle(data, offset);
			offset += 4;

			header.EaseOutDuration = BitConverter.ToSingle(data, offset);
			offset += 4;

			var handPoseU32= BitConverter.ToUInt32(data, offset);
			header.HandPose = EHandPoseExtensions.FromUInt(handPoseU32);
			offset += 4;

			header.NumJoints = BitConverter.ToUInt32(data, offset);
			offset += 4;

			return offset;
		}

		private int ParseKeyframeData(byte[] data, int offset, out AnimationKeyframe keyframe)
		{
			// its just time, x, y, z all u16

			keyframe = new AnimationKeyframe();

			keyframe.Time = BitConverter.ToUInt16(data, offset);
			offset += 2;

			keyframe.X = BitConverter.ToUInt16(data, offset);
			offset += 2;

			keyframe.Y = BitConverter.ToUInt16(data, offset);
			offset += 2;

			keyframe.Z = BitConverter.ToUInt16(data, offset);
			offset = offset + 2;

			return offset;
		}

		private int ParseIndividualJointData(byte[] data, int offset, out AnimationJointData jointData)
		{

			jointData = new AnimationJointData();


			int stringLength = 0;
			while (data[offset + stringLength] != 0)
			{
				stringLength++;
			}
			jointData.JointName = System.Text.Encoding.UTF8.GetString(data, offset, stringLength);
			offset += stringLength + 1;

			jointData.JointPriority = BitConverter.ToInt32(data, offset);
			offset += 4;

			int rotationKeyCount = BitConverter.ToInt32(data, offset);
			offset += 4;

			jointData.RotationKeys = new AnimationKeyframe[rotationKeyCount];
			for (int i = 0; i < rotationKeyCount; i++)
			{
				offset = ParseKeyframeData(data, offset, out var keyframe);
				jointData.RotationKeys[i] = keyframe;
			}

			int positionKeyCount = BitConverter.ToInt32(data, offset);
			offset += 4;


			jointData.PositionKeys = new AnimationKeyframe[positionKeyCount];
			for (int i = 0; i < positionKeyCount; i++)
			{
				offset = ParseKeyframeData(data, offset, out var keyframe);
				jointData.PositionKeys[i] = keyframe;
			}

			return offset;
		}

		private int ParseJointData(byte[] data, int offset, out AnimationJointData[] jointData, int jointCount)
		{
			jointData = new AnimationJointData[jointCount];

			for (int i = 0; i < jointCount; i++)
			{
				offset = ParseIndividualJointData(data, offset, out var joint);
				jointData[i] = joint;
			}

			return offset;
		}

		private int ParseVector3(byte[] data, int offset, out OpenMetaverse.Vector3 vector)
		{
			vector = new OpenMetaverse.Vector3();

			vector.X = BitConverter.ToSingle(data, offset);
			offset += 4;

			vector.Y = BitConverter.ToSingle(data, offset);
			offset += 4;

			vector.Z = BitConverter.ToSingle(data, offset);
			offset += 4;

			return offset;
		}

		private int ParseIndividualConstraintData(byte[] data, int offset, out AnimationConstraint constraint)
		{
			constraint = new AnimationConstraint();

			constraint.ChainLength = data[offset];
			offset++;

			constraint.ConstraintType = data[offset];
			offset++;

			int stringLength = 0;
			while (data[offset + stringLength] != 0 && stringLength < 16)
			{
				stringLength++;
			}
			constraint.SourceVolume = System.Text.Encoding.UTF8.GetString(data, offset, stringLength);
			offset += 16;

			offset = ParseVector3(data, offset, out var sourceOffset);
			constraint.SourceOffset = sourceOffset;

			stringLength = 0;
			while (data[offset + stringLength] != 0 && stringLength < 16)
			{
				stringLength++;
			}
			constraint.TargetVolume = System.Text.Encoding.UTF8.GetString(data, offset, stringLength);
			offset += 16;
			
			offset = ParseVector3(data, offset, out var targetOffset);
			constraint.TargetOffset = targetOffset;

			offset = ParseVector3(data, offset, out var targetDir);
			constraint.TargetDir = targetDir;

			constraint.EaseInStart = BitConverter.ToSingle(data, offset);
			offset += 4;

			constraint.EaseInStop = BitConverter.ToSingle(data, offset);
			offset += 4;

			constraint.EaseOutStart = BitConverter.ToSingle(data, offset);
			offset += 4;

			constraint.EaseOutStop = BitConverter.ToSingle(data, offset);
			offset += 4;

			return offset;
		}

		private int ParseConstraintData(byte[] data, int offset, out AnimationConstraint[] constraintData)
		{
			int constraintCount = BitConverter.ToInt32(data, offset);
			offset += 4;

			constraintData = new AnimationConstraint[constraintCount];
			for (int i = 0; i < constraintCount; i++)
			{
				offset = ParseIndividualConstraintData(data, offset, out var constraint);
				constraintData[i] = constraint;
			}

			return offset;
		}

		private void TranscodeFacetedAnimationAtDetailLevel(AnimationRequest request)
		{
			var assetAnimation = request.AssetAnimation;

			// Parse the header
			request.DecodedAnimation = new DecodedAnimation();

			int offset = ParseHeader(assetAnimation.AssetData, 0, out var header);
			request.DecodedAnimation.Header = header;

			// Parse the joint data
			offset = ParseJointData(assetAnimation.AssetData, offset, out var jointData, (int)header.NumJoints);
			request.DecodedAnimation.JointData = jointData;

			// Parse the constraint data
			offset = ParseConstraintData(assetAnimation.AssetData, offset, out var constraintData);
			request.DecodedAnimation.Constraints = constraintData;
			request.DecodedAnimation.animationId = request.UUID;
			_readyAnimationQueue.Enqueue(request);
		}
	}
}
