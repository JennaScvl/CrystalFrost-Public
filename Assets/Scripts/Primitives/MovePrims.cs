using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Primitives
{
public struct MovePrims : IJobParallelFor
{
	public NativeArray<float3> positionArray;
	public NativeArray<float3> newpositionArray;
	public NativeArray<quaternion> rotationArray;
	public NativeArray<quaternion> newrotationArray;

	public void Execute(int index)
	{
		positionArray[index] = newpositionArray[index];
		rotationArray[index] = newrotationArray[index];
		//throw new NotImplementedException();
	}
}


}