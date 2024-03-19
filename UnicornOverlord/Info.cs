using OfficeOpenXml;

using System.IO;

namespace UnicornOverlord
{
    internal class Info
    {
        private static Info mThis = new Info();
        public List<NameValueInfo> Item { get; private set; } = new List<NameValueInfo>();
        public List<NameValueInfo> Class { get; private set; } = new List<NameValueInfo>();
        public List<string> Languages { get; private set; } = new List<string>();
        public int CurrentSelectedLanguage { get; internal set; }

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

            int totalColumns = itemSheet.Columns.Count();

            Languages = new List<string>();
            for (int column = 2; column <= totalColumns; column++)
            {
                // 获取第一行第column列的值
                var cellValue = itemSheet.Cells[1, column].Value;
                if (cellValue != null)
                {
                    Languages.Add(cellValue.ToString());
                }
            }
            //Handle class data

            for (int row = 2; row <= classSheet.Dimension.End.Row; row++)
            {
                var id = Convert.ToUInt32(classSheet.Cells[row, 1].Value.ToString());
                if (id == 0)
                    continue;

                List<string> nameTranslation = new List<string>();
                for (int i = 2; i < Languages.Count + 2; i++)
                {
                    if (classSheet.Cells[row, i].Value == null)
                        break;
                    var name = classSheet.Cells[row, i].Value.ToString();
                    nameTranslation.Add(name);
                }

                NameValueInfo type = new NameValueInfo();
                if (type.Line(id, nameTranslation))
                {
                    Class.Add(type);
                }
            }
            Class.Sort();

            //Handle item data
            for (int row = 2; row <= itemSheet.Dimension.End.Row; row++)
            {
                var id = Convert.ToUInt32(itemSheet.Cells[row, 1].Value.ToString());
                if (id == 0)
                    continue;

                List<string> nameTranslation = new List<string>();
                for (int i = 2; i < Languages.Count + 2; i++)
                {
                    if (itemSheet.Cells[row, i].Value == null)
                        break;
                    var name = itemSheet.Cells[row, i].Value.ToString();
                    nameTranslation.Add(name);
                }

                NameValueInfo type = new NameValueInfo();
                if (type.Line(id, nameTranslation))
                {
                    Item.Add(type);
                }
            }
            Item.Sort();
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
    }
}
