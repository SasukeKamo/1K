using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    MainMenu MainMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnter()
    {
        gameObject.SetActive(true);
    }

    public void OnExit()
    {
        gameObject.SetActive(false);
    }

    public void Back()
    {
        MainMenu.OnEnter();
        OnExit();
    }
}

