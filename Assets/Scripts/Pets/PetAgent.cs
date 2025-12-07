using UnityEngine;
using PetSimLite.Data;
using PetSimLite.Resources;

namespace PetSimLite.Pets
{
    /// <summary>
    /// Handles simple follow + attack behaviour for a pet instance.
    /// </summary>
    public class PetAgent : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float followRange = 3f;
        [SerializeField] private float attackRange = 1.5f;

        [Header("Stats")]
        [SerializeField] private float damagePerSecond = 1f;

        public PetData PetData { get; private set; }
        public Breakable CurrentTarget { get; private set; }
        public bool IsIdle => CurrentTarget == null;

        private Transform _player;
        private float _damageBuffer;

        public void Initialize(PetData data, Transform player)
        {
            PetData = data;
            _player = player;
            if (data != null)
            {
                damagePerSecond = Mathf.Max(0.1f, data.BasePower);
            }
        }

        private void Update()
        {
            if (_player == null) return;

            var target = CurrentTarget;
            if (target != null)
            {
                // Target might get destroyed between frames; bail if so.
                if (target == null)
                {
                    ClearTarget();
                    return;
                }

                float dist = Vector3.Distance(transform.position, target.transform.position);
                if (dist > attackRange)
                {
                    MoveTowards(target.transform.position);
                }
                else
                {
                    Attack(target);
                }
            }
            else
            {
                FollowPlayer();
            }
        }

        public void AssignTarget(Breakable target)
        {
            CurrentTarget = target;
            _damageBuffer = 0f;
        }

        public void ClearTarget()
        {
            CurrentTarget = null;
            _damageBuffer = 0f;
        }

        private void FollowPlayer()
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist > followRange)
            {
                MoveTowards(_player.position);
            }
        }

        private void MoveTowards(Vector3 destination)
        {
            Vector3 flatDest = new Vector3(destination.x, transform.position.y, destination.z);
            Vector3 dir = flatDest - transform.position;
            if (dir.sqrMagnitude < 0.0001f) return;

            Vector3 step = dir.normalized * moveSpeed * Time.deltaTime;
            transform.position += step;
            transform.forward = Vector3.Lerp(transform.forward, dir.normalized, 10f * Time.deltaTime);
        }

        private void Attack(Breakable target)
        {
            if (target == null)
            {
                ClearTarget();
                return;
            }

            _damageBuffer += damagePerSecond * Time.deltaTime;
            int damageInt = Mathf.FloorToInt(_damageBuffer);
            if (damageInt > 0)
            {
                target.ApplyDamage(damageInt);
                _damageBuffer -= damageInt;
            }
        }
    }
}
