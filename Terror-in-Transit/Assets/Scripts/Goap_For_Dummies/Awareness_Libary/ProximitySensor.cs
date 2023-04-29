using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximitySensor : MonoBehaviour
{
    [SerializeField] float proximityRange = 1f;

    [SerializeField] AwarenessSystem awareness;

    [SerializeField] Color proximityColor = new Color(1f, 1f, 1f, 0.25f);

    // Start is called before the first frame update
    void Start()
    {
        awareness = gameObject.GetComponent<AwarenessSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < DeteactableTargetManager.Instance.AllTargets.Count; i++) {
            var candidateTarget = DeteactableTargetManager.Instance.AllTargets[i];

            //Skip if we are checking ourself
            if (candidateTarget.gameObject == gameObject) return;

            if(Vector3.Distance(candidateTarget.transform.position, transform.position) < proximityRange) {
                awareness.ReportInProximity(candidateTarget);
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = proximityColor;
        Gizmos.DrawWireSphere(transform.position, proximityRange);
    }
}
