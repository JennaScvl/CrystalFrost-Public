using Microsoft.Extensions.Logging;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrystalFrost
{
    /// <summary>
    /// Provides an abstraction around Unity Editor Events.
    /// </summary>
    /// 
    /// 
    public interface IUnityEditorEvents
    {
        bool IsEditor { get; }
        event Action BeforeAssemblyReload;
        event Action AfterAssemblyReload;
        event Action HierarchyChanged;
        event Action EditorPaused;
        event Action EditorUnpaused;
        event Action EnteredEditMode;
        event Action EnteredPlayMode;
        event Action ExitingEditMode;
        event Action ExitingPlayMode;
        event Action ProjectChanged;
        event Action Quitting;
        event Action WantsToQuit;
    }

    /// <inheritdoc/>
    public class UnityEditorEvents : IUnityEditorEvents, IDisposable
    {
        private readonly ILogger<UnityEditorEvents> _log;
        public event Action BeforeAssemblyReload;
        public event Action AfterAssemblyReload;
        public event Action HierarchyChanged;
        public event Action EditorPaused;
        public event Action EditorUnpaused;
        public event Action EnteredEditMode;
        public event Action EnteredPlayMode;
        public event Action ExitingEditMode;
        public event Action ExitingPlayMode;
        public event Action ProjectChanged;
        public event Action Quitting;
        public event Action WantsToQuit;
        
        private const bool _isEditor =
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        public bool IsEditor => _isEditor;

        public UnityEditorEvents(ILogger<UnityEditorEvents> log)
        { 
            _log = log;

#if UNITY_EDITOR
            EditorApplication.hierarchyChanged += EditorApplication_hierarchyChanged;
            EditorApplication.pauseStateChanged += EditorApplication_pauseStateChanged;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            EditorApplication.projectChanged += EditorApplication_projectChanged;
            EditorApplication.quitting += EditorApplication_quitting;
            EditorApplication.wantsToQuit += EditorApplication_wantsToQuit;

            AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEvents_beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEvents_afterAssemblyReload;
#endif
        }

#if UNITY_EDITOR
        private void AssemblyReloadEvents_afterAssemblyReload()
        {
            _log.EditorEvent_AfterAssemblyReload();
            AfterAssemblyReload?.Invoke();
        }

        private void AssemblyReloadEvents_beforeAssemblyReload()
        {
            _log.EditorEvent_BeforeAssemblyReload();;
            BeforeAssemblyReload?.Invoke();
        }

        private bool EditorApplication_wantsToQuit()
        {
            _log.EditorEvent_WantsToQuit();
            WantsToQuit?.Invoke();
            return true;
        }

        private void EditorApplication_quitting()
        {
            _log.EditorEvent_Quitting();
            Quitting?.Invoke();
        }

        private void EditorApplication_projectChanged()
        {
            _log.EditorEvent_ProjectChanged();
            ProjectChanged?.Invoke();
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange mode)
        {
            _log.EditorEvent_PlayModeChange(mode);
            var e = mode switch
            {
                PlayModeStateChange.EnteredPlayMode => EnteredPlayMode,
                PlayModeStateChange.EnteredEditMode => EnteredEditMode,
                PlayModeStateChange.ExitingPlayMode => ExitingPlayMode,
                PlayModeStateChange.ExitingEditMode => ExitingEditMode,
                _ => null
            };
            e?.Invoke();
        }

        private void EditorApplication_pauseStateChanged(PauseState state)
        {
            _log.EditorEvent_PauseStateChange(state);
            var e = state switch
            {
                PauseState.Paused => EditorPaused,
                PauseState.Unpaused => EditorUnpaused,
                _ => null
            };
            e?.Invoke();
        }

        private void EditorApplication_hierarchyChanged()
        {
            _log.EditorEvent_HierarchyChanged();
            HierarchyChanged?.Invoke();
        }
#endif

        public void Dispose()
        {
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= EditorApplication_hierarchyChanged;
            EditorApplication.pauseStateChanged -= EditorApplication_pauseStateChanged;
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            EditorApplication.projectChanged -= EditorApplication_projectChanged;
            EditorApplication.quitting -= EditorApplication_quitting;
            EditorApplication.wantsToQuit -= EditorApplication_wantsToQuit;

            AssemblyReloadEvents.beforeAssemblyReload -= AssemblyReloadEvents_beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= AssemblyReloadEvents_afterAssemblyReload;
#endif
            GC.SuppressFinalize(this);
        }
    }
}
