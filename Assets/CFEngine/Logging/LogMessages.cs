//------------------------------------------------------
// This is a generated file. Do not make manual changes.
// You probably want to edit LogMessages.xml instead.
//------------------------------------------------------
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using UUID = OpenMetaverse.UUID;

namespace CrystalFrost
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId GridClientCacheDirSet_Event
            = new(10001, "GridClientCacheDirSet");
        internal static readonly Action<ILogger, string, Exception> GridClientCacheDirSet_Action
            = LoggerMessage.Define<string>(LogLevel.Debug,
                GridClientCacheDirSet_Event,
                "The GridClient ASSET_CACHE_DIR is set to {Dir}");
        /// <summary>
        /// (10001) Debug: The GridClient ASSET_CACHE_DIR is set to {Dir}
        /// </summary>
        [Conditional("DEBUG")]
        public static void GridClientCacheDirSet(this ILogger logger, string dir)
            => GridClientCacheDirSet_Action(logger, dir, null!);

        internal static readonly EventId FileNotFound_Event
            = new(10002, "FileNotFound");
        internal static readonly Action<ILogger, string, Exception> FileNotFound_Action
            = LoggerMessage.Define<string>(LogLevel.Information,
                FileNotFound_Event,
                "The file '{Filename}' does not exist.");
        /// <summary>
        /// (10002) Information: The file '{Filename}' does not exist.
        /// </summary>
        [Conditional("DEBUG")]
        public static void FileNotFound(this ILogger logger, string filename)
            => FileNotFound_Action(logger, filename, null!);

        internal static readonly EventId BackgroundWorkerConcurrency_Event
            = new(10003, "BackgroundWorkerConcurrency");
        internal static readonly Action<ILogger, string, int, Exception> BackgroundWorkerConcurrency_Action
            = LoggerMessage.Define<string, int>(LogLevel.Debug,
                BackgroundWorkerConcurrency_Event,
                "Background Worker {Name} is processing {Count} units of work.");
        /// <summary>
        /// (10003) Debug: Background Worker {Name} is processing {Count} units of work.
        /// </summary>
        [Conditional("DEBUG")]
        public static void BackgroundWorkerConcurrency(this ILogger logger, string name, int count)
            => BackgroundWorkerConcurrency_Action(logger, name, count, null!);

        internal static readonly EventId BackgroundWorkerTaskFailed_Event
            = new(10004, "BackgroundWorkerTaskFailed");
        internal static readonly Action<ILogger, string, Exception> BackgroundWorkerTaskFailed_Action
            = LoggerMessage.Define<string>(LogLevel.Error,
                BackgroundWorkerTaskFailed_Event,
                "Background Worker {Name} had a task fail.");
        /// <summary>
        /// (10004) Error: Background Worker {Name} had a task fail.
        /// </summary>
        public static void BackgroundWorkerTaskFailed(this ILogger logger, string name, Exception ex)
            => BackgroundWorkerTaskFailed_Action(logger, name, ex);

        internal static readonly EventId ShutdownSignalSet_Event
            = new(10005, "ShutdownSignalSet");
        internal static readonly Action<ILogger, Exception> ShutdownSignalSet_Action
            = LoggerMessage.Define(LogLevel.Information,
                ShutdownSignalSet_Event,
                "The Shutdown Signal has been set.");
        /// <summary>
        /// (10005) Information: The Shutdown Signal has been set.
        /// </summary>
        [Conditional("DEBUG")]
        public static void ShutdownSignalSet(this ILogger logger)
            => ShutdownSignalSet_Action(logger, null!);

        internal static readonly EventId DIProviderInitialized_Event
            = new(10006, "DIProviderInitialized");
        internal static readonly Action<ILogger, Exception> DIProviderInitialized_Action
            = LoggerMessage.Define(LogLevel.Debug,
                DIProviderInitialized_Event,
                "DI Service Provider has been initialized.");
        /// <summary>
        /// (10006) Debug: DI Service Provider has been initialized.
        /// </summary>
        [Conditional("DEBUG")]
        public static void DIProviderInitialized(this ILogger logger)
            => DIProviderInitialized_Action(logger, null!);

        internal static readonly EventId DIRegisteringComponents_Event
            = new(10007, "DIRegisteringComponents");
        internal static readonly Action<ILogger, Exception> DIRegisteringComponents_Action
            = LoggerMessage.Define(LogLevel.Debug,
                DIRegisteringComponents_Event,
                "Registering components in DI Container.");
        /// <summary>
        /// (10007) Debug: Registering components in DI Container.
        /// </summary>
        [Conditional("DEBUG")]
        public static void DIRegisteringComponents(this ILogger logger)
            => DIRegisteringComponents_Action(logger, null!);

        internal static readonly EventId DIRegistrationComplete_Event
            = new(10008, "DIRegistrationComplete");
        internal static readonly Action<ILogger, Exception> DIRegistrationComplete_Action
            = LoggerMessage.Define(LogLevel.Debug,
                DIRegistrationComplete_Event,
                "Finished registering components in the DI Container.");
        /// <summary>
        /// (10008) Debug: Finished registering components in the DI Container.
        /// </summary>
        [Conditional("DEBUG")]
        public static void DIRegistrationComplete(this ILogger logger)
            => DIRegistrationComplete_Action(logger, null!);

        internal static readonly EventId EditorEvent_AfterAssemblyReload_Event
            = new(10009, "EditorEvent_AfterAssemblyReload");
        internal static readonly Action<ILogger, Exception> EditorEvent_AfterAssemblyReload_Action
            = LoggerMessage.Define(LogLevel.Debug,
                EditorEvent_AfterAssemblyReload_Event,
                "Unity Editor After Assembly Reload");
        /// <summary>
        /// (10009) Debug: Unity Editor After Assembly Reload
        /// </summary>
        [Conditional("DEBUG")]
        public static void EditorEvent_AfterAssemblyReload(this ILogger logger)
            => EditorEvent_AfterAssemblyReload_Action(logger, null!);

        internal static readonly EventId EditorEvent_BeforeAssemblyReload_Event
            = new(10010, "EditorEvent_BeforeAssemblyReload");
        internal static readonly Action<ILogger, Exception> EditorEvent_BeforeAssemblyReload_Action
            = LoggerMessage.Define(LogLevel.Debug,
                EditorEvent_BeforeAssemblyReload_Event,
                "Unity Editor Before Assembly Reload");
        /// <summary>
        /// (10010) Debug: Unity Editor Before Assembly Reload
        /// </summary>
        [Conditional("DEBUG")]
        public static void EditorEvent_BeforeAssemblyReload(this ILogger logger)
            => EditorEvent_BeforeAssemblyReload_Action(logger, null!);

        internal static readonly EventId EditorEvent_WantsToQuit_Event
            = new(10011, "EditorEvent_WantsToQuit");
        internal static readonly Action<ILogger, Exception> EditorEvent_WantsToQuit_Action
            = LoggerMessage.Define(LogLevel.Debug,
                EditorEvent_WantsToQuit_Event,
                "Unity Editor wants to quit");
        /// <summary>
        /// (10011) Debug: Unity Editor wants to quit
        /// </summary>
        [Conditional("DEBUG")]
        public static void EditorEvent_WantsToQuit(this ILogger logger)
            => EditorEvent_WantsToQuit_Action(logger, null!);

        internal static readonly EventId EditorEvent_Quitting_Event
            = new(10012, "EditorEvent_Quitting");
        internal static readonly Action<ILogger, Exception> EditorEvent_Quitting_Action
            = LoggerMessage.Define(LogLevel.Debug,
                EditorEvent_Quitting_Event,
                "Unity Editor quitting");
        /// <summary>
        /// (10012) Debug: Unity Editor quitting
        /// </summary>
        [Conditional("DEBUG")]
        public static void EditorEvent_Quitting(this ILogger logger)
            => EditorEvent_Quitting_Action(logger, null!);

        internal static readonly EventId EditorEvent_ProjectChanged_Event
            = new(10013, "EditorEvent_ProjectChanged");
        internal static readonly Action<ILogger, Exception> EditorEvent_ProjectChanged_Action
            = LoggerMessage.Define(LogLevel.Debug,
                EditorEvent_ProjectChanged_Event,
                "Unity Editor project changed");
        /// <summary>
        /// (10013) Debug: Unity Editor project changed
        /// </summary>
        [Conditional("DEBUG")]
        public static void EditorEvent_ProjectChanged(this ILogger logger)
            => EditorEvent_ProjectChanged_Action(logger, null!);

        internal static readonly EventId EditorEvent_PlayModeChange_Event
            = new(10014, "EditorEvent_PlayModeChange");
        internal static readonly Action<ILogger, Enum, Exception> EditorEvent_PlayModeChange_Action
            = LoggerMessage.Define<Enum>(LogLevel.Debug,
                EditorEvent_PlayModeChange_Event,
                "Unity Editor play mode change: {Mode}");
        /// <summary>
        /// (10014) Debug: Unity Editor play mode change: {Mode}
        /// </summary>
        [Conditional("DEBUG")]
        public static void EditorEvent_PlayModeChange(this ILogger logger, Enum mode)
            => EditorEvent_PlayModeChange_Action(logger, mode, null!);

        internal static readonly EventId EditorEvent_PauseStateChange_Event
            = new(10015, "EditorEvent_PauseStateChange");
        internal static readonly Action<ILogger, Enum, Exception> EditorEvent_PauseStateChange_Action
            = LoggerMessage.Define<Enum>(LogLevel.Debug,
                EditorEvent_PauseStateChange_Event,
                "Unity Editor pause state change: {PauseState}");
        /// <summary>
        /// (10015) Debug: Unity Editor pause state change: {PauseState}
        /// </summary>
        [Conditional("DEBUG")]
        public static void EditorEvent_PauseStateChange(this ILogger logger, Enum pauseState)
            => EditorEvent_PauseStateChange_Action(logger, pauseState, null!);

        internal static readonly EventId EditorEvent_HierarchyChanged_Event
            = new(10016, "EditorEvent_HierarchyChanged");
        internal static readonly Action<ILogger, Exception> EditorEvent_HierarchyChanged_Action
            = LoggerMessage.Define(LogLevel.Debug,
                EditorEvent_HierarchyChanged_Event,
                "Unity Editor hierarchy changed");
        /// <summary>
        /// (10016) Debug: Unity Editor hierarchy changed
        /// </summary>
        [Conditional("DEBUG")]
        public static void EditorEvent_HierarchyChanged(this ILogger logger)
            => EditorEvent_HierarchyChanged_Action(logger, null!);

        internal static readonly EventId Render_NewOrphanObject_Event
            = new(10017, "Render_NewOrphanObject");
        internal static readonly Action<ILogger, uint, uint, Exception> Render_NewOrphanObject_Action
            = LoggerMessage.Define<uint, uint>(LogLevel.Warning,
                Render_NewOrphanObject_Event,
                "The Child Object {LocalID} was sent to the render code before its Parent {ParentID}. Object Re-enqued.");
        /// <summary>
        /// (10017) Warning: The Child Object {LocalID} was sent to the render code before its Parent {ParentID}. Object Re-enqued.
        /// </summary>
        public static void Render_NewOrphanObject(this ILogger logger, uint localID, uint parentID)
            => Render_NewOrphanObject_Action(logger, localID, parentID, null!);

        internal static readonly EventId Render_SceneObjectNeedsRenderers_Event
            = new(10018, "Render_SceneObjectNeedsRenderers");
        internal static readonly Action<ILogger, uint, Exception> Render_SceneObjectNeedsRenderers_Action
            = LoggerMessage.Define<uint>(LogLevel.Debug,
                Render_SceneObjectNeedsRenderers_Event,
                "The Object {LocalID} needs renderers.");
        /// <summary>
        /// (10018) Debug: The Object {LocalID} needs renderers.
        /// </summary>
        [Conditional("DEBUG")]
        public static void Render_SceneObjectNeedsRenderers(this ILogger logger, uint localID)
            => Render_SceneObjectNeedsRenderers_Action(logger, localID, null!);

    }
}

namespace CrystalFrost.Client.Credentials
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId LoadingCredentials_Event
            = new(20001, "LoadingCredentials");
        internal static readonly Action<ILogger, Exception> LoadingCredentials_Action
            = LoggerMessage.Define(LogLevel.Debug,
                LoadingCredentials_Event,
                "Loading Credentials");
        /// <summary>
        /// (20001) Debug: Loading Credentials
        /// </summary>
        [Conditional("DEBUG")]
        public static void LoadingCredentials(this ILogger logger)
            => LoadingCredentials_Action(logger, null!);

        internal static readonly EventId ErrorReadingCredentials_Event
            = new(20002, "ErrorReadingCredentials");
        internal static readonly Action<ILogger, string, Exception> ErrorReadingCredentials_Action
            = LoggerMessage.Define<string>(LogLevel.Warning,
                ErrorReadingCredentials_Event,
                "There was a problem reading the credentials file {Filename}");
        /// <summary>
        /// (20002) Warning: There was a problem reading the credentials file {Filename}
        /// </summary>
        public static void ErrorReadingCredentials(this ILogger logger, string filename, Exception ex)
            => ErrorReadingCredentials_Action(logger, filename, ex);

        internal static readonly EventId SavingCredentials_Event
            = new(20003, "SavingCredentials");
        internal static readonly Action<ILogger, Exception> SavingCredentials_Action
            = LoggerMessage.Define(LogLevel.Debug,
                SavingCredentials_Event,
                "Saving Credentials");
        /// <summary>
        /// (20003) Debug: Saving Credentials
        /// </summary>
        [Conditional("DEBUG")]
        public static void SavingCredentials(this ILogger logger)
            => SavingCredentials_Action(logger, null!);

        internal static readonly EventId SavedEncryptedCredentials_Event
            = new(20004, "SavedEncryptedCredentials");
        internal static readonly Action<ILogger, string, Exception> SavedEncryptedCredentials_Action
            = LoggerMessage.Define<string>(LogLevel.Debug,
                SavedEncryptedCredentials_Event,
                "Saved Encrypted Credentials to {Filename}");
        /// <summary>
        /// (20004) Debug: Saved Encrypted Credentials to {Filename}
        /// </summary>
        [Conditional("DEBUG")]
        public static void SavedEncryptedCredentials(this ILogger logger, string filename)
            => SavedEncryptedCredentials_Action(logger, filename, null!);

    }
}

namespace CrystalFrost.Lib
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId UnsupportedBitDepth_Event
            = new(30001, "UnsupportedBitDepth");
        internal static readonly Action<ILogger, int, Exception> UnsupportedBitDepth_Action
            = LoggerMessage.Define<int>(LogLevel.Warning,
                UnsupportedBitDepth_Event,
                "Unsupported bit depth: {BitsPerPixel}");
        /// <summary>
        /// (30001) Warning: Unsupported bit depth: {BitsPerPixel}
        /// </summary>
        public static void UnsupportedBitDepth(this ILogger logger, int bitsPerPixel)
            => UnsupportedBitDepth_Action(logger, bitsPerPixel, null!);

    }
}

namespace CrystalFrost.Assets.Textures
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId TextureRequested_Event
            = new(40001, "TextureRequested");
        internal static readonly Action<ILogger, UUID, Exception> TextureRequested_Action
            = LoggerMessage.Define<UUID>(LogLevel.Debug,
                TextureRequested_Event,
                "Texture Requested: {Uuid}");
        /// <summary>
        /// (40001) Debug: Texture Requested: {Uuid}
        /// </summary>
        [Conditional("DEBUG")]
        public static void TextureRequested(this ILogger logger, UUID uuid)
            => TextureRequested_Action(logger, uuid, null!);

        internal static readonly EventId DuplicateTextureRequest_Event
            = new(40002, "DuplicateTextureRequest");
        internal static readonly Action<ILogger, UUID, Exception> DuplicateTextureRequest_Action
            = LoggerMessage.Define<UUID>(LogLevel.Debug,
                DuplicateTextureRequest_Event,
                "Duplicate Texture Request Discarded: {Uuid}");
        /// <summary>
        /// (40002) Debug: Duplicate Texture Request Discarded: {Uuid}
        /// </summary>
        [Conditional("DEBUG")]
        public static void DuplicateTextureRequest(this ILogger logger, UUID uuid)
            => DuplicateTextureRequest_Action(logger, uuid, null!);

        internal static readonly EventId PendingTextureRemoved_Event
            = new(40003, "PendingTextureRemoved");
        internal static readonly Action<ILogger, UUID, Exception> PendingTextureRemoved_Action
            = LoggerMessage.Define<UUID>(LogLevel.Debug,
                PendingTextureRemoved_Event,
                "Texture is no longer pending: {Uuid}");
        /// <summary>
        /// (40003) Debug: Texture is no longer pending: {Uuid}
        /// </summary>
        [Conditional("DEBUG")]
        public static void PendingTextureRemoved(this ILogger logger, UUID uuid)
            => PendingTextureRemoved_Action(logger, uuid, null!);

    }
}

namespace CrystalFrost.Assets.Mesh
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId MeshRequested_Event
            = new(50001, "MeshRequested");
        internal static readonly Action<ILogger, UUID, Exception> MeshRequested_Action
            = LoggerMessage.Define<UUID>(LogLevel.Debug,
                MeshRequested_Event,
                "Mesh Requested: {Uuid}");
        /// <summary>
        /// (50001) Debug: Mesh Requested: {Uuid}
        /// </summary>
        [Conditional("DEBUG")]
        public static void MeshRequested(this ILogger logger, UUID uuid)
            => MeshRequested_Action(logger, uuid, null!);

    }
}

namespace CrystalFrost.WorldState
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId TerseObjectUpdate_Event
            = new(60001, "TerseObjectUpdate");
        internal static readonly Action<ILogger, uint, Exception> TerseObjectUpdate_Action
            = LoggerMessage.Define<uint>(LogLevel.Information,
                TerseObjectUpdate_Event,
                "Terse Object Update Recieved for {LocalID}");
        /// <summary>
        /// (60001) Information: Terse Object Update Recieved for {LocalID}
        /// </summary>
        [Conditional("DEBUG")]
        public static void TerseObjectUpdate(this ILogger logger, uint localID)
            => TerseObjectUpdate_Action(logger, localID, null!);

        internal static readonly EventId ObjectUpdate_Event
            = new(60002, "ObjectUpdate");
        internal static readonly Action<ILogger, uint, Exception> ObjectUpdate_Action
            = LoggerMessage.Define<uint>(LogLevel.Information,
                ObjectUpdate_Event,
                "Object Update Recieved for {LocalID}");
        /// <summary>
        /// (60002) Information: Object Update Recieved for {LocalID}
        /// </summary>
        [Conditional("DEBUG")]
        public static void ObjectUpdate(this ILogger logger, uint localID)
            => ObjectUpdate_Action(logger, localID, null!);

        internal static readonly EventId ObjectBlockDataUpdate_Event
            = new(60003, "ObjectBlockDataUpdate");
        internal static readonly Action<ILogger, uint, Exception> ObjectBlockDataUpdate_Action
            = LoggerMessage.Define<uint>(LogLevel.Information,
                ObjectBlockDataUpdate_Event,
                "Object Block Data Update Recieved for {LocalID}");
        /// <summary>
        /// (60003) Information: Object Block Data Update Recieved for {LocalID}
        /// </summary>
        [Conditional("DEBUG")]
        public static void ObjectBlockDataUpdate(this ILogger logger, uint localID)
            => ObjectBlockDataUpdate_Action(logger, localID, null!);

        internal static readonly EventId FailedAddingToWorldObjects_Event
            = new(60004, "FailedAddingToWorldObjects");
        internal static readonly Action<ILogger, uint, Exception> FailedAddingToWorldObjects_Action
            = LoggerMessage.Define<uint>(LogLevel.Warning,
                FailedAddingToWorldObjects_Event,
                "Could not add {LocalID} to WorldObjects.");
        /// <summary>
        /// (60004) Warning: Could not add {LocalID} to WorldObjects.
        /// </summary>
        public static void FailedAddingToWorldObjects(this ILogger logger, uint localID)
            => FailedAddingToWorldObjects_Action(logger, localID, null!);

        internal static readonly EventId FailedAddingRegionToWorld_Event
            = new(60005, "FailedAddingRegionToWorld");
        internal static readonly Action<ILogger, UUID, Exception> FailedAddingRegionToWorld_Action
            = LoggerMessage.Define<UUID>(LogLevel.Warning,
                FailedAddingRegionToWorld_Event,
                "Could not add Region {RegionID} to world.");
        /// <summary>
        /// (60005) Warning: Could not add Region {RegionID} to world.
        /// </summary>
        public static void FailedAddingRegionToWorld(this ILogger logger, UUID regionID)
            => FailedAddingRegionToWorld_Action(logger, regionID, null!);

        internal static readonly EventId OrphanDetected_Event
            = new(60006, "OrphanDetected");
        internal static readonly Action<ILogger, uint, uint, Exception> OrphanDetected_Action
            = LoggerMessage.Define<uint, uint>(LogLevel.Information,
                OrphanDetected_Event,
                "Object {LocalID} has a Parent {ParentId} but the parent was not found.");
        /// <summary>
        /// (60006) Information: Object {LocalID} has a Parent {ParentId} but the parent was not found.
        /// </summary>
        [Conditional("DEBUG")]
        public static void OrphanDetected(this ILogger logger, uint localID, uint parentId)
            => OrphanDetected_Action(logger, localID, parentId, null!);

        internal static readonly EventId OrphanReuinited_Event
            = new(60007, "OrphanReuinited");
        internal static readonly Action<ILogger, uint, uint, Exception> OrphanReuinited_Action
            = LoggerMessage.Define<uint, uint>(LogLevel.Information,
                OrphanReuinited_Event,
                "Object {LocalID} has been united with Parent {ParentId}.");
        /// <summary>
        /// (60007) Information: Object {LocalID} has been united with Parent {ParentId}.
        /// </summary>
        [Conditional("DEBUG")]
        public static void OrphanReuinited(this ILogger logger, uint localID, uint parentId)
            => OrphanReuinited_Action(logger, localID, parentId, null!);

        internal static readonly EventId NewRegion_Event
            = new(60008, "NewRegion");
        internal static readonly Action<ILogger, UUID, string, Exception> NewRegion_Action
            = LoggerMessage.Define<UUID, string>(LogLevel.Information,
                NewRegion_Event,
                "New Region Observed {RegionID} {Name}");
        /// <summary>
        /// (60008) Information: New Region Observed {RegionID} {Name}
        /// </summary>
        [Conditional("DEBUG")]
        public static void NewRegion(this ILogger logger, UUID regionID, string name)
            => NewRegion_Action(logger, regionID, name, null!);

    }
}

namespace CrystalFrost.UnityRendering
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId FailedAddingToAllSceneObjects_Event
            = new(70001, "FailedAddingToAllSceneObjects");
        internal static readonly Action<ILogger, uint, Exception> FailedAddingToAllSceneObjects_Action
            = LoggerMessage.Define<uint>(LogLevel.Warning,
                FailedAddingToAllSceneObjects_Event,
                "Failed adding {LocalID} to AllSceneObjects.");
        /// <summary>
        /// (70001) Warning: Failed adding {LocalID} to AllSceneObjects.
        /// </summary>
        public static void FailedAddingToAllSceneObjects(this ILogger logger, uint localID)
            => FailedAddingToAllSceneObjects_Action(logger, localID, null!);

    }
}

namespace CrystalFrost.Scripts
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId LoggingOut_Event
            = new(80001, "LoggingOut");
        internal static readonly Action<ILogger, Exception> LoggingOut_Action
            = LoggerMessage.Define(LogLevel.Information,
                LoggingOut_Event,
                "Attempting Logout");
        /// <summary>
        /// (80001) Information: Attempting Logout
        /// </summary>
        [Conditional("DEBUG")]
        public static void LoggingOut(this ILogger logger)
            => LoggingOut_Action(logger, null!);

        internal static readonly EventId LoggingIn_Event
            = new(80002, "LoggingIn");
        internal static readonly Action<ILogger, string, string, string, Exception> LoggingIn_Action
            = LoggerMessage.Define<string, string, string>(LogLevel.Information,
                LoggingIn_Event,
                "Logging in as {FirstName}.{LastName} to {GridUri}");
        /// <summary>
        /// (80002) Information: Logging in as {FirstName}.{LastName} to {GridUri}
        /// </summary>
        [Conditional("DEBUG")]
        public static void LoggingIn(this ILogger logger, string firstName, string lastName, string gridUri)
            => LoggingIn_Action(logger, firstName, lastName, gridUri, null!);

    }
}

namespace CrystalFrost.Logging
{
	public static partial class GeneratedLoggerMessages
	{
        internal static readonly EventId LMV_Debug_Event
            = new(90001, "LMV_Debug");
        internal static readonly Action<ILogger, string, Exception> LMV_Debug_Action
            = LoggerMessage.Define<string>(LogLevel.Debug,
                LMV_Debug_Event,
                "LMV-Dbug: {Message}");
        /// <summary>
        /// (90001) Debug: LMV-Dbug: {Message}
        /// </summary>
        [Conditional("DEBUG")]
        public static void LMV_Debug(this ILogger logger, string message)
            => LMV_Debug_Action(logger, message, null!);

        internal static readonly EventId LMV_Information_Event
            = new(90002, "LMV_Information");
        internal static readonly Action<ILogger, string, Exception> LMV_Information_Action
            = LoggerMessage.Define<string>(LogLevel.Information,
                LMV_Information_Event,
                "LMV-Info: {Message}");
        /// <summary>
        /// (90002) Information: LMV-Info: {Message}
        /// </summary>
        [Conditional("DEBUG")]
        public static void LMV_Information(this ILogger logger, string message)
            => LMV_Information_Action(logger, message, null!);

        internal static readonly EventId LMV_Warning_Event
            = new(90003, "LMV_Warning");
        internal static readonly Action<ILogger, string, Exception> LMV_Warning_Action
            = LoggerMessage.Define<string>(LogLevel.Warning,
                LMV_Warning_Event,
                "LMV-Warn: {Message}");
        /// <summary>
        /// (90003) Warning: LMV-Warn: {Message}
        /// </summary>
        public static void LMV_Warning(this ILogger logger, string message)
            => LMV_Warning_Action(logger, message, null!);

        internal static readonly EventId LMV_Error_Event
            = new(90004, "LMV_Error");
        internal static readonly Action<ILogger, string, Exception> LMV_Error_Action
            = LoggerMessage.Define<string>(LogLevel.Error,
                LMV_Error_Event,
                "LMV-Fail: {Message}");
        /// <summary>
        /// (90004) Error: LMV-Fail: {Message}
        /// </summary>
        public static void LMV_Error(this ILogger logger, string message)
            => LMV_Error_Action(logger, message, null!);

    }
}


