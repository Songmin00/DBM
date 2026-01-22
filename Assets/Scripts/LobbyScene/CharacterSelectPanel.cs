using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPanel : MonoBehaviour
{
    [SerializeField] GameObject _buttonPrefab;
    [SerializeField] List<GameObject> _characterPrefabs;
    [SerializeField] Vector3 _previewPos = new Vector3(1, 0, -8);
    [SerializeField] Quaternion _previewRot = Quaternion.Euler(0, 180, 0);

    
    List<Button> _buttons;
    GameObject _currentPreview;
    

    private void Awake()
    {
        _buttons = new List<Button>();
        if (_buttonPrefab == null)
        {
            Debug.Log("CharacterSelectPanel에 버튼 프리팹을 할당해주세요.");
        }
        if (_characterPrefabs.Count == 0)
        {
            Debug.Log("캐릭터 프리뷰를 위해 CharacterSelectPanel에 캐릭터 프리팹을 할당해주세요");
            return;
        }


        for (int i = 0; i < _characterPrefabs.Count; i++)
        {
            int index = i;
            Button button = Instantiate(_buttonPrefab, transform).GetComponent<Button>();
            
            button.onClick.AddListener(() => OnCharacterSelectButtonClick(_characterPrefabs[index]));            
            _buttons.Add(button);            
        }
    }


    public void OnCharacterSelectButtonClick(GameObject character)
    {
        if (_currentPreview != null)
        {
            Destroy(_currentPreview);
        }        
        CharacterStateManager.Instance.SetCharacterPrefab(character);
        _currentPreview = Instantiate(character, _previewPos, _previewRot);
    }

    public void CancelSelect()
    {
        if (_currentPreview != null)
        {
            Destroy(_currentPreview);
        }
    }
}
