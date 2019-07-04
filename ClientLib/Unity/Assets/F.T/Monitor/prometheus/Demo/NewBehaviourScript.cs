using System;
using System.Collections;
using System.Collections.Generic;
using FTServer.Monitor;
using UnityEngine;

namespace Demo
{
    public class NewBehaviourScript : MonoBehaviour
    {
        private static Counter ProcessedJobCount;

        private MetricPusher pusher;
        // Start is called before the first frame update
        void Start()
        {
            LogProxy.WriteLine("start 1");
            pusher = new MetricPusher(endpoint: "http://192.168.2.5:9091/metrics", job: "deviced1",intervalMilliseconds:5000);
            pusher.Start();
            LogProxy.WriteLine("start 2");
            ProcessedJobCount = Metrics
                .CreateCounter("myapp_jobs_processed_total", "Number of processed jobs.",
                    new CounterConfiguration
                    {
                        // Here you specify only the names of the labels.
                        LabelNames = new[] { "DeviceId" }
                    });
            ProcessedJobCount.Labels("android1");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.anyKeyDown)
            {
                LogProxy.WriteLine(ProcessedJobCount.WithLabels("android1").Value);
                ProcessedJobCount.WithLabels("android1").Inc();
            }
        }
    }
}
