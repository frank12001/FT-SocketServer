
public enum EventCode : byte
{
    SendPrepareRoomMessage = 12,
    SendGamingRoomMessage = 13,
    DeliverExceptionToClient = 31,
}

/// <summary>
/// exception , it's same as client's exception
/// </summary>
public enum ExceptionCode : int
{
    /// <summary>
    /// when you try to join in the queue but it's seem you have been in there  
    /// </summary>
    RepeatQueue = 20,
    /// <summary>
    /// when check the scene because of ready to create prepare room . But some player not in the correct scene now.
    /// </summary>
    InTheIncorrectSceneWhenReadyToPrepareRoom = 21,
}
     