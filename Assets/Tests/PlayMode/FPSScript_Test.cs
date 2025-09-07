using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Collections;
using FBCapture;
using NUnit.Framework;

namespace CrystalFrost.Tests.PlayMode
{
    public class FPSScript_Test
    {
        [UnityTest]
        public IEnumerator FPSScript_UsesUnscaledDeltaTime()
        {
            // Arrange
            var go = new GameObject();
            var text = go.AddComponent<Text>();
            var fpsScript = go.AddComponent<FPSScript>();

            // Set a custom font for the Text component to avoid errors
            text.font = Font.CreateDynamicFontFromOSFont("Arial", 14);

            Time.timeScale = 0.5f;

            // Act
            // Wait for a few frames to allow the script to update
            yield return null;
            yield return null;
            yield return null;

            // Assert
            // The script should use Time.unscaledDeltaTime, so the displayed
            // millisecond value should be close to Time.unscaledDeltaTime * 1000.
            // If it were using Time.deltaTime, it would be about half of that.

            // We need to parse the text to get the msec value
            string[] parts = text.text.Split(' ');
            float msec = float.Parse(parts[0]);

            // We allow for a generous tolerance due to the smoothing algorithm
            Assert.IsTrue(msec > Time.unscaledDeltaTime * 1000 * 0.5f, "FPS script is likely using scaled time!");

            // Cleanup
            Time.timeScale = 1.0f;
            Object.Destroy(go);
        }
    }
}
