using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoubleSure : MonoBehaviour
{
    public GameObject window;
    private Action<bool> callBack;
    public static DoubleSure Instance;
    public Button sureButton;
    public Button cancelButton;
    public TextMeshProUGUI msg;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        if(sureButton == null)
        {
            window.transform.Find("sure").GetComponent<Button>();
        }
        if(cancelButton == null)
        {
            cancelButton = window.transform.Find("cancel").GetComponent<Button>();
        }
        if (msg == null)
        {
            msg = transform.GetComponentInChildren<TextMeshProUGUI>();
        }
        sureButton .onClick.AddListener(OnSureClick);
        cancelButton.onClick.AddListener(OnCancleClick);
    }

    private void OnCancleClick()
    {
        callBack?.Invoke(false);
        window.gameObject.SetActive(false);
    }

    private void OnSureClick()
    {
        callBack?.Invoke(true);
        window.gameObject.SetActive(false);
    }

    public void RequestSure(Action<bool> action)
    {
        callBack = action;
        window.gameObject.SetActive(true);
    }
    public void RequestSure(Action<bool> action,string tips)
    {
        callBack = action;
        msg.text = tips;
        window.gameObject.SetActive(true);
    }
    public void UpdateTips(string tips)
    {
        msg.text = tips;
    }
}
