using UnityEngine;

//로비에서 고른 캐릭터 정보를 인게임으로 가져가기 위한 클래스
public class CharacterStateManager : Singleton<CharacterStateManager>
{
    PlayerType _playerType; //킬러인지 생존자인지 구분
    GameObject _characterPrefab;

    public void SetPlayerType(PlayerType type) //인풋 설정을 위한 타입 보관
    {
        _playerType = type;
    }

    public void SetCharacterPrefab(GameObject prefab) //캐릭터 생성을 위한 프리팹 보관
    {
        _characterPrefab = prefab;
    }
}
