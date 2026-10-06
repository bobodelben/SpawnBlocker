// SpawnBlocker
// a Valheim mod using Jötunn to stop monsters from spawning around Spawn Blocker pieces
//
// File:    SpawnBlockerArea.cs
// Project: SpawnBlocker

using System.Collections.Generic;
using UnityEngine;

namespace SpawnBlocker
{
    /// <summary>
    /// Added to the Spawn Blocker prefab. Every loaded blocker registers itself here,
    /// so a "is this point blocked?" check is a few distance comparisons instead of a physics query.
    /// </summary>
    internal class SpawnBlockerArea : MonoBehaviour
    {
        private static readonly List<SpawnBlockerArea> s_blockers = new List<SpawnBlockerArea>();

        private ZNetView _nview;

        private void Start()
        {
            _nview = GetComponent<ZNetView>();
            s_blockers.Add(this);
        }

        private void OnDestroy()
        {
            s_blockers.Remove(this);
        }

        public static bool IsBlocked(Vector3 point)
        {
            float radius = SpawnBlocker.BlockRadius.Value;
            float radiusSqr = radius * radius;

            foreach (SpawnBlockerArea blocker in s_blockers)
            {
                // The build ghost shown while placing the piece has no valid ZNetView and must not block anything
                if (blocker._nview != null && blocker._nview.IsValid() &&
                    (blocker.transform.position - point).sqrMagnitude < radiusSqr)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
