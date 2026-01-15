using System;
using System.Collections;
using UnityEngine;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] CanvasGroup _lobbyPanel;
    [SerializeField] CanvasGroup _roleSelectPanel;
    [SerializeField] float _fadeDuration = 0.25f;
    [SerializeField] float _emptyDuration = 0.1f;
    bool _isFading;


    private void Awake()
    {
        _isFading = false;
        SetPanelAlpha(_lobbyPanel, true);
        SetPanelAlpha(_roleSelectPanel, false);
        SetPanelInteract(_lobbyPanel, true);
        SetPanelInteract(_roleSelectPanel, false);
        _lobbyPanel.gameObject.SetActive(true);
        _roleSelectPanel.gameObject.SetActive(true);
    }

    public void OnPlayButtonClick()
    {        
        ChangePanel(_lobbyPanel, _roleSelectPanel);        
    }

    public void OnBackToLobbyButtonClick()
    {
        ChangePanel(_roleSelectPanel, _lobbyPanel);
    }
    
    private void ChangePanel(CanvasGroup panelToOff, CanvasGroup panelToOn)
    {
        if (_isFading)
        {
            return;
        }
        StartCoroutine(PanelChangeRoutine(panelToOff, panelToOn));
    }


    IEnumerator PanelChangeRoutine(CanvasGroup outPanel, CanvasGroup inPanel)
    {        
        _isFading = true;

        //원래 알파값 정상적으로 되어있겠지만, 혹시 몰라 이중 안전처리
        SetPanelAlpha(outPanel, true); 
        SetPanelAlpha(inPanel, false); 

        //페이딩 도중에는 양 패널 상호작용 전부 막아두기
        SetPanelInteract(outPanel, false);
        SetPanelInteract(inPanel, false);
        

        float t1 = 0f;
        float t2 = 0f;

        while (t1 < _fadeDuration)
        {
            t1 += Time.deltaTime;
            float ratio = t1 / _fadeDuration;

            outPanel.alpha = Mathf.Lerp(1f, 0f, ratio);
            yield return null;
        }
        SetPanelAlpha(outPanel, false);

        yield return new WaitForSeconds(_emptyDuration);

        while (t2 < _fadeDuration)
        {
            t2 += Time.deltaTime;
            float ratio = t2 / _fadeDuration;
            
            inPanel.alpha = Mathf.Lerp(0f, 1f, ratio);
            yield return null;
        }        
        SetPanelAlpha(inPanel, true);

        //페이딩 끝나면 새 패널만 상호작용 켜기
        SetPanelInteract(inPanel, true);


        _isFading = false;
    }


    private void SetPanelInteract(CanvasGroup canvasGroup, bool visible)
    {        
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void SetPanelAlpha(CanvasGroup canvasGroup, bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;        
    }
}
