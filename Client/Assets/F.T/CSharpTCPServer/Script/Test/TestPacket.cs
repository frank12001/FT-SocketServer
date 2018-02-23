using System;

[Serializable]
public class TestPacket{

    public int i;
    public bool b;
    public TestPacket(int i ,bool b)
    {
        this.i = i;
        this.b = b;
    }
}