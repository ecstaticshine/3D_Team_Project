using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PrologueManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector prologueDirector;

    [Header("Skip UI")]
    [SerializeField] private GameObject skipTextObject;
    [SerializeField] private CanvasGroup skipTextCanvasGroup;

    public bool isBlinking = false;

    void Start()
    {
        //SceneController.Instance.LoadScene(SceneName.Training, false);

        if (prologueDirector != null)
        {
            prologueDirector.stopped += OnTimelineFinished;
            prologueDirector.Play();
        }

        StartCoroutine(BlinkText_co());


    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        GoNext();

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SkipPrologue();
        }

    }

    public void SkipPrologue()
    {
        if (prologueDirector != null)
        {
            prologueDirector.time = prologueDirector.duration;
            prologueDirector.Evaluate();
            prologueDirector.Stop();
        }

        GoNext();
    }

    private IEnumerator BlinkText_co()
    {
        isBlinking = true;
        skipTextObject.SetActive(true); // 텍스트 나타나기

        while (true)
        {
            
            float alpha = (Mathf.Sin(Time.time * 3f) + 1f) / 2f;
            if (skipTextCanvasGroup != null) skipTextCanvasGroup.alpha = alpha;

            yield return null;
        }
    }
    private void GoNext()
    {
        SceneController.Instance.LoadScene(SceneName.Training, true);
    }

}
