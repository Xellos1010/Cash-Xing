using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BridgeAnimatorTriggerSignaler : MonoBehaviour
{
    
    /// <summary>
    /// public reference for children bridge animators
    /// </summary>
    public Animator[] bridgeAnimators
    {
        get
        {
            if (_bridgeAnimators?.Length < 1 || StaticUtilities.ContainsNull<Animator>(_bridgeAnimators))
            {
                _bridgeAnimators = GetComponentsInChildren<Animator>();
            }
            return _bridgeAnimators;
        }
    }
    public Animator?[] _bridgeAnimators;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
