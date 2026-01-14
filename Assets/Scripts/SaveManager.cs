using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SaveManager : Singleton<SaveManager>
{
    public DatabaseReference DatabaseReference {  get; set; }
    public FirebaseUser User {  get; set; }
    public PlayerData PlayerData { get; set; }

    protected override void Awake()
    {
        base.Awake();
        PlayerData = new PlayerData();
    }

    private void Start()
    {
        this.DatabaseReference = AuthManager.DbRef;
        this.User = AuthManager.User;
    }


    public void SaveToDB()
    {
        StartCoroutine(UpdateBloodPoint(PlayerData.BloodPoint));
    }

    private IEnumerator UpdateBloodPoint(int bp)
    {
        var dbTask = DatabaseReference.Child("users").Child(User.UserId).Child("bp").SetValueAsync(bp); //루트 정보 안에 users라는 이름으로 폴더를 생성, users 폴더 안에 UserId 폴더를 만들고 안에 또 money 폴더를 생성(이미 있다면 덮어쓰기). 그리고 값을 설정
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted); //DB에 저장 완료될 때까지 코루틴으로 기다림

        if (dbTask.Exception != null) //문제가 있다면
        {
            Debug.Log($"Bp 업데이트 실패. 사유 : {dbTask.Exception}");
        }
        else //문제 없다면
        {
            Debug.Log($"Bp 저장 완료");
        }
    }
    

    public void LoadFromDB()
    {
        StartCoroutine(LoadUserData());        
    }

    private IEnumerator LoadUserData()
    {
        var dbTask = DatabaseReference.Child("users").Child(User.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null) //예외 발생 시
        {
            Debug.Log($"데이터 로딩 실패. 사유 : {dbTask.Exception}");
        }
        else if (dbTask.Result.Value == null) //불러올 데이터가 없다면
        {
            Debug.Log($"불러올 데이터가 없습니다.");
        }
        else
        {
            DataSnapshot snapshot = dbTask.Result; //자식딸린 루트를 받아왔기 때문에 이걸 스냅샷이라는 큰 틀에 담아서 사용
            
            PlayerData.BloodPoint = snapshot.Child("bp").Exists ? (int)snapshot.Child("bp").Value : 0; //존재하면 로딩, 없으면 0            

            Debug.Log($"Bp 로딩 성공!");
        }

    }
}
