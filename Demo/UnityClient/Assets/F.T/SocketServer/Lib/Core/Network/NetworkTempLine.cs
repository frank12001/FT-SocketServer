using System.Collections.Generic;

namespace FTServer
{
    public class NetworkTempLine<T>
    {
        private const byte LineCount = 2;

        private static byte CurrectLine = 0;
        private static List<T>[] Lines;

        public NetworkTempLine()
        {
            Lines = new List<T>[LineCount];
            for (var i = 0; i < Lines.Length; i++)
            {
                Lines[i] = new List<T>();
            }
        }

        public byte GetUnUseLineIndex()
        {
            byte result = 0;
            if (CurrectLine.Equals(0))
                result = 1;

            return result;
        }

        public void ChangeCurrectLine()
        {
            byte unUnseLineIndex = GetUnUseLineIndex();
            CurrectLine = unUnseLineIndex;
        }

        public List<T> GetUnUseLine()
        {
            byte unUnseLineIndex = GetUnUseLineIndex();
            return new List<T>(Lines[unUnseLineIndex]);
        }

        public void ClearUnUseLine()
        {
            byte unUnseLineIndex = GetUnUseLineIndex();
            Lines[unUnseLineIndex].Clear();
        }

        public void AddToCurrectLine(T t)
        {
            Lines[CurrectLine].Add(t);
        }

    }
}