using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<PlayerNetworkData> netState = new(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 vel;
    private float rotVel;
    [SerializeField] private float interpolationTime = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            netState.Value = new PlayerNetworkData()
            {
                Position = transform.position,
                Rotation = transform.rotation.eulerAngles
            };
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, netState.Value.Position, ref vel, interpolationTime);
            transform.rotation = Quaternion.Euler(
                0,
                Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, netState.Value.Rotation.y, ref rotVel, interpolationTime),
                0
            );
        }

    }

    struct PlayerNetworkData : INetworkSerializable
    {
        private float x, z;
        private short yRot;

        internal Vector3 Position
        {
            get => new Vector3(x, 0, z);
            set
            {
                x = value.x;
                z = value.z;
            }
        }

        internal Vector3 Rotation
        {
            get => new Vector3(0, yRot, 0);
            set => yRot = (short)value.y;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref z);
            serializer.SerializeValue(ref yRot);
        }

    }
}
