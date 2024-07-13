namespace CrystalFrost.Config
{
	/// <summary>
	/// represents configuration values that affect if newly coded features are used
	/// These are meant to be removed once a feature that is being developed is no longer
	/// experimental.
	/// </summary>
    public class CodeConfig
    {
        public const string subsectionName = "Code";

		/// <summary>
		/// Is the new code for managing the objects enabled?
        public bool UseNewObjectGraph { get; set; } = false;

		/// <summary>
		/// should the renderer limit itself it objects on the current sim?
		/// </summary>
        public bool LimitToCurrentRegion { get; set;} = false;

        public uint LimitQueueItemsPerUpdateTo { get; set; } = 1;

		// Time interval to wait before executing UpdateCamera method
		public float updateCameraInterval { get; set; } = 0.1f;
	}
}
