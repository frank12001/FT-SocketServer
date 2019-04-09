# Socket-Server 和會員系統
這是一個整合會員系統的展示專案

使用 git clone 下載此專案

快速入門
  使用 Unity 打開 UnityDemo 專案
  PlayerSetting/OtherSettings/Configuration/ScriptRuntimeVersion 設為 .Net 4.x Equivalent
  PlayerSetting/OtherSettings/Configuration/ScriptingBackend       設為 IL2CPP
  進入在 F.T 資料夾內的 QuickStart.scene
  按 Play 執行專案
  如果 Hierarchy 中 Main 物件上的，FTServerConnecter.IsConnect 為 true 及代表成功連上我們提供的測試伺服器
  
進階操作
  CallBackHandler 為處理邏輯的中介層
  在 UnityDemo 專案中提供兩個 CallBackHandler 進行邏輯操作 AccountCallBackHandler 及 GroupCallBackHandler
  您可以使用  GameObject.FindObjectOfType<Main>()._AccountCallBack 找到該物件
  AccountCallBackHandler 包含帳號的 Set 及 Get
  GroupCallBackHandler   包含群集的 Join , Exit , GetList , Broadcast
  

