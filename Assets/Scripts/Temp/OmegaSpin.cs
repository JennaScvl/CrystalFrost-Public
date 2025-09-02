using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Temp
{
	public struct OmegaSpin : IJobParallelFor
	{
		public NativeArray<quaternion> rotationArray;
		public NativeArray<quaternion> newrotationArray;
		[ReadOnly] public float deltaTime;

		public void Execute(int index)
		{
			rotationArray[index] = newrotationArray[index];
			//throw new NotImplementedException();
		}
	}
}