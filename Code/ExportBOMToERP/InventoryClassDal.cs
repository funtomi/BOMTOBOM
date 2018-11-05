using System;
using System.Data;
using System.Xml;
using Thyt.TiPLM.DEL.Product;

namespace ExportBOMToERP {
    public class InventoryClassDal : BaseDal {
        public InventoryClassDal(DEBusinessItem dItem) {
            _dEBusinessItem = dItem;
            this._name = "inventoryclass";
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
                row[col] = val == null ? DBNull.Value : val;
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
            dt.Columns.Add("code");//		存货分类编码
            dt.Columns.Add("name");//存货分类名称
            dt.Columns.Add("rank", typeof(int));//存货分类编码级次
            dt.Columns.Add("end_rank_flag", typeof(int));//		末级标志
            dt.Columns.Add("econo_sort_code");//		所属经济分类编码
            dt.Columns.Add("barcode");//		对应条形码中的编码 
            return dt;
        }
    }
}
