using System;
using System.Data;
using System.Xml;
using Thyt.TiPLM.DEL.Product;

namespace ExportBOMToERP {
    public class UnitDal : BaseDal {
        public UnitDal(DEBusinessItem dItem) {
            _dEBusinessItem = dItem;
            this._name = "unit";
            _filePath = BuildFilePath(dItem, _name);
        }

        protected override XmlDocument BuildXmlDocment(string operatorStr) {
            XmlDocument doc = CreateXmlSchema(_name, _dEBusinessItem, operatorStr);
            DataTable dt = GetDataTable(_dEBusinessItem);
            dt.WriteXml(_filePath);
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            var node = doc.ImportNode(docTemp.DocumentElement, true);
            string path = string.Format("ufinterface//{0}", _name);
            var cNode = node.SelectNodes(_name);
            if (cNode == null || cNode.Count == 0) {
                return null;
            }
            for (int j = 0; j < cNode[0].ChildNodes.Count; j++) {
                var childNode = cNode[0].ChildNodes[j];
                doc.SelectSingleNode(path).AppendChild(childNode);
                j--;
            }
            return doc;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetDataTable(DEBusinessItem dEBusinessItem) {
            if (dEBusinessItem == null) {
                return null;
            }
            var dt = BuildElementDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = dEBusinessItem.GetAttrValue(dEBusinessItem.ClassName, col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "group_code":
                        row[col] = val == null ? "01" : val;
                        break;
                }
            }
            dt.Rows.Add(row);
            return dt;
        }

        /// <summary>
        /// 构建节点属性结构
        /// </summary>
        /// <returns></returns>
        private DataTable BuildElementDt() {
            DataTable dt = new DataTable(_name);
            dt.Columns.Add("code");//	计量单位编码
            dt.Columns.Add("name");//	计量单位名称
            dt.Columns.Add("group_code");//	计量单位组编码
            dt.Columns.Add("barcode");//	对应条形码中的编码
            dt.Columns.Add("main_flag", typeof(int));//	主计量单位标志（是否主计量单位）
            dt.Columns.Add("changerate", typeof(decimal));//	换算率
            dt.Columns.Add("portion", typeof(decimal));//	合理浮动比例
            dt.Columns.Add("SerialNum", typeof(int));//	辅计量单位序号
            dt.Columns.Add("censingular");//	英文名称单数
            dt.Columns.Add("cenplural");//	英文名称复数
            dt.Columns.Add("cunitrefinvcode");//	对应存货编码

            return dt;
        }
    }
}
