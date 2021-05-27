using System;
using UnityEngine;
namespace BoomSports.Prototype.Managers
{
    [Serializable]
    public abstract class BaseBoomSportsManager : MonoBehaviour
    {
        public StripConfigurationObject configurationObject
        {
            get
            {
                if (_configurationObject == null)
                    _configurationObject = GameObject.FindObjectOfType<StripConfigurationObject>();
                return _configurationObject;
            }
        }
        [SerializeField]
        public StripConfigurationObject _configurationObject;
    }
}