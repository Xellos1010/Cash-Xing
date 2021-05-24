using System;
using UnityEngine;
namespace BoomSports.Prototype.Managers
{
    [Serializable]
    public abstract class BaseBoomSportsManager : MonoBehaviour
    {
        [SerializeField]
        public StripConfigurationObject configurationObject;
    }
}