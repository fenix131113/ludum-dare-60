using UnityEngine;

namespace GameAssembly.Player._2D
{
    public class TradingZoneScroll : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private Transform leftBoundary;
        [SerializeField] private Transform rightBoundary;
        [SerializeField] private float minOffset = -5f;
        [SerializeField] private float maxOffset = 5f;

        private Vector3 _startLocalPosition;
        private float _offset;
        private bool _hasStartPosition;

        private void Awake() => SaveStartPosition();

        public void ResetScroll()
        {
            SaveStartPosition();
            _offset = 0f;

            if (contentRoot)
                contentRoot.localPosition = _startLocalPosition;
        }

        public bool TryGetVelocity(Transform player, Vector3 position, float speed, float deltaTime,
            out Vector3 velocity)
        {
            velocity = Vector3.zero;

            if (!isActiveAndEnabled || !contentRoot || !leftBoundary || !rightBoundary || deltaTime <= 0f ||
                player.IsChildOf(contentRoot) || leftBoundary.IsChildOf(contentRoot) ||
                rightBoundary.IsChildOf(contentRoot))
                return false;

            var direction = rightBoundary.position - leftBoundary.position;
            direction.y = 0f;
            var width = direction.magnitude;

            if (width < 0.001f)
                return false;

            if (Mathf.Abs(speed) < 0.001f)
                return true;

            direction /= width;
            var playerOffset = Vector3.Dot(position - leftBoundary.position, direction);
            var movement = speed * deltaTime;
            var nextPlayerOffset = Mathf.Clamp(playerOffset + movement, 0f, width);
            var playerMovement = nextPlayerOffset - playerOffset;

            // Only movement past the window edge scrolls the square.
            var scrollMovement = movement - playerMovement;
            if (Mathf.Abs(speed) > 0.001f && scrollMovement * speed > 0f)
            {
                SaveStartPosition();
                var nextOffset = Mathf.Clamp(_offset - scrollMovement, minOffset, maxOffset);
                var worldMovement = direction * (nextOffset - _offset);
                contentRoot.position += worldMovement;
                _offset = nextOffset;
            }

            velocity = direction * (playerMovement / deltaTime);
            return true;
        }

        private void SaveStartPosition()
        {
            if (_hasStartPosition || !contentRoot)
                return;

            _startLocalPosition = contentRoot.localPosition;
            _hasStartPosition = true;
        }

        private void OnValidate()
        {
            minOffset = Mathf.Min(0f, minOffset);
            maxOffset = Mathf.Max(0f, maxOffset);
        }

        private void OnDrawGizmosSelected()
        {
            if (!leftBoundary || !rightBoundary)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftBoundary.position, rightBoundary.position);
            Gizmos.DrawWireSphere(leftBoundary.position, 0.05f);
            Gizmos.DrawWireSphere(rightBoundary.position, 0.05f);
        }
    }
}
