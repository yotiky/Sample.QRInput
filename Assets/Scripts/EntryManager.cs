using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class EntryManager : MonoBehaviour
{
    public GameObject page1;
    public GameObject page2;
    public GameObject page3;
    public GameObject result;
    public GameObject controls;
    public QRScanner qrScanner;

    private UserData user = new UserData();
    private GameObject currentPage;

    private ReactiveProperty<bool> isScanning = new ReactiveProperty<bool>();
    private bool isSingleScan;

    void Start()
    {
        currentPage = page1;
        Navigate();

        isScanning
            .Where(x => x)
            .Subscribe(_ => qrScanner.StartScan())
            .AddTo(this);

        isScanning
            .Where(x => !x)
            .Subscribe(_ => qrScanner.StopScan())
            .AddTo(this);

        qrScanner.OnScanned
            .Subscribe(x =>
            {
                if (isSingleScan)
                {
                    var input = currentPage.transform.Find("InputField").GetComponent<TMP_InputField>();
                    input.text = x.Data;
                    isScanning.Value = false;
                }
                else
                {
                    user = JsonUtility.FromJson<UserData>(x.Data);
                    var input = currentPage.transform.Find("InputField").GetComponent<TMP_InputField>();
                    input.text = "success!";
                    isScanning.Value = false;
                }
            })
            .AddTo(this);
    }

    public void NextPage()
    {
        if (!string.IsNullOrEmpty(user.name) && !string.IsNullOrEmpty(user.id) && !string.IsNullOrEmpty(user.password))
        {
            currentPage = result;
            SetResult();
        }
        else if (currentPage == page1)
        {
            var input = currentPage.transform.Find("InputField").GetComponent<TMP_InputField>();
            user.name = input.text;
            currentPage = page2;
        }
        else if (currentPage == page2)
        {
            var input = currentPage.transform.Find("InputField").GetComponent<TMP_InputField>();
            user.id = input.text;
            currentPage = page3;
        }
        else if (currentPage == page3)
        {
            var input = currentPage.transform.Find("InputField").GetComponent<TMP_InputField>();
            user.password = input.text;
            currentPage = result;
            SetResult();
        }

        Navigate();
    }

    private void Navigate()
    {
        page1.SetActive(page1 == currentPage);
        page2.SetActive(page2 == currentPage);
        page3.SetActive(page3 == currentPage);
        result.SetActive(result == currentPage);
    }

    private void SetResult()
    {
        var nameCom = result.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        nameCom.text = user.name;
        var idCom = result.transform.Find("ID").GetComponent<TextMeshProUGUI>();
        idCom.text = user.id;
        var passCom = result.transform.Find("Password").GetComponent<TextMeshProUGUI>();
        passCom.text = user.password;
        controls.SetActive(false);
    }

    public void ScanQRSingle()
    {
        if (isScanning.Value) { return; }
        
        isSingleScan = true;
        isScanning.Value = true;
    }

    public void ScanQRMulti()
    {
        if (isScanning.Value) { return; }
        
        isSingleScan = false;
        isScanning.Value = true;
    }
}
[Serializable]
public class UserData
{
    public string name;
    public string id;
    public string password;
}
