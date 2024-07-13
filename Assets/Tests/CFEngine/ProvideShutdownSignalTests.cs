using CrystalFrost;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;

namespace CrystalFrostEngine.Tests
{
    public class ProvideShutdownSignalTests
    {
        [Test]
        public void When_ShutdownSignal_Then_OnShutdownEventIsRaised()
        {
            var mockLock = new Mock<ILogger<ProvideShutdownSignal>>();
            var mockEvents = new Mock<IUnityEditorEvents>();
            var testsubject = new ProvideShutdownSignal(mockLock.Object, mockEvents.Object);

            int eventRaised = 0;
            var eventHandler = new Action(() => eventRaised++);
            testsubject.OnShutdown += eventHandler;

            try
            {
                testsubject.SignalShutdown();
                Assert.AreEqual(1, eventRaised);
                testsubject.SignalShutdown();
                Assert.AreEqual(2, eventRaised);
            }
            finally
            {
                testsubject.OnShutdown -= eventHandler;
            }
        }
    }
}
