using UnityEngine;
using UnityEngine.UI;

namespace Health
{
    // Temporary — delete once the real hit marker exists.
    // Subscribes to the channel and logs every damage event, so you can
    // verify the pipeline fires correct values before building any UI.
    public class DamageDealtDebugListener : MonoBehaviour
    {
        [SerializeField]
        private DamageDealtChannelSO channel;

        [SerializeField]
        private RawImage hitmarker;

        // Subscribe in OnEnable, unsubscribe in OnDisable, named method —
        // the exact poolable-subscriber discipline you already own.
        private void OnEnable()
        {
            if (channel != null)
                channel.OnRaised += HandleDamageDealt;
        }

        private void OnDisable()
        {
            if (channel != null)
                channel.OnRaised -= HandleDamageDealt;
        }

        private void HandleDamageDealt(DamageDealtEvent evt)
        {
            hitmarker.enabled = true;
        }
    }
}
