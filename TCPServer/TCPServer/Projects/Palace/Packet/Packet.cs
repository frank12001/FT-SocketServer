using System;

namespace TCPServer.Projects.Palace.Packet
{
    /// <summary>
    /// 開始排隊時，傳入的資料
    /// </summary>
    [Serializable]
    public struct PalaceTest
    {
        public string PlayerName, StellarId, StellarAvatar1PartId, StellarAvatar2PartId, StellarAvatar3PartId;
        public int ChipMultiple;
        public byte PlayerIdInRoom;
        public string FirebaseUserId;
       
    }
    [Serializable]
    public enum MyEnum1 : byte
    {
        a, b, c, d, e, f, g
    }
    /// <summary>
    /// 開始排隊時，傳入的資料
    /// </summary>
    [Serializable]
    public class PalaceTest2
    {
        private MyEnum1 a, b, c, d, e, f, g;
        private int i, j;
        private float z1, z2, z3, z4, z5, z6;
        private byte bb;

        public PalaceTest2()
        {
            a = MyEnum1.a;
            b = MyEnum1.b;
            c = MyEnum1.c;
            d = MyEnum1.d;
            e = MyEnum1.e;
            f = MyEnum1.f;
            g = MyEnum1.g;

            i = 1;
            j = 2;

            bb = 1;
        }

    }
}
