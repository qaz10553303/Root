using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RootController : MonoBehaviour
{
    public Transform GameObjTrans;
    public Transform RotationTrans;
    private KeyCode leftKey;
    private KeyCode rightKey;
    private float _moveSpd;
    private float _lastRootSpawnDepth;

    public void Init(KeyCode leftKey, KeyCode rightKey, Vector3 initPos, float moveSpd)
    {
        this.leftKey = leftKey;
        this.rightKey = rightKey;
        GameObjTrans.localPosition = initPos;
        RotationTrans.localEulerAngles = new Vector3(0,0,180);
        transform.localPosition = Vector3.zero;
        _moveSpd = moveSpd;
        _lastRootSpawnDepth = 0;
    }

    public void Tick()
    {
        HandleRotateInput();

        Move();

        if (GameManager.Instance.GetCurrentDepth() - _lastRootSpawnDepth > GameConfig.ROOT_PIECE_SPAWN_INTERVAL_LENGTH)
        {
            InstantiateEmissionRootPiece();
            _lastRootSpawnDepth = GameManager.Instance.GetCurrentDepth();
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

    public void InstantiateEmissionRootPiece()
    {
        var prefab = GameManager.Instance.RootEmissionPrefab;
        var instantiateRoot = GameManager.Instance.RootInstantiateRoot;
        var go = GameObject.Instantiate(prefab, instantiateRoot);
        go.transform.localPosition = GameObjTrans.localPosition;
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
}
