using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public delegate void LerpEvent();
public delegate void LerpEventRaiseWin(double winAmount);
public delegate void LerpEventRaiseWinLerpedObject(LerpableObject objectLerped);
[Serializable]
public class LerpableObject
{
    public event LerpEventRaiseWin lerpComplete;
    public event LerpEventRaiseWinLerpedObject lerpCompleteObjectReturn;
    [SerializeField]
    public Transform objectToLerp;
    public float percentComplete;
    public Vector3 startPositionCache;
    internal float winAmount;

    public LerpableObject(Transform objectToLerp, float percentComplete)
    {
        this.objectToLerp = objectToLerp;
        this.percentComplete = percentComplete;
        startPositionCache = this.objectToLerp.position;
        percentComplete = 0;
    }

    internal bool isFinishedLerping()
    {
        return percentComplete >= 1;
    }

    internal void LerpObject(float percentCompletePerFrame, Vector3 endPosition)
    {
        if (objectToLerp != null)
        {
            Debug.Log($"Lerping Object {objectToLerp.gameObject.name} Adding {percentCompletePerFrame} complete to {percentComplete}");
            //So the symbol moves above the hiding symbol object set z depth to top layer
            objectToLerp.transform.position = Vector3.Lerp(startPositionCache + (Vector3.back * 30), endPosition, percentComplete);
            if (percentComplete >= 1)
            {
                ResetObjectToLerp();
            }
        }
        else
        {
            Debug.LogWarning("No object to lerp but trying");
        }
    }

    private void ResetObjectToLerp()
    {
        objectToLerp.transform.GetComponentInChildren<MeshRenderer>().enabled = false;
        objectToLerp.transform.position = startPositionCache;
        lerpComplete?.Invoke(winAmount);
        lerpCompleteObjectReturn?.Invoke(this);
    }
}
public class LerpToMe : MonoBehaviour
{
    public event LerpEvent lerpComplete;
    /// <summary>
    /// Update Percentage Amount per frame
    /// </summary>
    public float percentCompletePerFrame = .1f;

    /// <summary>
    /// Object Lerping
    /// </summary>
    public List<LerpableObject> objectsToLerp;
    /// <summary>
    /// New Position of the object to lerp per frame
    /// </summary>
    public Vector3 newPositionTemp;
    /// <summary>
    /// Cache the position of the objectToLerp on assign to lerp from to
    /// </summary>
    // Update is called once per frame
    int lerpFinishCount = 0;
    LerpableObject temp;
    void Update()
    {
        if (objectsToLerp != null)
        {
            if (objectsToLerp.Count > 0)
            {
                lerpFinishCount = 0;
                for (int lerpObject = 0; lerpObject < objectsToLerp.Count; lerpObject++)
                {
                    if (!objectsToLerp[lerpObject].isFinishedLerping())
                    {
                        temp = objectsToLerp[lerpObject];
                        temp.percentComplete += percentCompletePerFrame;
                        if (temp.percentComplete > 1)
                            temp.percentComplete = 1;
                        objectsToLerp[lerpObject] = temp;
                        objectsToLerp[lerpObject].LerpObject(percentCompletePerFrame,transform.position);
                    }
                    else
                    {
                        lerpFinishCount+=1;
                        if (lerpFinishCount == objectsToLerp.Count - 1)
                        {
                            ObjectLerpComplete();
                        }
                    }
                }
            }
        }
    }

    private void ObjectLerpComplete()
    {
        Debug.Log("Object Lerp Complete");
        lerpComplete?.Invoke();
    }

    internal void SetLerpToMe(Transform transform)
    {
        if (objectsToLerp != null)
            objectsToLerp.Clear();
        AddLerpToMeObject(transform);
    }

    internal LerpableObject AddLerpToMeObject(Transform transform)
    {
        if (objectsToLerp == null)
            objectsToLerp = new List<LerpableObject>();
        LerpableObject returnObject = new LerpableObject(transform, 0);
        returnObject.lerpCompleteObjectReturn += ReturnObject_lerpCompleteObjectReturn; ;
        objectsToLerp.Add(returnObject);
        return returnObject;
    }

    private void ReturnObject_lerpCompleteObjectReturn(LerpableObject objectLerped)
    {
        Debug.Log($"Lerp Complete with sub object {objectLerped.objectToLerp.name} Removing from list");
        objectLerped.lerpCompleteObjectReturn -= ReturnObject_lerpCompleteObjectReturn;
        objectsToLerp.Remove(objectLerped);
    }
}
