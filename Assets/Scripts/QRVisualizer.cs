using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class QRVisualizer : MonoBehaviour
{
    public TextMeshPro textMeshPro;
    private QRScanner qRScanner;

    void Start()
    {
        textMeshPro.text = "";

        qRScanner = GetComponent<QRScanner>();
        qRScanner.OnScanned
            .Subscribe(qr =>
            {
                Debug.Log(qr.Data);
                textMeshPro.text = qr.Data;
            })
            .AddTo(this);
        qRScanner.IsReady
            .Where(x => x)
            .First()
            .Subscribe(_ => qRScanner.StartScan())
            .AddTo(this);
    }
}
