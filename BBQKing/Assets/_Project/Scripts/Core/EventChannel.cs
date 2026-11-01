using System;
using UnityEngine;

namespace BBQKing.Core
{
    /// <summary>
    /// A message board that one system posts to and others listen to, without either needing a
    /// reference to the other. For example, the player's interactor posts "show this prompt" and
    /// the UI listens; neither knows the other exists. This is how CLAUDE.md asks systems to talk.
    ///
    /// Each channel is an asset in Data/Events. Drag the same asset into the poster and the
    /// listeners in the Inspector to connect them. Listeners subscribe in OnEnable and unsubscribe
    /// in OnDisable, so a destroyed object is never called.
    ///
    /// Unity cannot save a generic class as an asset directly, so each kind of channel is a small
    /// subclass, such as <see cref="StringEventChannel"/>.
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        /// <summary>Called whenever something raises this channel.</summary>
        public event Action<T> Raised;

        public void Raise(T value) => Raised?.Invoke(value);
    }
}
