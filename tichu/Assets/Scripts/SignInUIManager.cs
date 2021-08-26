using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignInUIManager : MonoBehaviour
{
    public GameObject LoginUI;
    public GameObject RegisterUI;

    private void Awake()
    {
        LoginUI.SetActive(true);
        RegisterUI.SetActive(false);
    }

    public void RegisterButton()
    {
        LoginUI.SetActive(false);
        RegisterUI.SetActive(true);
    }

    public void BackButton()
    {
        LoginUI.SetActive(true);
        RegisterUI.SetActive(false);
    }
}
