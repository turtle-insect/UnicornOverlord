using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OfficeOpenXml;

using static OfficeOpenXml.ExcelErrorValue;

namespace UnicornOverlord
{
    internal class Info
    {
        private static Info mThis = new Info();
        public List<NameValueInfo> Item { get; private set; } = new List<NameValueInfo>();
        public List<NameValueInfo> Class { get; private set; } = new List<NameValueInfo>();

        private Info() { }

        static Info()
        {
            mThis.Initialize();
        }

        public static Info Instance()
        {
            return mThis;
        }

        private void Initialize()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            using var package = new ExcelPackage(new FileInfo("info\\data.xlsx"));
            var classSheet = package.Workbook.Worksheets["class"];
            var itemSheet = package.Workbook.Worksheets["item"];

            //Handle class data
            for (int row = 2; row <= classSheet.Dimension.End.Row; row++)
            {
                var id = Convert.ToUInt32(classSheet.Cells[row, 1].Value.ToString());
                if (id == 0)
                    continue;
                var nameEn = classSheet.Cells[row, 2].Value.ToString();
                var nameJp = classSheet.Cells[row, 3].Value.ToString();
                var nameCn = classSheet.Cells[row, 4].Value.ToString();

                NameValueInfo type = new NameValueInfo();
                if (type.Line(id, new string[] { nameEn, nameJp, nameCn }))
                {
                    Class.Add(type);
                }
            }

            //Handle item data
            for (int row = 2; row <= itemSheet.Dimension.End.Row; row++)
            {
                var id = Convert.ToUInt32(itemSheet.Cells[row, 1].Value.ToString());
                if (id == 0)
                    continue;
                var nameEn = itemSheet.Cells[row, 2].Value.ToString();
                var nameJp = itemSheet.Cells[row, 3].Value.ToString();
                var nameCn = itemSheet.Cells[row, 4].Value.ToString();

                NameValueInfo type = new NameValueInfo();
                if (type.Line(id, new string[] { nameEn, nameJp, nameCn }))
                {
                    Item.Add(type);
                }
            }

            //AppendList("info\\item.txt", Item);
            //AppendList("info\\class.txt", Class);
        }

        public NameValueInfo? Search<Type>(List<Type> list, uint id)
            where Type : NameValueInfo, new()
        {
            int min = 0;
            int max = list.Count;
            for (; min < max;)
            {
                int mid = (min + max) / 2;
                if (list[mid].Value == id) return list[mid];
                else if (list[mid].Value > id) max = mid;
                else min = mid + 1;
            }
            return null;
        }

        private void AppendList<Type>(String filename, List<Type> items) where Type : NameValueInfo, new()
        {
            if (!System.IO.File.Exists(filename)) return;
            String[] lines = System.IO.File.ReadAllLines(filename);

            foreach (String line in lines)
            {
                if (line.Length < 3) continue;
                if (line[0] == '#') continue;
                String[] values = line.Split('\t');
                if (values.Length < 2) continue;
                if (String.IsNullOrEmpty(values[0])) continue;
                if (String.IsNullOrEmpty(values[1])) continue;

                Type type = new Type();
                if (type.Line(values))
                {
                    items.Add(type);
                }
            }

            items.Sort();
        }
    }
}
