using System.Collections.Generic;
using System.Linq;

namespace UGoap.Base
{
    public class DebugRecord
    {
        private static List<string> _recordedInfo = new();

        public static void Record(string record)
        {
            _recordedInfo.Add(record);
        }

        public static List<string> GetRecords()
        {
            List<string> records = _recordedInfo.ToList();
            _recordedInfo.Clear();
            return records;
        }
    }
}