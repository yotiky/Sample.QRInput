using Microsoft.MixedReality.QR;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class QRScanner : MonoBehaviour
{
    private struct ActionData
    {
        public enum EventType
        {
            Added,
            Updated,
            Removed,
        }
        public EventType Type { get; set; }
        public QRCode QRCode { get; set; }
    }

    private QRCodeWatcher qRCodeWatcher;
    private readonly ConcurrentQueue<ActionData> pendingActions = new ConcurrentQueue<ActionData>();

    public bool IsSupported { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public ReactiveProperty<bool> IsReady { get; } = new ReactiveProperty<bool>();

    private readonly Subject<QRCode> onScanned = new Subject<QRCode>();
    public IObservable<QRCode> OnScanned => onScanned;

    async void Start()
    {
        IsSupported = QRCodeWatcher.IsSupported();
        await QRCodeWatcher.RequestAccessAsync();

        qRCodeWatcher = new QRCodeWatcher();
        qRCodeWatcher.Added += QRCodeWatcher_Added;
        qRCodeWatcher.Updated += QRCodeWatcher_Updated;
        qRCodeWatcher.Removed += QRCodeWatcher_Removed;

        IsReady.Value = true;

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if(pendingActions.TryDequeue(out var action))
                {
                    if (action.Type == ActionData.EventType.Added 
                        || action.Type == ActionData.EventType.Updated)
                    {
                        if (DateTimeOffset.Compare(StartTime, action.QRCode.LastDetectedTime) < 0)
                        {
                            onScanned.OnNext(action.QRCode);
                        }
                    }
                }

            })
            .AddTo(this);
    }

    private void QRCodeWatcher_Added(object sender, QRCodeAddedEventArgs e)
    {
        pendingActions.Enqueue(new ActionData { Type = ActionData.EventType.Added, QRCode = e.Code });
    }
    private void QRCodeWatcher_Updated(object sender, QRCodeUpdatedEventArgs e)
    {
        pendingActions.Enqueue(new ActionData { Type = ActionData.EventType.Updated, QRCode = e.Code });
    }
    private void QRCodeWatcher_Removed(object sender, QRCodeRemovedEventArgs e)
    {
        pendingActions.Enqueue(new ActionData { Type = ActionData.EventType.Removed, QRCode = e.Code });
    }

    public void StartScan()
    {
        if (!IsReady.Value) { return; }

        StartTime = DateTimeOffset.Now;
        qRCodeWatcher.Start();
    }

    public void StopScan()
    {
        if (!IsReady.Value) { return; }

        qRCodeWatcher.Stop();
    }
}
