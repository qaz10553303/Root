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
    private int _currFlag;

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

        AddNewRoot();

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

        StartCoroutine(MoveCameraUpwards(10.0f, _currDepth + 2.0f));
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
        var yPos = Camera.transform.position.y - (_scrollSpd * Time.deltaTime);
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
        _currFlag = 0;
        _water = GameConfig.GAME_START_WATER;
        _scrollSpd = GameConfig.GAME_START_SCROLL_SPD;
        _waterDropPerSec = GameConfig.WATER_DROP_PER_SEC;
        Camera.transform.position = GameConfig.CAMERA_INIT_POS;
    }

    private RootController InstantiateNewRootController(Vector3 initPos, float moveSpd, float spawnAnimTime)
    {
        var go = GameObject.Instantiate(RootWithControllerPrefab,RootInstantiateRoot);
        var controller = go.GetComponentInChildren<RootController>();
        _rootControllerList.Add(controller);
        int rootSlot = _rootControllerList.Count;
        controller.Init(rootSlot, GetRootControlKey(rootSlot, true), GetRootControlKey(rootSlot, false), initPos, moveSpd, spawnAnimTime);
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


    public static float GetPosXByPercInScreen(float posXPercInScreen)
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
    private KeyCode GetRootControlKey(int rootSlot, bool isLeft)
    {
        switch (rootSlot)
        {
            case 1:
                return isLeft ? GameSetting.root1LeftKey : GameSetting.root1RightKey;
            case 2:
                return isLeft ? GameSetting.root2LeftKey : GameSetting.root2RightKey;
            case 3:
                return isLeft ? GameSetting.root3LeftKey : GameSetting.root3RightKey;
            case 4:
                return isLeft ? GameSetting.root4LeftKey : GameSetting.root4RightKey;
            default:
                Debug.LogError($"RootSlot:{rootSlot} undefined!");
                return KeyCode.None;
        }
    }

    #endregion


    #region Public Interface
    public void AddWater(float waterAmt)
    {
        _water += waterAmt;
        _water = Mathf.Clamp(_water, 0, GameConfig.MAX_WATER);
    }

    public void AddWaterDecrease(float decreaseAmt)
    {
        _waterDropPerSec += decreaseAmt;
    }

    public void AddNewRoot()
    {
        if(_rootControllerList.Count<GameConfig.ROOT_MAX_COUNT)
        {
            Vector3 initPos = new Vector3(GetPosXByPercInScreen(50), 0, 0);
            if (_rootControllerList.Count > 0)
            {
                int randIndex = Random.Range(0, _rootControllerList.Count);
                initPos = _rootControllerList[randIndex].GameObjTrans.localPosition;
            }

            var rootController = InstantiateNewRootController(initPos, _scrollSpd, 1);
        }
    }

    public void AddScrollSpeed(float addAmount)
    {
        _scrollSpd += addAmount;
        for (int i = 0; i < _rootControllerList.Count; i++)
        {
            _rootControllerList[i].AddSpeed(addAmount);
        }
    }
    #endregion
}
