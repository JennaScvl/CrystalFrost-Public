//////////////////////////////////////////////////////
// MK Install Wizard Configuration            		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2021 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
namespace MK.Glow.Editor.InstallWizard
{
    public enum RenderPipeline
    {
        [UnityEngine.InspectorName("Built-in Legacy")]
        Built_in_Legacy = 0,
        [UnityEngine.InspectorName("Built-in + Post Processing Stack")]
        Built_in_PostProcessingStack = 1,
        //Lightweight = 2,
        Universal = 3,
        [UnityEngine.InspectorName("High Definition")]
        High_Definition = 4
    }
    
    [System.Serializable]
    public class ExampleContainer
    {
        public string name = "";
        public UnityEngine.Object scene = null;
        public UnityEngine.Texture2D icon = null;

        public void DrawEditorButton()
        {
            if(UnityEngine.GUILayout.Button(icon, UnityEngine.GUILayout.Width(64), UnityEngine.GUILayout.Height(64)))
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(UnityEditor.AssetDatabase.GetAssetOrScenePath(scene));
        }
    }
}
#endif