using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{ 
    public static AuthManager instance;
    FirebaseAuth auth;

    // 인증을 관리할 객체
    

    // void Awake()
    // { 
    //     // 객체 초기화
    //     if(instance == null)
    //     {
    //         instance = this;
    //         DontDestroyOnLoad(instance);
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }

    // }
    // void Start()
    // {
    //     auth = FirebaseAuth.DefaultInstance;
    // }
    // public void login(string id, string pw)
    // {
    //     // 제공되는 함수 : 이메일과 비밀번호로 로그인 시켜 줌
    //     auth.SignInWithEmailAndPasswordAsync(id, pw).ContinueWith(
    //         task => {
    //             if (task.IsCanceled)
    //             {
    //                 Debug.LogError("로그인 취소");
    //                 return;
    //             }
    //             if (task.IsFaulted)
    //             {
    //                 foreach (var exception in task.Exception.Flatten().InnerExceptions)
    //                 {
    //                     Debug.LogError($"로그인 실패 이유: {exception.Message}");
    //                 }
    //                 return;
    //             }

    //             AuthResult authResult = task.Result;
    //             FirebaseUser newUser = authResult.User;

    //             Debug.Log($"로그인 성공: {newUser.Email}, UID: {newUser.UserId}");
    //         });
    // }
    // public void register(string id, string pw) {
    //     // 제공되는 함수 : 이메일과 비밀번호로 회원가입 시켜 줌
    //     auth.CreateUserWithEmailAndPasswordAsync(id, pw).ContinueWith(
    //         task => 
    //         {
    //             if (task.IsCanceled)
    //             {
    //                 Debug.LogError("회원가입 취소");
    //                 return;
    //             }
    //             if (task.IsFaulted)
    //             {
    //                 foreach (var exception in task.Exception.Flatten().InnerExceptions)
    //                 {
    //                     Debug.LogError($"회원가입 실패 이유: {exception.Message}");
    //                 }
    //                 return;
    //             }

    //             AuthResult authResult = task.Result;
    //             FirebaseUser newUser = authResult.User;

    //             Debug.Log($"회원가입 성공: {newUser.Email}, UID: {newUser.UserId}");
    //         });
    // }
}