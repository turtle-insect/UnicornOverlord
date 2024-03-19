using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicornOverlord
{
    internal class NameValueInfo : IComparable
    {
        public uint Value { get; private set; }
        private List<String> mNames = new List<String>();

        public String Name
        {
            get
            {
                var index = Info.Instance().CurrentSelectedLanguage;
                if (index >= mNames.Count) index = 0;

                var value = mNames[index];
                if (String.IsNullOrEmpty(value))
                {
                    value = mNames[0];
                }
                return value;
            }
        }

        public int CompareTo(Object? obj)
        {
            var dist = obj as NameValueInfo;
            if (dist == null) return 0;

            if (Value < dist.Value) return -1;
            else if (Value > dist.Value) return 1;
            else return 0;
        }

        public virtual bool Line(String[] oneLine)
        {
            if (oneLine[0].Length > 1 && oneLine[0][1] == 'x') Value = Convert.ToUInt32(oneLine[0], 16);
            else Value = Convert.ToUInt32(oneLine[0]);

            for (int index = 1; index < oneLine.Length; index++)
            {
                mNames.Add(oneLine[index].TrimStart().TrimEnd());
            }
            return true;
        }


        public virtual bool Line(uint Id, IList<string> names)
        {
            Value = Id;

            for (int index = 0; index < names.Count(); index++)
            {
                mNames.Add(names[index].TrimStart().TrimEnd());
            }
            return true;
        }
    }
}
