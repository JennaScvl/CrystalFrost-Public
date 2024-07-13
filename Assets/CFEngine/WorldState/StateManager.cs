using CrystalFrost.Assets.CFEngine.WorldState;

namespace CrystalFrost.WorldState
{
	public interface IStateManager
	{
		IWorld World { get; }
	}

	public class StateManager : IStateManager
	{
		private readonly IHandleTerseUpdate _handleTerseUpdate;
		private readonly IHandleObjectBlockDataUpdate _handleObjectBlockDataUpdate;
		private readonly IHandleObjectUpdate _handleObjectUpdate;

		public IWorld World { get; private set; }

		public StateManager(
			IWorld world,
			IHandleTerseUpdate handleTerseUpdate,
			IHandleObjectBlockDataUpdate handleObjectBlockDataUpdate,
			IHandleObjectUpdate handleObjectUpdate)
		{
			World = world;
			_handleTerseUpdate = handleTerseUpdate;
			_handleObjectBlockDataUpdate = handleObjectBlockDataUpdate;
			_handleObjectUpdate = handleObjectUpdate;
		}
	}
}
