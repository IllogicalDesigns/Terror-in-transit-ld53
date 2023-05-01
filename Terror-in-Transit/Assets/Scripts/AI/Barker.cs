using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barker : MonoBehaviour {
    [SerializeField] private AudioSource BarkSrc;

    [SerializeField] private AudioClip[] broadSearch;
    private float broadCooldwn = 3f, broadTimer;
    [SerializeField] private AudioClip[] spotted;
    private float spottedCooldwn = 15f, spottedTimer;
    [SerializeField] private AudioClip[] investigate;
    private float investigateCooldwn = 10f, investigateTimer;
    [SerializeField] private AudioClip[] Confirms;
    [SerializeField] private AudioClip[] attacked;
    private float attackedCooldwn = 10f, attackedTimer;

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
        if (broadTimer > 0) broadTimer -= Time.deltaTime;
        if (spottedTimer > 0) spottedTimer -= Time.deltaTime;
        if (attackedTimer > 0) investigateTimer -= Time.deltaTime;
        if (investigateTimer > 0) investigateTimer -= Time.deltaTime;
    }

    public void BarkLine(string line) {
        switch (line) {
            case "BroadSearch":
                if (broadTimer > 0) break;
                broadTimer = broadCooldwn;

                BarkSrc.clip = broadSearch[Random.Range(0, broadSearch.Length - 1)];
                if (!BarkSrc.isPlaying) BarkSrc.Play();
                break;

            case "Spotted":
                if (spottedTimer > 0) break;
                spottedTimer = spottedCooldwn;

                BarkSrc.clip = spotted[Random.Range(0, spotted.Length - 1)];
                if (!BarkSrc.isPlaying) BarkSrc.Play();
                break;

            case "Investigate":
                if (investigateTimer > 0) break;
                investigateTimer = investigateCooldwn;

                BarkSrc.clip = investigate[Random.Range(0, investigate.Length - 1)];
                if (!BarkSrc.isPlaying) BarkSrc.Play();
                break;

            case "Confirm":
                BarkSrc.clip = Confirms[Random.Range(0, Confirms.Length - 1)];
                if (!BarkSrc.isPlaying) BarkSrc.Play();
                break;

            case "Attacked":
                if (attackedTimer > 0) break;
                attackedTimer = attackedCooldwn;

                BarkSrc.clip = attacked[Random.Range(0, attacked.Length - 1)];
                if (!BarkSrc.isPlaying) BarkSrc.Play();
                break;

            default:
                Debug.Log("No bark for " + line);
                break;
        }
    }
}
