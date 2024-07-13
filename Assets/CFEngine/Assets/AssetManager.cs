using CrystalFrost.Assets.Animation;
using CrystalFrost.Assets.Mesh;

namespace CrystalFrost.Assets
{
    public interface IAssetManager
    {
        public ITextureManager Textures { get; }
        public IMeshManager Meshes { get; }
        public IAnimationManager AnimationManager { get; }
    }

    public class AssetManager : IAssetManager
    {
        public ITextureManager Textures { get; }
        public IMeshManager Meshes { get; }
		public IAnimationManager AnimationManager { get; }

		public AssetManager(ITextureManager textures,
            IMeshManager meshes, IAnimationManager animationManager)
        {
            Textures = textures;
            Meshes = meshes;
            AnimationManager = animationManager;
        }
    }
}
