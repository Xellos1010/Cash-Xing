using UnityEngine;
using System;
namespace BoomSports.Prototype.Managers
{
    public class SoundManager : BaseBoomSportsManager
    {
        public MachineSoundsReferenceScriptableObject machineSoundsReference;

        public AudioSource audioSource
        {
            get
            {
                if(_audioSource == null)
                {
                    _audioSource = Camera.main.GetComponent<AudioSource>();
                }
                return _audioSource;
            }
        }
        public AudioSource _audioSource;

        //Hook into all events
        void OnEnable()
        {
            //Setup Reel Start and Stop Spin
            for (int reel = 0; reel < configurationObject.configurationGroupManagers.Length; reel++)
            {
                configurationObject.configurationGroupManagers[reel].objectGroupStartSpin += SoundManager_reelStartSpin;
                configurationObject.configurationGroupManagers[reel].objectGroupEndSpin += SoundManager_reelStopSpin;
            }
            configurationObject.managers.rackingManager.rackStart += Racking_manager_rackStart;
            configurationObject.managers.rackingManager.rackEnd += Racking_manager_rackEnd;
        }

        private void Racking_manager_rackEnd()
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
            audioSource.PlayOneShot(machineSoundsReference.rollupEnd);
        }

        private void Racking_manager_rackStart(double amountToRack)
        {
            audioSource.loop = true;
            audioSource.clip = machineSoundsReference.rollups[configurationObject.GetRollupIndexFromAmountToRack(amountToRack)];
            audioSource.Play();
        }

        private void SoundManager_reelStopSpin(int reelNumber)
        {
            //audioSource.PlayOneShot(machineSoundsReference.reelStops[reelNumber]);
        }

        private void SoundManager_reelStartSpin(int reelNumber)
        {
            //audioSource.PlayOneShot(machineSoundsReference.reelStarts[reelNumber]);
        }

        void OnDisable()
        {
            //Setup Reel Start and Stop Spin
            try
            {
                for (int reel = 0; reel < configurationObject.configurationGroupManagers.Length; reel++)
                {
                    configurationObject.configurationGroupManagers[reel].objectGroupStartSpin -= SoundManager_reelStartSpin;
                    configurationObject.configurationGroupManagers[reel].objectGroupEndSpin -= SoundManager_reelStopSpin;
                }
                configurationObject.managers.rackingManager.rackStart -= Racking_manager_rackStart;
                configurationObject.managers.rackingManager.rackEnd -= Racking_manager_rackEnd;
            }
            catch
            {
                Debug.LogWarning("Sound Manager deregister from events issue");
            }
        }

        internal void PlayAudioForWinningPayline(WinningPayline winningPayline)
        {
            int winningSymbol = winningPayline.GetWinningWymbol().symbol;
            audioSource.PlayOneShot(configurationObject.ReturnSymbolSound(winningSymbol));
        }
    }

}