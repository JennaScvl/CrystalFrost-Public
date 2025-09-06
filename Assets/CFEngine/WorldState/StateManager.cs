using CrystalFrost.Assets.CFEngine.WorldState;

namespace CrystalFrost.WorldState
{
	/// <summary>
	/// Defines an interface for a manager that oversees the world state.
	/// </summary>
	public interface IStateManager
	{
		/// <summary>
		/// Gets the world state.
		/// </summary>
		IWorld World { get; }
	}

	/// <summary>
	/// Manages the overall state of the virtual world by coordinating various update handlers.
	/// </summary>
	public class StateManager : IStateManager
	{
		private readonly IHandleTerseUpdate _handleTerseUpdate;
		private readonly IHandleObjectBlockDataUpdate _handleObjectBlockDataUpdate;
		private readonly IHandleObjectUpdate _handleObjectUpdate;

		/// <summary>
		/// Gets the world state.
		/// </summary>
		public IWorld World { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StateManager"/> class.
		/// </summary>
		/// <param name="world">The world state.</param>
		/// <param name="handleTerseUpdate">The handler for terse object updates.</param>
		/// <param name="handleObjectBlockDataUpdate">The handler for object data block updates.</param>
		/// <param name="handleObjectUpdate">The handler for object updates.</param>
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
