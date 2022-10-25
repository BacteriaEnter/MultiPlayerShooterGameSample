using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private bool _usingServerAuth;
    [SerializeField] private float _cheapInterpolationTime = 0.1f;

    private NetworkVariable<PlayerNetworkState> _playerState;


    private void Awake()
    {
        var permission = _usingServerAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
        _playerState = new NetworkVariable<PlayerNetworkState>(writePerm: permission);
    }

    private void Update()
    {
        if (IsOwner) TransmitState();
        else ConsumeState();
    }


    #region Transmit State

    private void TransmitState()
    {
        var state = new PlayerNetworkState
        {
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles
        };

        if (IsServer || !_usingServerAuth)
            _playerState.Value = state;
        else
            TransmitStateServerRpc(state);
    }

    [ServerRpc]
    private void TransmitStateServerRpc(PlayerNetworkState state)
    {
        _playerState.Value = state;
    }

    #endregion

    #region Interpolate State

    private Vector3 _vel;
    private float _rotvel;
    private void ConsumeState()
    {
        transform.position = Vector3.SmoothDamp(transform.position, _playerState.Value.Position, ref _vel, _cheapInterpolationTime);
        transform.rotation = Quaternion.Euler(
            0,
            Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _playerState.Value.Rotation.y, ref _rotvel, _cheapInterpolationTime),
            0);
    }

    #endregion

    struct PlayerNetworkState : INetworkSerializable
    {
        private Vector3 _netpos;
        private float _yRot;

        internal Vector3 Position
        {
            get => _netpos;
            set => _netpos = value;

        }

        internal Vector3 Rotation
        {
            get => new Vector3(0, _yRot, 0);
            set => _yRot = value.y;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _netpos);
            serializer.SerializeValue(ref _yRot);
        }
    }


    
}
