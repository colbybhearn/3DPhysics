using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Helper.Audio
{
    class Sound
    {
        #region Properties / Fields
        string Name;
        SoundEffectInstance Instance;
        public float VolumeFactor;
        public SoundTypes type;
        #endregion

        #region Initialization
        public Sound(string name, SoundEffect effect, bool loop, float volFactor, SoundTypes type)
        {
            Name = name;
            Instance = effect.CreateInstance();
            Instance.IsLooped = loop;
            VolumeFactor = volFactor;
            this.type = type;
        }
        #endregion

        #region Methods

        public void Play()
        {
            Instance.Play();
        }

        /// <summary>
        /// Sets the volume at which the sound plays
        /// </summary>
        /// <param name="finalVolume">The pre-scaled volume between 0.0f and 1.0f, (scaled by SoundTypeFactor and MasterVolumeFactor)</param>
        public void SetVolume(float finalVolume)
        {
            Instance.Volume = finalVolume;
        }

        public void Stop()
        {
            Instance.Stop();
        }
        #endregion
    }
}
