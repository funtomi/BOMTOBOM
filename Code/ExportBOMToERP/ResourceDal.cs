using System;
using System.Data;
using System.Xml;
using Thyt.TiPLM.DEL.Product;

namespace ExportBOMToERP {
    public class ResourceDal : BaseDal {
        public ResourceDal(DEBusinessItem dItem) {
            _dEBusinessItem = dItem;
            this._name = "Resource";
            _filePath = BuildFilePath(dItem, _name);
        }

        protected override XmlDocument BuildXmlDocment(string operatorStr) {
            XmlDocument doc = CreateXmlSchema(_name, _dEBusinessItem, operatorStr);
            DataTable dt = GetHeadDt(_dEBusinessItem);
            dt.WriteXml(_filePath);
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            var node = doc.ImportNode(docTemp.DocumentElement, true);
            string path = string.Format("ufinterface//{0}", _name);
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);
            //child
            doc = BuildChildXmlDoc(doc, _dEBusinessItem);
            return doc;
        }

        /// <summary>
        /// 构建子结构
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="_dEBusinessItem"></param>
        /// <returns></returns>
        private XmlDocument BuildChildXmlDoc(XmlDocument doc, DEBusinessItem baseItem) {
            if (baseItem == null || doc == null) {
                return doc;
            }
            var linkItems = GetLinks(baseItem, "RESOURCETOWORKCEN");//todo：修改关系名(资源和工作中心)
            if (linkItems == null || linkItems.Count == 0) {
                return doc;
            }
            for (int i = 0; i < linkItems.BizItems.Count; i++) {
                var item = linkItems.BizItems[i] as DEBusinessItem;//工作中心对象
                if (item == null) {
                    continue;
                }
                var relation = linkItems.RelationList[i] as DERelation2;//资源和工作中心关系
                var dt = GetBodyDt(baseItem, item, relation);
                AddComponentNode(doc, dt);
            }
            return doc;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private DataTable GetHeadDt(DEBusinessItem item) {
            if (item == null) {
                return null;
            }
            var dt = BuildHeadDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = item.GetAttrValue(item.ClassName, col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "Description":
                        row[col] = item.Name;
                        break;
                    case "ResType":
                    case "FeeType":
                    case "BaseType":
                        row[col] = val == null ? 1 : val; ;
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
        private DataTable BuildHeadDt() {
            DataTable dt = new DataTable("Header");
            dt.Columns.Add("ResId", typeof(int));//	资源资料ID 	int
            dt.Columns.Add("ResCode");//	资源代号  	nvarchar
            dt.Columns.Add("Description");//	说明  	nvarchar
            dt.Columns.Add("ResType", typeof(int));//	资源类型 	tinyint
            dt.Columns.Add("FeeType", typeof(int));//	计费类型 	tinyint
            dt.Columns.Add("BaseType", typeof(int));//	基准类型	tinyint
            dt.Columns.Add("Define1");//	表头自定义项1	nvarchar
            dt.Columns.Add("Define2");//	表头自定义项2	nvarchar
            dt.Columns.Add("Define3");//	表头自定义项3	nvarchar
            dt.Columns.Add("Define4");//	表头自定义项4	datetime
            dt.Columns.Add("Define5", typeof(int));//	表头自定义项5	int
            dt.Columns.Add("Define6", typeof(DateTime));//	表头自定义项6	datetime
            _dateNames.Add("Define6");
            dt.Columns.Add("Define7", typeof(float));//	表头自定义项7	float
            dt.Columns.Add("Define8");//	表头自定义项8	nvarchar
            dt.Columns.Add("Define9");//	表头自定义项9	nvarchar
            dt.Columns.Add("Define10	");//表头自定义项10	nvarchar
            dt.Columns.Add("Define11	");//表头自定义项11	nvarchar
            dt.Columns.Add("Define12");//	表头自定义项12	nvarchar
            dt.Columns.Add("Define13");//	表头自定义项12	nvarchar
            dt.Columns.Add("Define14");//	表头自定义项12	nvarchar
            dt.Columns.Add("Define15", typeof(int));//	表头自定义项15	int
            dt.Columns.Add("Define16", typeof(float));//	表头自定义项16	float
            dt.Columns.Add("ReportFlag", typeof(bool));//	是否报告点	bit
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private DataTable GetBodyDt(DEBusinessItem baseItem, DEBusinessItem item, DERelation2 relation) {
            if (item == null) {
                return null;
            }
            var dt = BuildBodyDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = baseItem.GetAttrValue(baseItem.ClassName, col.ColumnName.ToUpper());
                var rltVal = relation.GetAttrValue(col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = rltVal == null ? DBNull.Value : rltVal;
                        break;
                    case "ResId":
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "Qty":
                        row[col] = rltVal == null ? 1.00m : rltVal;
                        break;
                    case "KeyFlag":
                    case "FiniteScheduleRel":
                        row[col] = rltVal == null ? 0 : rltVal;
                        break;
                    case "CrpFlag":
                        row[col] = rltVal == null ? 1 : rltVal;
                        break;
                    case "UseRate":
                    case "EfficiencyRate":
                    case "OverRate":
                        row[col] = rltVal == null ? 100.00m : rltVal;
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
        private DataTable BuildBodyDt() {
            DataTable dt = new DataTable("Body");
            dt.Columns.Add("WcResId", typeof(int));//	资源资料明细ID	int
            dt.Columns.Add("ResId", typeof(int));//	资源资料ID 	int
            dt.Columns.Add("WcCode");//	工作中心代号	nvarchar
            dt.Columns.Add("Qty", typeof(decimal));//	可用数量	Udt_QTY
            dt.Columns.Add("KeyFlag", typeof(int));//	关键资源	bit
            dt.Columns.Add("CrpFlag", typeof(int));//	计算产能	bit
            dt.Columns.Add("UseRate", typeof(decimal));//	利用率	Udt_Rate100
            dt.Columns.Add("EfficiencyRate", typeof(decimal));//	效率	Udt_Rate100
            dt.Columns.Add("FiniteScheduleRel", typeof(int));//	有限排程相关	bit
            dt.Columns.Add("OverRate", typeof(decimal));//	超载百分比	Udt_Rate100
            dt.Columns.Add("Define22");//	表头自定义项22	nvarchar
            dt.Columns.Add("Define23	");//表头自定义项23	nvarchar
            dt.Columns.Add("Define24	");//表头自定义项24	nvarchar
            dt.Columns.Add("Define25	");//表头自定义项25	nvarchar
            dt.Columns.Add("Define26	", typeof(float));//表头自定义项26	float
            dt.Columns.Add("Define27	", typeof(float));//表头自定义项27	float
            dt.Columns.Add("Define28	");//表头自定义项28	nvarchar
            dt.Columns.Add("Define29	");//表头自定义项29	nvarchar
            dt.Columns.Add("Define30	");//表头自定义项30	nvarchar
            dt.Columns.Add("Define31	");//表头自定义项31	nvarchar
            dt.Columns.Add("Define32	");//表头自定义项32	nvarchar
            dt.Columns.Add("Define33");//	表头自定义项33	nvarchar
            dt.Columns.Add("Define34	", typeof(int));//表头自定义项34	int
            dt.Columns.Add("Define35	", typeof(int));//表头自定义项35	int
            dt.Columns.Add("Define36	", typeof(DateTime));//表头自定义项36	datetime
            _dateNames.Add("Define36");
            dt.Columns.Add("Define37	", typeof(DateTime));//表头自定义项37	datetime
            _dateNames.Add("Define37");
            dt.Columns.Add("FeeRate", typeof(decimal));//	资源费率	decimal

            return dt;
        }
    }
}
