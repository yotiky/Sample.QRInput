using QRTracking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class QRVisualizer : MonoBehaviour
{
    public GameObject qrCodePlane;
    private SpatialGraphCoordinateSystem spatialGraph;
    private GameObject plane;
    private TextMeshPro data;
    private QRScanner qRScanner;

    void Start()
    {
        spatialGraph = qrCodePlane.GetComponent<SpatialGraphCoordinateSystem>();
        plane = qrCodePlane.transform.Find("Cube").gameObject;
        data = qrCodePlane.transform.Find("Text").GetComponent<TextMeshPro>();

        qRScanner = GetComponent<QRScanner>();
        qRScanner.OnScanned
            .Subscribe(qr =>
            {
                Debug.Log(qr.Data);
                qrCodePlane.SetActive(true);
                spatialGraph.Id = qr.SpatialGraphNodeId;
                plane.transform.localPosition = new Vector3(qr.PhysicalSideLength / 2, qr.PhysicalSideLength / 2, 0);
                plane.transform.localScale = new Vector3(qr.PhysicalSideLength, qr.PhysicalSideLength, 0.001f);
                data.text = qr.Data;
                data.gameObject.transform.localPosition = new Vector3(qr.PhysicalSideLength / 2, qr.PhysicalSideLength / 2, -0.001f);
            })
            .AddTo(this);
        qRScanner.IsReady
            .Where(x => x)
            .First()
            .Subscribe(_ => qRScanner.StartScan())
            .AddTo(this);

        qrCodePlane.SetActive(false);
    }
}
