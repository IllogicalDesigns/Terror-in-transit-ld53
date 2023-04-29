using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableTarget : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DeteactableTargetManager.Instance.Register(this);
    }

    //private void OnEnable() {
    //    DeteactableTargetManager.Instance.Register(this);
    //}

    //private void OnDisenable() {
    //    DeteactableTargetManager.Instance.Register(this);
    //}

    private void OnDestroy() {
        if(DeteactableTargetManager.Instance) DeteactableTargetManager.Instance.DeRegister(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
