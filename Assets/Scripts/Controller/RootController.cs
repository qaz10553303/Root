using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum RootStatus
{
    InSpawn = 1,
    Running = 2,
}
public class RootController : MonoBehaviour
{
    public Transform GameObjTrans;
    public Transform RotationTrans;
    private KeyCode leftKey;
    private KeyCode rightKey;
    private float _moveSpd;
    private Vector3 _lastRootSpawnPos;
    private int _rootSlot;
    private RootStatus _rootStatus;

    public void Init(int rootSlot, KeyCode leftKey, KeyCode rightKey, Vector3 spawnPos, float moveSpd, float delaySpawnTime)
    {
        this.leftKey = leftKey;
        this.rightKey = rightKey;
        GameObjTrans.localPosition = spawnPos;
        RotationTrans.localEulerAngles = new Vector3(0,0,180);
        transform.localPosition = Vector3.zero;
        _moveSpd = moveSpd;
        _lastRootSpawnPos = spawnPos;
        _rootSlot = rootSlot;
        
        if (delaySpawnTime > 0)
        {
            _rootStatus = RootStatus.InSpawn;
            StartCoroutine(PlaySpawnAnimation(delaySpawnTime));
        }
        else
        {
            _rootStatus = RootStatus.Running;
        }
    }

    private IEnumerator PlaySpawnAnimation(float delaySpawnTime)
    {
        yield return new WaitForSeconds(delaySpawnTime);

        _rootStatus = RootStatus.Running;

        var xPerc = GetRootFinalXPosPercBySlot(_rootSlot);
        var finalX = GameManager.GetPosXByPercInScreen(xPerc);

        AddSpeed(GameConfig.ROOT_SPAWN_SPEED_BOOST);
        float xDelta = finalX - GameObjTrans.transform.position.x;
        if (xDelta > 0)
        {
            RotationTrans.localEulerAngles = new Vector3(0, 0, GameConfig.MOST_RIGHT_ROOT_ROTATION_Z);
        }
        else if (xDelta < 0)
        {
            RotationTrans.localEulerAngles = new Vector3(0, 0, GameConfig.MOST_LEFT_ROOT_ROTATION_Z);
        }

        bool rotEnd = false;
        bool boostEnd = false;
        while (!rotEnd || !boostEnd)
        {
            if (!rotEnd && Mathf.Abs(xDelta) < 0.05f)
            {
                RotationTrans.localEulerAngles = new Vector3(0, 0, 180);
                rotEnd = true;
            }

            var currDepth = GameManager.Instance.GetCurrentDepth();
            if (!boostEnd && - GameObjTrans.localPosition.y > currDepth)
            {
                AddSpeed(-GameConfig.ROOT_SPAWN_SPEED_BOOST);
                boostEnd = true;
            }
            yield return null;
        }
    }

    private float GetRootFinalXPosPercBySlot(int rootSlot)
    {
        switch (rootSlot)
        {
            case 1: 
                return GameConfig.ROOT_1_SPAWN_X_PERC;
            case 2:
                return GameConfig.ROOT_2_SPAWN_X_PERC;
            case 3:
                return GameConfig.ROOT_3_SPAWN_X_PERC;
            case 4:
                return GameConfig.ROOT_4_SPAWN_X_PERC;
            default:
                Debug.LogError($"Invalid root slot:{rootSlot}");
                return 0;
        }
    }

    public void Tick()
    {
        if(_rootStatus == RootStatus.InSpawn)
        {
            return;
        }

        HandleRotateInput();

        Move();

        if ((GameObjTrans.localPosition - _lastRootSpawnPos).magnitude > GameConfig.ROOT_PIECE_SPAWN_INTERVAL_LENGTH)
        {
            InstantiateEmissionRootPiece();
            _lastRootSpawnPos = GameObjTrans.localPosition;
        }
    }

    private void Move()
    {
        Vector2 moveDir = Quaternion.Euler(0, 0, RotationTrans.localEulerAngles.z) * Vector2.up;
        float deltaMoveDist = _moveSpd * Time.deltaTime;
        var moveDist = new Vector2(moveDir.x * deltaMoveDist, -deltaMoveDist);
        GameObjTrans.Translate(moveDist);
    }

    private void HandleRotateInput()
    {
        float currRot = RotationTrans.eulerAngles.z;
        float rotDelta = GameConfig.ROOT_ROTATION_SPD * Time.deltaTime;
        if (Input.GetKey(leftKey) && RotationTrans.localRotation.eulerAngles.z > GameConfig.MOST_LEFT_ROOT_ROTATION_Z)
        {
            currRot = Mathf.Max(RotationTrans.localRotation.eulerAngles.z - rotDelta, GameConfig.MOST_LEFT_ROOT_ROTATION_Z);
        }
        if (Input.GetKey(rightKey) && RotationTrans.localRotation.eulerAngles.z < GameConfig.MOST_RIGHT_ROOT_ROTATION_Z)
        {
            currRot = Mathf.Min(RotationTrans.localRotation.eulerAngles.z + rotDelta, GameConfig.MOST_RIGHT_ROOT_ROTATION_Z);
        }
        RotationTrans.localEulerAngles = new Vector3(0, 0, currRot);
    }

    private void InstantiateEmissionRootPiece()
    {
        var prefab = GameManager.Instance.RootEmissionPrefab;
        var instantiateRoot = GameManager.Instance.RootInstantiateRoot;
        var go = GameObject.Instantiate(prefab, instantiateRoot);
        go.transform.localPosition = (GameObjTrans.localPosition - (GameObjTrans.localPosition - _lastRootSpawnPos).normalized * 0.1f);
        go.transform.localEulerAngles = RotationTrans.localEulerAngles;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if(collision.TryGetComponent<ItemControllerBase>(out var itemContrller))
            {
                itemContrller.OnPickUp();
            }
        }
    }

    public void AddSpeed(float speedAmt)
    {
        _moveSpd += speedAmt;
    }

}
