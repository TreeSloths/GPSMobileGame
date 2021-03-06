﻿using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using UnityEngine;

public class FirebaseStart : MonoBehaviour
{
    public FirebaseApp FireBaseApp { get; set; }

    public void Update()
    {
        Debug.Log("Script is runned");
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FireBaseApp = FirebaseApp.DefaultInstance;
                Debug.Log("Dependency Avaible");
            }

            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }
}