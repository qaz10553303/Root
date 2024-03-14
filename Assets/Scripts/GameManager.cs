using AYellowpaper.SerializedCollections;
using JetBrains.Rider.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStatus
{
    NotStarted = 0,
    Started = 1,
    End = 2,
}

public class GameManager : SingletonBase<GameManager>
{
    private float _water;
    private float _scrollSpd;
    private float _waterDropPerSec;
    private float _time;
    private float _divedDepth;
    private GameStatus _gameStatus;
    private List<RootController> _rootControllerList = new List<RootController>();

    public Transform RootInstantiateRoot;
    public GameObject RootEmissionPrefab;
    public GameObject RootWithControllerPrefab;
    public Camera Camera;

    //[SerializedDictionary("Element Type", "Description")]
    //public Dictionary<int, List<GameObject>> LevelPiecePrefabDict;

    public void StartGame()
    {
        if (_gameStatus != GameStatus.NotStarted)
        {
            return;
        }
        Debug.LogError("Game Start!");

        InitValues();

        var controller = InstantiateNewRootController(50,GameSetting.root1LeftKey,GameSetting.root1RightKey);
        _rootControllerList.Add(controller);

        _gameStatus = GameStatus.Started;
    }

    public void EndGame()
    {
        if (_gameStatus != GameStatus.Started)
        {
            return;
        }
        Debug.LogError("Game End!");
        _gameStatus = GameStatus.End;
    }


    private void Update()
    {
        if(_gameStatus != GameStatus.Started)
        {
            return;
        }

        _time += Time.deltaTime;
        //_water -= Time.deltaTime * _waterDropPerSec;
        _divedDepth += Time.deltaTime * _scrollSpd;

        for (int i = 0; i < _rootControllerList.Count; i++)
        {
            var controller = _rootControllerList[i];
            controller.Tick();
        }

        //MoveCamera();

        RefreshUI();

        if (GameOverCheck())
        {
            EndGame();
        }
    }

    private void MoveCamera()
    {
        var xPos = FindCameraTowardsXPos();
        //var x = GameConfig.CAMERA_X_MOVE_SPD * Time.deltaTime;
        var yPos = Camera.transform.position.y - (GameConfig.GAME_START_SCROLL_SPD * Time.deltaTime);
        Camera.transform.position = new Vector3(xPos, yPos,-10);
    }

    private void RefreshUI()
    {
        UIManager.Instance.RefreshDivedMeterText(_divedDepth);
        UIManager.Instance.RefreshWater(_water,GameConfig.MAX_WATER);
    }

    private bool GameOverCheck()
    {
        if (_water <= 0)
        {
            return true;
        }
        return false;
    }

    private void InitValues()
    {
        _rootControllerList = new List<RootController>();
        _gameStatus = GameStatus.NotStarted;
        _time = 0;
        _divedDepth = 0;
        _water = GameConfig.GAME_START_WATER;
        _scrollSpd = GameConfig.GAME_START_SCROLL_SPD;
        _waterDropPerSec = GameConfig.GAME_START_WATER;
        Camera.transform.position = GameConfig.CAMERA_INIT_POS;
    }

    private RootController InstantiateNewRootController(float posXPercInScreen, KeyCode leftKey, KeyCode rightKey)
    {
        var go = GameObject.Instantiate(RootWithControllerPrefab,RootInstantiateRoot);
        var controller = go.GetComponentInChildren<RootController>();
        var initPos = new Vector3(GetPosXByPercInScreen(posXPercInScreen), 6,0);
        controller.Init(leftKey, rightKey, initPos,_scrollSpd);
        return controller;
    }

    private static float GetPosXByPercInScreen(float posXPercInScreen)
    {
        var xLocalPos = Screen.width * (posXPercInScreen - 50) / 100;
        return xLocalPos;
    }

    public float GetCurrentDepth()
    {
        return _divedDepth;
    }

    private float FindCameraTowardsXPos()
    {
        float minX = 0;
        float maxX = 0;
        foreach (var controller in _rootControllerList)
        {
            var xPos = controller.GameObjTrans.transform.position.x;
            if (xPos < minX)
            {
                minX = xPos;
            }
            if (xPos > maxX)
            {
                maxX = xPos;
            }
        }
        return (minX + maxX) / 2;
    }
}
