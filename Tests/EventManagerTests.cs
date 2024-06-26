using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using Cysharp.Threading.Tasks;

namespace Conkist.GDK.Tests
{
    /// <summary>
    /// Unit tests for the EventManager class.
    /// </summary>
    public class EventManagerTests
    {
        private struct TestEvent
        {
            public string Message;
        }

        /// <summary>
        /// A test listener class that subscribes to TestEvent.
        /// </summary>
        private class TestEventListener : EventListener<TestEvent>
        {
            public string ReceivedMessage = null;

            public void OnEventCallback(TestEvent eventType)
            {
                ReceivedMessage = eventType.Message;
            }
        }

        /// <summary>
        /// Tests that an eventlistener can be added and removed correctly from event manager.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator EventManager_CanAddAndRemoveListener()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                TestEventListener listener = new TestEventListener();
                var eventType = typeof(TestEvent);

                // Act
                EventManager.AddListener(listener);
                Assert.IsTrue(EventManager.Test_SubscriptionExists(eventType, listener), "Listener should be added.");

                EventManager.RemoveListener(listener);
                Assert.IsFalse(EventManager.Test_SubscriptionExists(eventType, listener), "Listener should be removed.");

                await UniTask.Yield();
            });
        }
        
        /// <summary>
        /// Tests that an event can be triggered and that a listener correctly receives the event.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator EventManager_CanTriggerEvent()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                TestEventListener listener = new TestEventListener();
                EventManager.AddListener(listener);
                string testMessage = "Test Event";

                // Act
                var testEvent = new TestEvent { Message = testMessage };
                EventManager.TriggerEvent(testEvent);

                // Allow some time for the event to process
                await UniTask.Yield();

                // Assert
                Assert.AreEqual(testMessage, listener.ReceivedMessage, "Listener should receive the triggered event.");

                EventManager.RemoveListener(listener);
            });
        }

        /// <summary>
        /// Tests that triggering an event with no listeners does not cause any issues.
        /// </summary>
        /// <returns>An IEnumerator for UnityTest using UniTask.</returns>
        [UnityTest]
        public IEnumerator EventManager_CanHandleNoListeners()
        {
            return UniTask.ToCoroutine(async () =>
            {
                // Arrange
                var testEvent = new TestEvent { Message = "No Listeners" };

                // Act
                // Trigger an event with no listeners
                EventManager.TriggerEvent(testEvent);
                await UniTask.Yield();

                // No assert needed here, we just want to ensure that no exception is thrown.
                Assert.Pass();
            });
        }
    }
}
