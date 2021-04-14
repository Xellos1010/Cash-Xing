using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LerpToMe : MonoBehaviour
{
    public delegate void LerpComplete();
    public static event LerpComplete lerpComplete;
    /// <summary>
    /// Update Percentage Amount per frame
    /// </summary>
    public float percentCompletePerFrame = .1f;
    /// <summary>
    /// Percentage lerp completion
    /// </summary>
    public float percentComplete = 0;
    /// <summary>
    /// Object Lerping
    /// </summary>
    public Transform objectToLerp
    {
        get
        {
            return _objectToLerp;
        }
        set
        {
            _objectToLerp = value;
            percentComplete = 0;
            if (value != null)
            {
                startPositionCache = value.position;
            }
        }
    }
    /// <summary>
    /// Reference for object to lerp
    /// </summary>
    [SerializeField]
    internal Transform _objectToLerp;
    /// <summary>
    /// New Position of the object to lerp per frame
    /// </summary>
    public Vector3 newPosition;
    /// <summary>
    /// Cache the position of the objectToLerp on assign to lerp from to
    /// </summary>
    public Vector3 startPositionCache;
    // Update is called once per frame
    void Update()
    {
        if (objectToLerp != null)
        {
            if(objectToLerp.position != transform.position)
            {
                percentComplete += percentCompletePerFrame;
                if (percentComplete > 1)
                    percentComplete = 1;
                //So the symbol moves above the hiding symbol object set z depth to top layer
                newPosition = Vector3.Lerp(startPositionCache + (Vector3.back * 10),transform.position, percentComplete);
                objectToLerp.transform.position = newPosition;
            }
            if (percentComplete == 1)
            {
                ObjectLerpComplete();
            }
        }
    }

    private void ObjectLerpComplete()
    {
        objectToLerp.transform.GetComponentInChildren<MeshRenderer>().enabled = false;
        objectToLerp.transform.position = startPositionCache;
        objectToLerp = null;
        startPositionCache = Vector3.zero;
        lerpComplete?.Invoke();
    }

    internal void SetLerpToMe(Transform transform)
    {
        objectToLerp = transform;
    }
}
