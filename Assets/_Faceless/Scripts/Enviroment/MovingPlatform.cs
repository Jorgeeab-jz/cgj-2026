using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Faceless
{
    public class MovingPlatform : MonoBehaviour
    {
        public enum LoopType
        {
            Restart,
            PingPong,
            Once
        }

        [Header("Platform Settings")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private float waitTime = 1f;
        [SerializeField] private Ease easeType = Ease.Linear;
        [SerializeField] private LoopType loopType = LoopType.Restart;

        [Header("Path Settings")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();
        [SerializeField] private bool useChildrenAsWaypoints = true;

        [Header("Gizmos")]
        [SerializeField] private Color gizmoColor = Color.yellow;

        private int _currentWaypointIndex = 0;
        private int _direction = 1;

        private void Start()
        {
            if (waypoints.Count == 0 && useChildrenAsWaypoints)
            {
                foreach (Transform child in transform)
                {
                    if (child != transform)
                    {
                        waypoints.Add(child);
                    }
                }
            }

            if (waypoints.Count > 1)
            {
                if (useChildrenAsWaypoints)
                {
                    foreach (var wp in waypoints)
                    {
                        wp.SetParent(null);
                    }
                }
                
                transform.position = waypoints[0].position;
                
                MoveToNextWaypoint();
            }
            else
            {
                Debug.LogWarning("MovingPlatform: Not enough waypoints assigned!", this);
            }
        }

        private void MoveToNextWaypoint()
        {
            if (waypoints.Count < 2) return;

            if (loopType == LoopType.PingPong)
            {
                _currentWaypointIndex += _direction;

                if (_currentWaypointIndex >= waypoints.Count)
                {
                    _currentWaypointIndex = waypoints.Count - 2;
                    _direction = -1;
                }
                else if (_currentWaypointIndex < 0)
                {
                    _currentWaypointIndex = 1;
                    _direction = 1;
                }
            }
            else // Restart or Once
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= waypoints.Count)
                {
                    if (loopType == LoopType.Restart)
                    {
                        _currentWaypointIndex = 0;
                    }
                    else
                    {
                        return; // Stop at the end
                    }
                }
            }

            Transform target = waypoints[_currentWaypointIndex];
            float distance = Vector3.Distance(transform.position, target.position);
            float duration = distance / speed;

            transform.DOMove(target.position, duration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(waitTime, MoveToNextWaypoint);
                });
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(null);
            }
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count == 0)
            {
                // Visualize children
                if (useChildrenAsWaypoints && transform.childCount > 0)
                {
                    Gizmos.color = gizmoColor;
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform current = transform.GetChild(i);
                        // For children visualizer, we can't easily check 'PingPong' vs 'Restart' logic perfectly 
                        // without the list, but let's assume default loop visual
                        Transform next = transform.GetChild((i + 1) % transform.childCount);
                        
                        // If we are strictly not looping restart (and assuming we might have the enum set),
                        // this check becomes tricky in editor time without the serialized property easily accessible 
                        // or just duplicating the list logic.
                        // Let's just draw lines between subsequent children.
                        
                        if (i < transform.childCount - 1)
                        {
                            Gizmos.DrawLine(current.position, transform.GetChild(i + 1).position);
                        }
                        else if (loopType == LoopType.Restart)
                        {
                             Gizmos.DrawLine(current.position, transform.GetChild(0).position);
                        }
                        
                        Gizmos.DrawSphere(current.position, 0.2f);
                    }
                }
                return;
            }

            Gizmos.color = gizmoColor;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;

                Transform current = waypoints[i];
                Gizmos.DrawSphere(current.position, 0.2f);

                if (i < waypoints.Count - 1)
                {
                    Gizmos.DrawLine(current.position, waypoints[i + 1].position);
                }
                else if (loopType == LoopType.Restart)
                {
                    // Draw line back to start only if Restart
                    Gizmos.DrawLine(current.position, waypoints[0].position);
                }
            }
        }
    }
}
