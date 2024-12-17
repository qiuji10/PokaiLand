using System;
using PokaiLand;
using PokaiLand.Enum;
using PokaiLand.Gameplay.Map;
using Unity.Netcode;
using UnityEngine;

public class ResetZone : NetworkBehaviour, IMapObject, IPostProcessMapObjectData<ResetZoneMapObjectData>
{
    [SerializeField] private Transform resetPoint;
    private readonly NetworkVariable<Vector2> _resetPosition = new NetworkVariable<Vector2>();
    public Vector2 ResetPoint => resetPoint.position;
    public EAddressableLabels Labels => EAddressableLabels.Map | EAddressableLabels.ResetZone;

    public override void OnNetworkSpawn()
    {
        UpdateResetPosition(Vector2.zero, _resetPosition.Value);
    }

    private void OnEnable()
    {
        _resetPosition.OnValueChanged += UpdateResetPosition;
    }

    private void OnDisable()
    {
        _resetPosition.OnValueChanged -= UpdateResetPosition;
    }
    
    public void DefineResetPosition(Vector2 position)
    {
        if (Application.isPlaying)
            _resetPosition.Value = position;
        else
            resetPoint.position = position;    
    }
    
    private void UpdateResetPosition(Vector2 prevPosition, Vector2 newPosition)
    {
        resetPoint.position = newPosition;
    }
    
    public void HandlePostProcessMapObjectData(ResetZoneMapObjectData data)
    {
        DefineResetPosition(data.spawnPoint);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = resetPoint.position;
        }
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        // Get the center position and scale
        Vector2 center = transform.position;
        Vector2 size = transform.localScale;

        // Calculate the corners based on rotation
        float angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 topRight = RotatePoint(new Vector2(size.x/2, size.y/2), angle);
        Vector2 topLeft = RotatePoint(new Vector2(-size.x/2, size.y/2), angle);
        Vector2 bottomRight = RotatePoint(new Vector2(size.x/2, -size.y/2), angle);
        Vector2 bottomLeft = RotatePoint(new Vector2(-size.x/2, -size.y/2), angle);

        // Draw the box using lines
        Gizmos.DrawLine(center + topLeft, center + topRight);
        Gizmos.DrawLine(center + topRight, center + bottomRight);
        Gizmos.DrawLine(center + bottomRight, center + bottomLeft);
        Gizmos.DrawLine(center + bottomLeft, center + topLeft);
    }

    private Vector2 RotatePoint(Vector2 point, float angle)
    {
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        return new Vector2(
            point.x * cos - point.y * sin,
            point.x * sin + point.y * cos
        );
    }
    #endif
}
