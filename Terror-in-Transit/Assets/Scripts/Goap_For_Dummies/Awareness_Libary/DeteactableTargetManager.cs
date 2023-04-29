using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeteactableTargetManager : MonoBehaviour
{
    public static DeteactableTargetManager Instance { get; private set; } = null;  // read-only, internally setable

    public List<DetectableTarget> AllTargets = new List<DetectableTarget>();

    private void Awake() {
        if (Instance) {
            Debug.LogError("Multiple detectalbe manager instances, it should be a singleton, destroying " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Register(DetectableTarget target) {
        AllTargets.Add(target);
    }

    public void DeRegister(DetectableTarget target) {
        AllTargets.Remove(target);
    }

}
