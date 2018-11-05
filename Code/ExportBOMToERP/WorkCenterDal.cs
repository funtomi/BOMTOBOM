using System;
using System.Data;
using System.Xml;
using Thyt.TiPLM.DEL.Product;

namespace ExportBOMToERP {
    public class WorkCenterDal : BaseDal {
        public WorkCenterDal(DEBusinessItem dItem) {
            _dEBusinessItem = dItem;
            this._name = "WorkCenters";
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
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);
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
                    case "Description":
                        row[col] = dEBusinessItem.Name;
                        break;
                    case "CalendarCode":
                        row[col] = "SYSTEM";
                        break;
                    case "ProductLineFlag":
                        row[col] = val == null ? 1 : val;
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
            DataTable dt = new DataTable("WorkCenter");
            dt.Columns.Add("WcId",typeof(int));//工作中心Id（自动编号）  
            dt.Columns.Add("WcCode");//	工作中心代号  
            dt.Columns.Add("Description");//	名称  
            dt.Columns.Add("CalendarCode");//	行事历Id  （工作日历代号）
            dt.Columns.Add("ProductLineFlag", typeof(int));//	是否生产线  
            dt.Columns.Add("DeptCode");//隶属部门
            return dt;
        }
    }
}
