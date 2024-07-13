using OpenMetaverse;
using CrystalFrost;
using UnityEngine;

public static class ClientManager
{
	public static bool isOpenSim = false;
    public static GridClient client;
    public static TexturePipeline texturePipeline;
    public static bool active = false;
    public static CFAssetManager assetManager;
    public static SimManager simManager;
    public static SoundManager soundManager;
    public static int mainThreadId;
    public static float viewDistance = 32f;
    public static Chat chat;
    public static ChatWindowUI chatWindow;
    public static Avatar avatar;

	public static CurrentOutfitFolder currentOutfitFolder;


	public static string DiffuseName = "_MainTex";
    public static string ColorName = "_Color";
    public static string EmissiveMapName = "_EmissionMap";
    public static string EmissiveColorName = "_EmissionColor";
#if MK_GLOW_PRESENT
	public static string MaterialNameModifier = "MK Glow ";
#else
    public static string MaterialNameModifier = "";
#endif
	// In Main method:

	// If called in the non main thread, will return false;
	public static bool IsMainThread
    {
        get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
    }
}
