Prometheus Client Lib
Use PushGateWay push metrics

from prometheus-net
https://github.com/prometheus-net/prometheus-net

modify history
1. Prometheus namespace change to FTServer
2. FTServer has Math namespace, change the Math to System.Math in prometheus-net
3. Metrics.cs line 83. use a monitor to check application state. Annotation it!!!!
