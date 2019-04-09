# Socket-Server 和會員系統
這是一個整合會員系統的展示專案

### 下載此專案   
```
    git clone 
    git checkout IntegrateHttpServerExample
```

### 快速入門  
    1. 使用 Unity 打開 UnityDemo 專案  
    2. PlayerSetting/OtherSettings/Configuration/ScriptRuntimeVersion 設為 .Net 4.x Equivalent  
    3. PlayerSetting/OtherSettings/Configuration/ScriptingBackend       設為 IL2CPP  
    4. 進入在 F.T 資料夾內的 QuickStart.scene  
    5. 按 Play 執行專案  
    6. 如果 Hierarchy 中 Main 物件上的，FTServerConnecter.IsConnect 為 true 及代表成功連上我們提供的測試伺服器  
  
### 進階操作  
    1. CallBackHandler 為處理邏輯的中介層  
    2. 在 UnityDemo 專案中提供兩個 CallBackHandler 進行邏輯操作 AccountCallBackHandler 及 GroupCallBackHandler  
    3. 您可以使用下列語法找到該物件
```csharp
        GameObject.FindObjectOfType<Main>()._AccountCallBack;
        GameObject.FindObjectOfType<Main>()._GroupCallBack;
```  
    4. AccountCallBackHandler - 包含帳號的 Set 及 Get  
    5. GroupCallBackHandler - 包含群集的 Join , Exit , GetList , Broadcast  
  

