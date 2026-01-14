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

    public void SetUserData()
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
        var dbTask = DatabaseReference.Child("users").Child(User.UserId).Child("bp").SetValueAsync(bp);
        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.Log($"Bp 업데이트 실패. 사유 : {dbTask.Exception}");
        }
        else
        {
            Debug.Log($"Bp 저장 완료");
        }
    }
    

    public void LoadFromDB()
    {
        if (User == null || DatabaseReference == null)
        {
            Debug.LogError("SaveManager에 플레이어 계정이 연결되지 않았습니다.");
            return;
        }

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
            FirstInit();
            Debug.Log($"불러올 데이터가 없습니다.");
        }
        else
        {
            DataSnapshot snapshot = dbTask.Result;
            
            PlayerData.BloodPoint = snapshot.Child("bp").Exists ? (int)snapshot.Child("bp").Value : 0;

            Debug.Log($"Bp 로딩 성공!");
        }

    }

    private void FirstInit()
    {
        PlayerData.BloodPoint = 0;
    }
}
