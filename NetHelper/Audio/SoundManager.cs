using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Helper.Audio
{

    public enum SoundTypes
    {
        Effect,
        Music
    }

    public class SoundManager
    {
        #region Properties / Fields
        private float masterVolumeFactor = 1.0f;
        public float MasterVolumeFactor
        {
            get
            {
                return masterVolumeFactor;
            }
            set
            {
                masterVolumeFactor = value;
                SetAllSoundFinalVolumes();
            }
        }
        private float effectVolumeFactor = 1.0f;
        public float EffectVolumeFactor
        {
            get
            {
                return effectVolumeFactor;
            }
            set
            {
                effectVolumeFactor = value;
                SetAllSoundFinalVolumes();
            }
        }
        private float musicVolumeFactor = 1.0f;
        public float MusicVolumeFactor
        {
            get
            {
                return musicVolumeFactor;
            }
            set
            {
                musicVolumeFactor = value;
                SetAllSoundFinalVolumes();
            }
        }
        SortedList<string, Sound> Sounds = new SortedList<string, Sound>();
        #endregion

        #region Initialization
        public SoundManager()
        {

        }

        public void AddSound(string name, SoundEffect effect, bool loop, float volFactor, SoundTypes type)
        {
            if (Sounds.ContainsKey(name))
                return;

            Sound s = new Sound(name, effect, loop, volFactor, type);
            SetSoundFinalVolume(s);
            Sounds.Add(name, s);
        }

        /// <summary>
        /// Adds an effect (no looping)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="effect"></param>
        public void AddSound(string name, SoundEffect effect, SoundTypes type)
        {
            AddSound(name, effect, false, 1.0f, type);
        }


        public void AddSoundEffect(string name, SoundEffect effect, bool loop)
        {
            AddSound(name, effect, loop, 1.0f, SoundTypes.Effect);
        }

        /// <summary>
        /// full volume factor, no looping
        /// </summary>
        /// <param name="name"></param>
        /// <param name="effect"></param>
        public void AddSoundEffect(string name, SoundEffect effect)
        {
            AddSound(name, effect, SoundTypes.Effect);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts playing the sound instance
        /// </summary>
        /// <param name="name"></param>
        public void Play(string name)
        {
            if (!Sounds.ContainsKey(name))
                return;            
            Sounds[name].Play();
        }

        /// <summary>
        /// Stop playing the sound instance
        /// </summary>
        /// <param name="name"></param>
        public void Stop(string name)
        {
            if (!Sounds.ContainsKey(name))
                return;
            Sounds[name].Stop();
        }

        /// <summary>
        /// sets
        /// </summary>
        /// <param name="name"></param>
        /// <param name="volumeFactor">Between 0.0 and 1.0?</param>
        public void SetVolume(string name, float volumeFactor)
        {
            if (!Sounds.ContainsKey(name))
                return;
            Sounds[name].VolumeFactor = volumeFactor;
            SetSoundFinalVolume(Sounds[name]);
        }

        /// <summary>
        /// Stops playback of all sounds
        /// </summary>
        public void StopAll()
        {
            foreach (Sound s in Sounds.Values)
                s.Stop();
            
        }

        /// <summary>
        /// Scales the volume of a sound by the Volume factor for its SoundType and the Master volumen Factor.
        /// </summary>
        /// <param name="soundVolFactor"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private float GetFinalVolume(float soundVolFactor, SoundTypes type)
        {
            float final=1.0f;
            switch (type)
            {
                case SoundTypes.Effect:
                    final *= EffectVolumeFactor;
                    break;
                case SoundTypes.Music:
                    final *= EffectVolumeFactor;
                    break;
                default:
                    break;
            }
            final *= soundVolFactor * MasterVolumeFactor;
            return final;
        }
        
        /// <summary>
        /// Iterates through all sounds, updating the volume at which the instance will play
        /// </summary>
        private void SetAllSoundFinalVolumes()
        {
            foreach (Sound s in Sounds.Values)
                SetSoundFinalVolume(s);
        }

        /// <summary>
        /// Sets the final volume at which the instance will play
        /// </summary>
        /// <param name="s"></param>
        private void SetSoundFinalVolume(Sound s)
        {
            s.SetVolume(GetFinalVolume(s.VolumeFactor, s.type));
        }
        #endregion
    }
}
