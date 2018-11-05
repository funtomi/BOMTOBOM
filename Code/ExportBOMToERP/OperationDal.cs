using System;
using System.Data;
using System.Xml;
using Thyt.TiPLM.DEL.Product;

namespace ExportBOMToERP {
    public class OperationDal : BaseDal {
        public OperationDal(DEBusinessItem dItem) {
            _dEBusinessItem = dItem;
            this._name = "Operation";
            _filePath = BuildFilePath(dItem, _name);
        }

        private bool _hasRss = false;

        protected override XmlDocument BuildXmlDocment(string operatorStr) {
            XmlDocument doc = CreateXmlSchema(_name, _dEBusinessItem, operatorStr);
            //OperationDetail
            DataTable dt = GetOperationDetailDt(_dEBusinessItem);
            dt.WriteXml(_filePath);
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            var node = doc.ImportNode(docTemp.DocumentElement, true);
            string path = string.Format("ufinterface//{0}", _name);
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);
            //OperationRes
            if (_hasRss) {
                doc = AddChildXmlDocument(doc, _dEBusinessItem);
            }
            //dt = GetOperationResDt(_dEBusinessItem);
            //dt.WriteXml(_filePath);
            //docTemp = new XmlDocument();
            //docTemp.Load(_filePath);
            //node = doc.ImportNode(docTemp.DocumentElement, true);
            //doc.SelectSingleNode(path).AppendChild(node.FirstChild);
            //OperationInsp
            dt = GetOperationInspDt(_dEBusinessItem);
            if (dt!=null) {
                dt.WriteXml(_filePath);
                docTemp = new XmlDocument();
                docTemp.Load(_filePath);
                node = doc.ImportNode(docTemp.DocumentElement, true);
                doc.SelectSingleNode(path).AppendChild(node.FirstChild);
            }
            
            return doc;
        }

        private XmlDocument AddChildXmlDocument(XmlDocument doc, DEBusinessItem baseItem) {
            if (doc == null || baseItem == null) {
                return doc;
            }
            var links = GetLinks(baseItem, "GXTORESOURCEDOC");//工序和资源
            if (links == null || links.Count == 0) {
                return doc;
            }
            for (int i = 0; i < links.BizItems.Count; i++) {

                var item = links.BizItems[i] as DEBusinessItem;//资源对象
                if (item == null) {
                    continue;
                }
                var relation = links.RelationList[i] as DERelation2;//工序和资源关系 
                var componentDt = GetOperationResDt(baseItem, item, relation);
                doc = AddComponentNode(doc, componentDt);
            }
            return doc;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetOperationDetailDt(DEBusinessItem dEBusinessItem) {
            if (dEBusinessItem == null) {
                return null;
            }
            var dt = BuildOperationDetailDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = dEBusinessItem.GetAttrValue(dEBusinessItem.ClassName, col.ColumnName.ToUpper());

                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "WcCode":
                        row[col] = val == null ? DBNull.Value : val;
                        _hasRss = val != null;
                        break;
                    
                    case "DeliverDays":
                        row[col] = val == null ? 0 : val;
                        break;
                    case "RltOptionFlag":
                    case "SubFlag":
                    case "IsBF":
                    case "IsPlanSub":
                        row[col] = val == null ? false : val;
                        break;
                    case "IsFee":
                    case "IsReport":
                        row[col] = val == null ? true : val;
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
        private DataTable BuildOperationDetailDt() {
            DataTable dt = new DataTable("OperationDetail");
            dt.Columns.Add("OperationId", typeof(int));//	标准工序资料ID 	int
            dt.Columns.Add("OpCode");//	标准工序代号  	nvarchar
            dt.Columns.Add("Description");//	说明  	nvarchar
            dt.Columns.Add("WcCode", typeof(int));//	工作中心代号  	int
            dt.Columns.Add("RltOptionFlag", typeof(bool));//	是/否与选项相关	bit
            dt.Columns.Add("SubFlag", typeof(bool));//	是/否委外工序(1/0)  	bit
            dt.Columns.Add("DeliverDays", typeof(int));//	交货天数 	int	4 
            dt.Columns.Add("IsBF", typeof(bool));//	倒冲工序(0否/1是) 	bit	1 
            dt.Columns.Add("IsFee", typeof(bool));//	计费点(0否/1是) 	bit	1 
            dt.Columns.Add("IsPlanSub", typeof(bool));//	计划委外工序(0否/1是) 	bit	1 
            dt.Columns.Add("IsReport", typeof(bool));//	报告点 (0否/1是) 	bit	1 

            dt.Columns.Add("SVendorCode");//	委外商代号  	nvarchar
            dt.Columns.Add("Remark");//	备注  	nvarchar
            dt.Columns.Add("OperationInspId", typeof(int));//	标准工序检验资料Id 	int
            dt.Columns.Add("Define22	");//表体自定义项1 	nvarchar
            dt.Columns.Add("Define23");//	表体自定义项2 	nvarchar
            dt.Columns.Add("Define24");//	表体自定义项3 	nvarchar
            dt.Columns.Add("Define25	");//表体自定义项4 	nvarchar
            dt.Columns.Add("Define26	");//表体自定义项5 	float
            dt.Columns.Add("Define27", typeof(float));//	表体自定义项6 	float
            dt.Columns.Add("Define28");//	表体自定义项7 	nvarchar
            dt.Columns.Add("Define29	");//表体自定义项8 	nvarchar
            dt.Columns.Add("Define30	");//表体自定义项9 	nvarchar
            dt.Columns.Add("Define31");//	表体自定义项10 	nvarchar
            dt.Columns.Add("Define32");//	表体自定义项11 	nvarchar
            dt.Columns.Add("Define33");//	表体自定义项12 	nvarchar
            dt.Columns.Add("Define34", typeof(int));//	表体自定义项13 	int
            dt.Columns.Add("Define35", typeof(int));//	表体自定义项14 	int
            dt.Columns.Add("Define36", typeof(DateTime));//	表体自定义项15 	datetime
            _dateNames.Add("Define36");
            dt.Columns.Add("Define37	", typeof(DateTime));//表体自定义项16 	datetime
            _dateNames.Add("Define37");
            return dt;
        }

        /// <summary>
        /// 获取OperationRes数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private DataTable GetOperationResDt(DEBusinessItem baseItem, DEBusinessItem item, DERelation2 relation) {
            if (item == null) {
                return null;
            }

            var dt = BuildOperationResDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = item.GetAttrValue(item.ClassName, col.ColumnName.ToUpper());
                var rltVal = relation.GetAttrValue(col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = rltVal == null ? DBNull.Value : rltVal;
                        break;
                    case "OperationId":
                        val = baseItem.GetAttrValue(baseItem.ClassName, col.ColumnName.ToUpper());
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "BaseType":
                        row[col] = val == null ? 1 : val;
                        break;
                    case "EfficientRate":
                        row[col] = val == null ? 100 : val;
                        break;
                }
            }
            dt.Rows.Add(row);
            return dt;
        }
        /// <summary>
        /// 构建OperationRes节点
        /// </summary>
        /// <returns></returns>
        private DataTable BuildOperationResDt() {
            DataTable dt = new DataTable("OperationRes");
            dt.Columns.Add("OperationResId", typeof(int));//	工序的相关资源资料ID 	int	4 
            dt.Columns.Add("OperationId", typeof(int));//	标准工序ID  	int	4 
            dt.Columns.Add("SortSeq", typeof(int));//	行号  	int	4 
            dt.Columns.Add("ResCode", typeof(int));//	资源Id  	int	4 
            dt.Columns.Add("BaseType", typeof(int));//	基准类型 	tinyint	1 
            dt.Columns.Add("BaseQtyN", typeof(decimal));//	基本用量-分子  	Udt_QTY	13 
            dt.Columns.Add("BaseQtyD", typeof(decimal));//	基本用量分母  	Udt_QTY	13 
            dt.Columns.Add("PlanFlag	", typeof(int));//计划否	tinyint	1 
            dt.Columns.Add("ResActivity");//	资源活动  	nvarchar	60 
            dt.Columns.Add("ResQty", typeof(decimal));//	资源个数 	Udt_QTY	13 
            dt.Columns.Add("EfficientRate", typeof(decimal));//	效率% 	Udt_Rate100	5 
            dt.Columns.Add("FeeType", typeof(int));//	计费类型 	tinyint	1 
            return dt;
        }
        /// <summary>
        /// 获取OperationRes数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private DataTable GetOperationInspDt(DEBusinessItem item) {
            if (item == null) {
                return null;
            }

            var dt = BuildOperationInspDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = item.GetAttrValue(item.ClassName, col.ColumnName.ToUpper());

                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "QTMethod":
                        if (val==null) {
                            return null;
                        }
                        row[col] = val;
                        break;
                }
            }
            dt.Rows.Add(row);
            return dt;
        }
        /// <summary>
        /// 构建OperationRes节点
        /// </summary>
        /// <returns></returns>
        private DataTable BuildOperationInspDt() {
            DataTable dt = new DataTable("OperationInsp");
            dt.Columns.Add("OperationResId", typeof(int));//	工序的相关资源资料ID 	int	4  
            dt.Columns.Add("OperationInspId", typeof(int));//	工序相关的检验资料ID	int
            dt.Columns.Add("QTMethod", typeof(int));//	检验方式	int
            dt.Columns.Add("DtMethod", typeof(int));//	抽检方案	int
            dt.Columns.Add("DtRate", typeof(decimal));//	抽检率  	Udt_Rate100
            dt.Columns.Add("DtNum", typeof(decimal));//	抽检量  	Udt_QTY
            dt.Columns.Add("DtStyle", typeof(int));//	抽检方式	smallint
            dt.Columns.Add("QcProjectCode", typeof(int));//	质量检验方案  	int
            dt.Columns.Add("QtLevel", typeof(int));//	检验水平  	smallint
            dt.Columns.Add("AqlVal");//	AQL值  	nvarchar
            return dt;
        }
    }
}
