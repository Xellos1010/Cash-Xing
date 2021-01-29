using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CycleBannerManager : MonoBehaviour 
{
    private Texture[] _tCycleBanners;
    //need to have cycle banners update based on the state of hte slot engine
    public Texture[] tCycleBanners
    {
        get
        {
            if(_tCycleBanners == null)
                _tCycleBanners = ReturnBanners();
            else if (_tCycleBanners.Length < 1 || _tCycleBanners != ReturnBanners())
            {
                _tCycleBanners = ReturnBanners();
            }
            return _tCycleBanners;
        }
    }
    public int iCycleBannerNumber = -1;
    [Range(1, 60)]
    public int iCycleOnDuration;
    [Range(1, 60)]
    public int iCycleOffDuration;

    [Range(0, 10)]
    public float fFadeLength;

    public bool bCycleEnabled = true;
    public CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }
    private CanvasGroup _canvasGroup;
    private RawImage rawImage
    {
        get
        {
            if (_rawImage == null)
                _rawImage = GetComponent<RawImage>();
            return _rawImage;
        }
    }
    private RawImage _rawImage;

    private bool bBannerObjectEnabled
    {
        get
        {
            if (canvasGroup.alpha < 1)
                return false;
            else
                return true;
        }
    }

    void OnEnable()
    {
        SetDefaultValues();
        StartCoroutine(CycleMessageBanners());
    }

    private void SetDefaultValues()
    {
        iCycleBannerNumber = 0;
        canvasGroup.alpha = 0;
        rawImage.texture = tCycleBanners[iCycleBannerNumber];
        bCycleEnabled = true;
    }

    //To cycle banners have gameobject that is dedicated to showing banners fade the alpha of the material then switch when material alpha is at 1
    private IEnumerator IncrementCycleBanners()
    {
        if (iCycleBannerNumber < 0)
        {
            iCycleBannerNumber = 0;

        }
        LeanTween.alphaCanvas(_canvasGroup, 0, fFadeLength);
        yield return new WaitForSeconds(fFadeLength);

    }

    private void InterruptBannerCycle()
    {
        StopCoroutine("CycleBanners");
    }

    private Texture[] ReturnBanners()
    {
        Texture[] ReturnValue;
        ReturnValue = Resources.LoadAll<Texture>("Skins/" + SlotEngine._instance.eSkin.ToString() + "/"+StateManager.enCurrentMode.ToString()+"/Cycle Banners");
        ReturnValue = Resources.LoadAll<Texture>("Skins/" + SlotEngine._instance.eSkin.ToString() + "/BaseGame/Cycle Banners");
        return ReturnValue;
    }

    private void SwitchCycleBannerTexture(Texture tTexture)
    {
        rawImage.texture = tTexture;
    }


    /*Main Function()
     * 
     */
    IEnumerator CycleMessageBanners()
    {
        //Increment CycleBanner if the banner is off
        while (bCycleEnabled)
        {
            if (!bBannerObjectEnabled)
            {
                IncrementCycleBanners();
                LeanTween.alphaCanvas(_canvasGroup, 1, fFadeLength);
                yield return new WaitForSeconds(fFadeLength);
                yield return new WaitForSeconds(iCycleOnDuration);
            }
            else
            {
                LeanTween.alphaCanvas(_canvasGroup, 0, fFadeLength);
                yield return new WaitForSeconds(fFadeLength);
                yield return new WaitForSeconds(iCycleOffDuration);
            }
        }
    }
}
