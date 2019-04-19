# Socket-Server  
## 註冊、登入、聊天室範例  

* 自架伺服器需有 k8s 的基礎知識，如無的話可直接開啟 Client 進入我們提供的測試伺服器
* 這是一個架設小遊戲的伺服器群集展示，包含註冊、登入、聊天室
  * 一個對外的 socketserver (source code /SocketServerDemo1)
  * 對內的 HttpServer       (source code /HttpServerMember)
  * DB (Redis)
  * UnityClient            (source code /UnityDemo)
  
* 使用 k8s/all.yml 進行伺服器部屬，並使用 UnityDemo 中的範例進行連線，即可執行聊天室 Demo  
