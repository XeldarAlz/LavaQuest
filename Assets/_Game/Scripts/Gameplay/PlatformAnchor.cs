using UnityEngine;
using System.Collections.Generic;
using LavaQuest.Core;

namespace LavaQuest.Gameplay
{
    public sealed class PlatformAnchor : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private List<RectTransform> _slots = new();

        [Header("Debug Visualization")] 
        [SerializeField] private bool _showGizmos = true;
        [SerializeField] private Color _gizmoColor = Color.green;
        [SerializeField] private float _gizmoRadius = 30f;

        private static RectTransform _cachedAvatarContainer;

        private readonly List<PlayerAvatar> _avatarsOnPlatform = new();
        private int _nextSlotIndex;

        public Vector2 Position => !_rectTransform ? Vector2.zero : GetWorldAnchoredPosition(_rectTransform);

        private void OnEnable()
        {
            GameEvents.OnAvatarContainerReady += HandleAvatarContainerReady;
        }

        private void OnDisable()
        {
            GameEvents.OnAvatarContainerReady -= HandleAvatarContainerReady;
        }

        private void OnValidate()
        {
            if (!_rectTransform)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            AutoFindSlots();
        }

        private void HandleAvatarContainerReady(RectTransform container)
        {
            _cachedAvatarContainer = container;
        }

        private void AutoFindSlots()
        {
            if (_slots.Count > 0)
            {
                return;
            }

            _slots.Clear();

            for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                RectTransform child = transform.GetChild(childIndex) as RectTransform;

                if (child)
                {
                    _slots.Add(child);
                }
            }
        }

        private Vector2 GetWorldAnchoredPosition(RectTransform target)
        {
            if (!_cachedAvatarContainer)
            {
                return target.anchoredPosition;
            }

            Vector3 worldPosition = target.position;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_cachedAvatarContainer,
                RectTransformUtility.WorldToScreenPoint(null, worldPosition), null, out Vector2 localPoint);

            return localPoint;
        }

        private Vector2 GetNextSlotPosition()
        {
            if (_slots.Count == 0)
            {
                return Position;
            }

            int slotIndex = _nextSlotIndex % _slots.Count;

            return GetWorldAnchoredPosition(_slots[slotIndex]);
        }

        public Vector2 ReserveNextSlot()
        {
            Vector2 position = GetNextSlotPosition();
            _nextSlotIndex++;
            return position;
        }

        public void AddAvatar(PlayerAvatar avatar)
        {
            if (!_avatarsOnPlatform.Contains(avatar))
            {
                _avatarsOnPlatform.Add(avatar);
            }
        }

        public void RemoveAvatar(PlayerAvatar avatar)
        {
            _avatarsOnPlatform.Remove(avatar);
        }

        public void ClearAvatars()
        {
            _avatarsOnPlatform.Clear();
            _nextSlotIndex = 0;
        }

        public List<PlayerAvatar> GetAvatars()
        {
            return new List<PlayerAvatar>(_avatarsOnPlatform);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_showGizmos)
            {
                return;
            }

            if (!_rectTransform)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            Gizmos.color = _gizmoColor;
            Vector3 worldPos = transform.position;

            float scaledRadius = _gizmoRadius * transform.lossyScale.x;
            Gizmos.DrawWireSphere(worldPos, scaledRadius);

            UnityEditor.Handles.Label(worldPos + Vector3.up * scaledRadius * 1.5f, gameObject.name);

            Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.5f);

            for (int slotIndex = 0; slotIndex < _slots.Count; slotIndex++)
            {
                if (!_slots[slotIndex])
                {
                    continue;
                }
                
                Vector3 slotWorldPos = _slots[slotIndex].position;
                float slotRadius = scaledRadius * 0.5f;
                Gizmos.DrawSphere(slotWorldPos, slotRadius);
                Gizmos.DrawLine(worldPos, slotWorldPos);
            }
        }
#endif
    }
}