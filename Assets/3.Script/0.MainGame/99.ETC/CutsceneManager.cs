using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector prologueDirector;

    [Header("Skip UI")]
    [SerializeField] private GameObject skipTextObject;
    [SerializeField] private CanvasGroup skipTextCanvasGroup;

    public bool isBlinking = false;

    void Start()
    {
        if (prologueDirector != null)
            prologueDirector.Play();

        SceneController.Instance.LoadNextScene();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SkipPrologue();
        }

        if (SceneController.Instance != null && SceneController.Instance.isLoadingVisualDone)
        {
            if (!isBlinking)
            {
            StartCoroutine(BlinkText_co());
            }
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


        SceneController.Instance.canActivateScene = true;
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

    public void LoadNextScene()
    {
        SceneController.Instance.LoadNextScene();

    }
}
