using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIController : MonoBehaviour
{
    [Header("UI 교체 연출 소요 시간")]
    [SerializeField] float _fadeDuration = 0.25f;
    [SerializeField] float _emptyDuration = 0.1f;

    [Header("좌상단 플레이 패널")]
    [SerializeField] CanvasGroup _lobbyPanel;
    [SerializeField] CanvasGroup _roleSelectPanel;
    
    bool _isFading;

    [Header("캐릭터 선택 버튼 패널 상위 오브젝트")]
    [SerializeField] CanvasGroup _killerSelectObject;
    [SerializeField] CanvasGroup _survivorSelectObject;

    [Header("캐릭터 선택 버튼 패널")]
    [SerializeField] CharacterSelectPanel _killerSelectPanel;
    [SerializeField] CharacterSelectPanel _survivorSelectPanel;


    [Header("매칭 상태 알림 패널")]
    [SerializeField] GameObject _matchingStatePanel;
    [SerializeField] TextMeshProUGUI _matchingStateText;
    [SerializeField] Button _playButton;
    [SerializeField] Button _killerPlayButton;
    [SerializeField] Button _survivorPlayButton;
    [SerializeField] GameObject _cancelButton;

    private void Awake()
    {
        _isFading = false;
        SetPanelAlpha(_lobbyPanel, true);
        SetPanelAlpha(_roleSelectPanel, false);

        SetPanelAlpha(_killerSelectObject, false);
        SetPanelAlpha(_survivorSelectObject, false);
        
        SetPanelInteract(_lobbyPanel, true);
        SetPanelInteract(_roleSelectPanel, false);

        SetPanelInteract(_killerSelectObject, false);
        SetPanelInteract(_survivorSelectObject, false);

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

    public void SetMatchingUIWhileMatching(bool isMatching)
    {
        _matchingStatePanel.SetActive(isMatching);
        _cancelButton.SetActive(isMatching);
        _playButton.interactable = !isMatching;
        _killerPlayButton.interactable = !isMatching;
        _survivorPlayButton.interactable = !isMatching;
    }

    public void SetMatchingStateText(string text)
    {
        _matchingStateText.text = text;
    }

    public void SetKillerSelectUI()
    {
        ChangePanel(_roleSelectPanel, _killerSelectObject);
    }
    public void SetSurvivorSelectUI()
    {
        ChangePanel(_roleSelectPanel, _survivorSelectObject);
    }



    public void SetReturnUIByKiller()
    {
        _killerSelectPanel.CancelSelect();
        ChangePanel(_killerSelectObject, _roleSelectPanel);
    }
    public void SetReturnUIBySurvivor()
    {
        _survivorSelectPanel.CancelSelect();
        ChangePanel(_survivorSelectObject, _roleSelectPanel);
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
