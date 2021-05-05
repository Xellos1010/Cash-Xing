using UnityEngine;
using System;
namespace Slot_Engine.Matrix
{
    public class SoundManager : MonoBehaviour
    {
        public ReelStripConfigurationObject matrix;
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
            for (int reel = 0; reel < matrix.reelStripManagers.Length; reel++)
            {
                matrix.reelStripManagers[reel].reelStartSpin += SoundManager_reelStartSpin;
                matrix.reelStripManagers[reel].reelStopSpin += SoundManager_reelStopSpin;
            }
            matrix.slotMachineManagers.racking_manager.rackStart += Racking_manager_rackStart;
            matrix.slotMachineManagers.racking_manager.rackEnd += Racking_manager_rackEnd;
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
            audioSource.clip = machineSoundsReference.rollups[matrix.GetRollupIndexFromAmountToRack(amountToRack)];
            audioSource.Play();
        }

        private void SoundManager_reelStopSpin(int reelNumber)
        {
            audioSource.PlayOneShot(machineSoundsReference.reelStops[reelNumber]);
        }

        private void SoundManager_reelStartSpin(int reelNumber)
        {
            audioSource.PlayOneShot(machineSoundsReference.reelStarts[reelNumber]);
        }

        void OnDisable()
        {
            for (int reel = 0; reel < matrix.reelStripManagers.Length; reel++)
            {
                matrix.reelStripManagers[reel].reelStartSpin += SoundManager_reelStartSpin;
            }
        }

        internal void PlayAudioForWinningPayline(WinningPayline winningPayline)
        {
            int winningSymbol = winningPayline.GetWinningWymbol().symbol;
            audioSource.PlayOneShot(matrix.ReturnSymbolSound(winningSymbol));
        }
    }

}