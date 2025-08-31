using System;
using CrystalFrost.Assets;
using CrystalFrost.Assets.CFEngine.WorldState;
using CrystalFrost.Assets.Mesh;
using CrystalFrost.Assets.Textures;
using CrystalFrost.Assets.Textures.OpenJ2K;

using CrystalFrost.Assets.Textures.AVLJ2K;
using CrystalFrost.Client.Credentials;
using CrystalFrost.Config;
using CrystalFrost.Exceptions;
using CrystalFrost.Lib;
using CrystalFrost.Logging;
using CrystalFrost.Timing;
using CrystalFrost.UnityRendering;
using CrystalFrost.WorldState;
using CrystalFrost.ObjectPooling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CrystalFrost.Assets.Animation;

namespace CrystalFrost
{
	/// <summary>
	/// Provides a Dependency Injection Serivce Provider.
	/// </summary>
	public class Services
	{
		private static readonly IServiceCollection _serviceCollection = new ServiceCollection();
		private static IServiceProvider _serviceProvider = default!;

		private static readonly object _initLock = new();
		private static bool _initialized = false;
		private static ILogger<Services> _log;

		/// <summary>
		/// Gets an instance of <typeparamref name="T"/>
		/// Will throw if its unable to provide a <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="ApplicationException"></exception>
		public static T GetService<T>()
		{
			if (!_initialized) Initialize();

			// if this throws, check the component registrations below.
			return
				Perf.Measure("Services.GetService", () => _serviceProvider.GetService<T>())
				?? throw new ApplicationException($"There requested type {typeof(T).FullName} could not be provided.");
		}

        /// <summary>
        /// Configuration root for the application.
        /// This is built up from defaults, config files, environment variables, and command line arguments.
        /// </summary>
        private static IConfigurationRoot _configRoot = default!;
        public static IConfigurationSection GetConfigSection (string sect) => Services._configRoot.GetSection(sect);

		private static void Initialize()
		{
			// its hypothetically possible that two threads could try to initialize
			// at the same time. only one of them will get to grab the lock object.
			// if that happens, the second thread will be forced to wait, and after
			// the wait, the first thread will have set initialized to true, and the
			// second thread will return without re-initializing.
			lock (_initLock)
			{
				if (_initialized) return;

				Perf.Measure("Services.RegisterComponents", RegisterComponents);

				_serviceProvider = _serviceCollection.BuildServiceProvider();
				_initialized = true;
				_log.DIProviderInitialized();
			}

			var globalEx = _serviceProvider.GetService<IGlobalExceptionHandler>();
			globalEx.Initialize();
		}

		private static void RegisterComponents()
		{

            Services._configRoot = new ConfigurationBuilder()
                // Logging config is handled as the Logging subsection of the main config.
                .AddInMemoryCollection(LoggingConfig.DefaultValues)
                // .AddJsonFile("cf-config.json", optional: true, reloadOnChange: true)
                // .AddEnvironmentVariables("CF_")
                // .AddCommandLine(Environment.GetCommandLineArgs())
                .Build();

            // UnityEngine.Debug.Log(Services._configRoot.GetDebugView()); // For debugging configuration

            // Add IOptions config for the individual config sections
            // This will allow someone to inject IOptions<CodeConfig> to get the config params.
            //     Using the IOptions interface allows the config to be changed at runtime
            //     (see IOptionsSnapshot for more info).
            _serviceCollection.Configure<GridConfig>(Services._configRoot.GetSection(GridConfig.subsectionName));
            _serviceCollection.Configure<CodeConfig>(Services._configRoot.GetSection(CodeConfig.subsectionName));
            _serviceCollection.Configure<MeshConfig>(Services._configRoot.GetSection(MeshConfig.subsectionName));
            _serviceCollection.Configure<TextureConfig>(Services._configRoot.GetSection(TextureConfig.subsectionName));
            _serviceCollection.Configure<LoggingConfig>(Services._configRoot.GetSection(LoggingConfig.subsectionName));

			// The logging config will usually come out of ConfigRoot
			var loggingConfig = LoggingConfiguration.GetLoggingConfiguration();
			// for now use the logger that uses UnityEngine.Debug
			var loggingProvder = new UnityDebugLoggerProvider(loggingConfig);

			// build a logger by hand so we can use it here.
			// the service provider can't give us a logger because it's
			// not setup yet.
			using var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.AddProvider(loggingProvder);
			});
			_log = loggerFactory.CreateLogger<Services>();

			// now we have a logger we can use in here.
			_log.DIRegisteringComponents();

			// register our logging with serviceCollection.
			_serviceCollection.AddLogging(builder =>
			{
				builder
					.AddConfiguration(loggingConfig)
					.AddProvider(loggingProvder);
			});

			_serviceCollection.AddSingleton<ILoginUriProvider, LoginUriProvider>();

			// provide a grid client factory, so that a GridClient can be created when
			// requested.
			_serviceCollection.AddSingleton<IGridClientFactory, GridClientFactory>();

			// When a grid client first is needed, use the registered factory to build one
			// from then on, always use that same instance (singleton)
			_serviceCollection.AddSingleton(p => p.GetService<IGridClientFactory>().BuildGridClient());

			// Credentials Store Stuff.
			_serviceCollection.AddScoped<IDefaultCredentialsStore, DefaultCredentialsStore>();
			_serviceCollection.AddScoped<IWindowsCredentialsStore, WindowsCredentialsStore>();
			_serviceCollection.AddSingleton<ICredentialsStoreFactory, CredentialsStoreFactory>();
			_serviceCollection.AddSingleton<IAesEncryptor, AesEncryptor>();

			_serviceCollection.AddScoped(p => p.GetService<ICredentialsStoreFactory>().GetCredentialsStore());

			// a way to signal to background threads to stop, so the application can close gracefully.
			_serviceCollection.AddSingleton<IProvideShutdownSignal, ProvideShutdownSignal>();

			// Mesh Stuff
			_serviceCollection.AddSingleton<IDecodedMeshQueue, DecodedMeshQueue>();
			_serviceCollection.AddSingleton<IMeshRequestQueue, MeshRequestQueue>();
			_serviceCollection.AddSingleton<IMeshDownloadRequestQueue, MeshDownloadRequestQueue>();
			_serviceCollection.AddSingleton<IDownloadedMeshQueue, DownloadedMeshQueue>();
			_serviceCollection.AddSingleton<IDownloadedMeshCacheQueue, DownloadedMeshCacheQueue>();
			_serviceCollection.AddSingleton<IMeshDownloadWorker, MeshDownloadWorker>();
			_serviceCollection.AddSingleton<IMeshDecodeWorker, MeshDecodeWorker>();
			_serviceCollection.AddSingleton<IMeshCacheWorker, MeshCacheWorker>();
			_serviceCollection.AddSingleton<IMeshManager, MeshManager>();
			_serviceCollection.AddScoped<IMeshDecoder, MeshDecoder>();

			// decode images
			_serviceCollection.AddScoped<ITgaReader, TgaReader>();
			// _serviceCollection.AddScoped<ITextureDecoder, OpenJ2KTextureDecoder>();
			//_serviceCollection.AddScoped<ITextureDecoder, CSJ2KTextureDecoder>();
			_serviceCollection.AddScoped<ITextureDecoder, AVLJ2KTextureDecoder>();

			// texture manager
			_serviceCollection.AddSingleton<ITextureManager, TextureManager>();
			_serviceCollection.AddSingleton<ITextureDecodeWorker, TextureDecodeWorker>();
			_serviceCollection.AddSingleton<ITextureDownloadWorker, TextureDownloadWorker>();
			_serviceCollection.AddSingleton<ITextureRequestQueue, TextureRequestQueue>();
			_serviceCollection.AddSingleton<IDecodedTextureCacheQueue, DownloadedTextureCacheQueue>();
			_serviceCollection.AddSingleton<IReadyTextureQueue, ReadyTextureQueue>();
			_serviceCollection.AddSingleton<IDownloadedTextureQueue, TextureQueues>();
			_serviceCollection.AddSingleton<ITextureCacheWorker, TextureCacheWorker>();
			_serviceCollection.AddSingleton<ITextureDownloadRequestQueue, TextureDownloadRequestQueue>();

			// Animation manager
			_serviceCollection.AddSingleton<IDecodedAnimationQueue, DecodedAnimationQueue>();
			_serviceCollection.AddSingleton<IAnimationRequestQueue, AnimationRequestQueue>();
			_serviceCollection.AddSingleton<IAnimationDownloadRequestQueue, AnimationDownloadRequestQueue>();
			_serviceCollection.AddSingleton<IDownloadedAnimationQueue, DownloadedAnimationQueue>();
			_serviceCollection.AddSingleton<IDownloadedAnimationCacheQueue, DownloadedAnimationCacheQueue>();
			_serviceCollection.AddSingleton<IAnimationDownloadWorker, AnimationDownloadWorker>();
			_serviceCollection.AddSingleton<IAnimationDecodeWorker, AnimationDecodeWorker>();
			_serviceCollection.AddSingleton<IAnimationCacheWorker, AnimationCacheWorker>();
			_serviceCollection.AddSingleton<IAnimationManager, AnimationManager>();
			_serviceCollection.AddScoped<IAnimationDecoder, AnimationDecoder>();

			// asset manager.
			_serviceCollection.AddSingleton<IAssetManager, AssetManager>();

			// object pooling
			_serviceCollection.AddSingleton<IObjectPoolingService, ObjectPoolingService>();

			// puts the unity application and editor modes behind an abstraction
			// (facilitates unit testing)
			_serviceCollection.AddSingleton<IUnityEditorEvents, UnityEditorEvents>();
			_serviceCollection.AddSingleton<IEngineBehaviorEvents, EngineBehaviorEvents>();

			_serviceCollection.AddSingleton<ITransformTexCoords, TransformTexCoordsForUnity>();

			_serviceCollection.AddSingleton<IGlobalExceptionHandler, GlobalExceptionHandler>();

			// world state stuff
			_serviceCollection.AddSingleton<IHandleTerseUpdate, HandleTerseUpdate>();
			_serviceCollection.AddSingleton<IHandleObjectUpdate, HandleObjectUpdate>();
			_serviceCollection.AddSingleton<IHandleObjectBlockDataUpdate, HandleObjectBlockDataUpdate>();
			_serviceCollection.AddSingleton<IStateManager, StateManager>();
			_serviceCollection.AddSingleton<IAllSimObject, AllSimObjects>();
			_serviceCollection.AddSingleton<IAllRegions, AllRegions>();
			_serviceCollection.AddSingleton<IWorld, World>();
			_serviceCollection.AddSingleton<INewSimObjectQueue, NewSimObjectQueue>();
			_serviceCollection.AddSingleton<IUnityRenderManager, UnityRenderManager>();
			_serviceCollection.AddSingleton<IAllSceneObjects, AllSceneObjects>();
			_serviceCollection.AddSingleton<ISceneObjectsNeedingRenderersQueue, SceneObjectsNeedingRenderersQueue>();

			// capture logging from LibMetaverse
			_serviceCollection.AddSingleton<ILMVLogger, LMVLogger>();

			// add more registrations here.

			_log.DIRegistrationComplete();
		}
	}
}
