using JetBrains.Rider.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GameStatus
{
    NotStarted = 0,
    Prepared = 1,
    Started = 2,
    End = 3,
}


public class GameManager : SingletonBase<GameManager>
{
    private float _water;
    private float _scrollSpd;
    private float _waterDropPerSec;
    private float _time;
    private float _currDepth;
    private int _currCase;
    private GameStatus _gameStatus;
    private List<RootController> _rootControllerList = new List<RootController>();

    private bool _shouldStartFollowing = false;
    private float _followTimer = 0f;



    public Transform ItemInstantiateRoot;
    public Transform RootInstantiateRoot;
    public GameObject RootEmissionPrefab;
    public GameObject RootWithControllerPrefab;
    public Camera Camera;

    private SerializedItemCollectionDict _itemCollectionDict;

    public void StartGame()
    {
        if (_gameStatus != GameStatus.NotStarted)
        {
            return;
        }
        Debug.LogError("Preparing Game...");

        _gameStatus = GameStatus.Prepared;

        StartCoroutine(StartGameAfterPreparation(1.5f));
    }

    private IEnumerator StartGameAfterPreparation(float duration)
    {
        float elapsedTime = 0;

        Vector3 startPos = Camera.transform.position;
        Vector3 endPos = new Vector3(Camera.transform.position.x, Camera.transform.position.y - GameConfig.CAMERA_PREP_MOVE_SPEED * duration, Camera.transform.position.z);

        while (elapsedTime < duration)
        {
            Camera.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.transform.position = endPos;

        Debug.LogError("Game Start!");
        InitValues();

        var controller = InstantiateNewRootController(50, GameSetting.root1LeftKey, GameSetting.root1RightKey);
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

        StartCoroutine(MoveCameraUpwards(5.0f, _currDepth));
    }

    private IEnumerator MoveCameraUpwards(float speed, float distance)
    {
        Vector3 startPos = Camera.transform.position;
        Vector3 endPos = new Vector3(Camera.transform.position.x, Camera.transform.position.y + distance, Camera.transform.position.z);

        float totalDuration = distance / speed;
        float elapsedTime = 0;

        while (elapsedTime < totalDuration)
        {
            Camera.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / totalDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.transform.position = endPos;
    }
    private void Start()
    {
        _itemCollectionDict = GetComponent<SerializedItemCollectionDict>();
    }

    private void Update()
    {
        if (_gameStatus != GameStatus.Started)
        {
            return;
        }

        _time += Time.deltaTime;
        _water -= Time.deltaTime * _waterDropPerSec;
        _currDepth += Time.deltaTime * _scrollSpd;


        for (int i = 0; i < _rootControllerList.Count; i++)
        {
            var controller = _rootControllerList[i];
            controller.Tick();
        }

        HandleCameraFollow();

        int newCase = GetMaxCaseNeedForGenerate(_currDepth);
        while (newCase > _currCase)
        {
            _currCase++;
            GenerateNewCase(_currCase);
        }

        RefreshUI();

        if (GameOverCheck())
        {
            EndGame();
        }
    }

    private void GenerateNewCase(int newCase)
    {
        if(_itemCollectionDict.TryGetValue(newCase,out var itemCollection))
        {
            if (itemCollection.Collections.Count > 0)
            {
                int collectionIndex = Random.Range(0, itemCollection.Collections.Count);
                var prefab = itemCollection.Collections[collectionIndex];
                var collectionGo = GameObject.Instantiate(prefab, ItemInstantiateRoot);
                int caseY = GetCaseY(newCase);
                collectionGo.transform.localPosition = new Vector3(0, caseY, 0);
            }
            else
            {
                Debug.LogError($"Collection:{newCase} is an empty list!");
            }
        }
        else
        {
            Debug.LogError($"Collection:{newCase} is not present in dict!");
        }
    }

    private void HandleCameraFollow()
    {
        if (!_shouldStartFollowing)
        {
            _followTimer += Time.deltaTime;
            if (_followTimer >= GameConfig.INITIAL_FOLLOW_DELAY)
            {
                _shouldStartFollowing = true;
            }
        }

        if (_shouldStartFollowing)
        {
            MoveCamera();
        }
    }


    private void MoveCamera()
    {
        //var xPos = FindCameraTowardsXPos();
        var xPos = 0;
        var yPos = Camera.transform.position.y - (GameConfig.GAME_START_SCROLL_SPD * Time.deltaTime);
        Camera.transform.position = new Vector3(xPos, yPos, -10);
    }


    private void RefreshUI()
    {
        UIManager.Instance.RefreshDivedMeterText(_currDepth);
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
        _currDepth = 0;
        _currCase = 0;
        _water = GameConfig.GAME_START_WATER;
        _scrollSpd = GameConfig.GAME_START_SCROLL_SPD;
        _waterDropPerSec = GameConfig.WATER_DROP_PER_SEC;
        Camera.transform.position = GameConfig.CAMERA_INIT_POS;
    }

    private RootController InstantiateNewRootController(float posXPercInScreen, KeyCode leftKey, KeyCode rightKey)
    {
        var go = GameObject.Instantiate(RootWithControllerPrefab,RootInstantiateRoot);
        var controller = go.GetComponentInChildren<RootController>();
        var initPos = new Vector3(GetPosXByPercInScreen(posXPercInScreen), -4,0);
        controller.Init(leftKey, rightKey, initPos,_scrollSpd);
        return controller;
    }



    public float GetCurrentDepth()
    {
        return _currDepth;
    }

    #region Utils
    private static int GetMaxCaseNeedForGenerate(float currDepth)
    {
        double casesNeeded = Math.Floor((currDepth + GameConfig.PRE_RENDER_DEPTH) / (GameConfig.CASE_Y_LENGTH + GameConfig.CASE_SPACING));

        return (int)casesNeeded;
    }

    public static int GetCaseY(int newCase)
    {
        return (int)(10 + (newCase - 1) * (GameConfig.CASE_Y_LENGTH + GameConfig.CASE_SPACING) + 0.5 * GameConfig.CASE_Y_LENGTH)*(-1);
    }


    private static float GetPosXByPercInScreen(float posXPercInScreen)
    {
        var xLocalPos = Screen.width * (posXPercInScreen - 50) / 100;
        return xLocalPos;
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
        var resultXPos = (minX + maxX) / 2;

        return resultXPos;
    }
    #endregion


    #region Public Interface
    public void AddWater(float waterAmt)
    {
        _water += waterAmt;
        _water = Mathf.Clamp(_water, 0, GameConfig.MAX_WATER);
    }
    #endregion
}
