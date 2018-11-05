using System;
using System.Data;
using System.Xml;
using Thyt.TiPLM.DEL.Product;

namespace ExportBOMToERP {
    public class RoutingDal : BaseDal {
        public RoutingDal(DEBusinessItem dItem) {
            _dEBusinessItem = dItem;
            this._name = "Routing";
            _filePath = BuildFilePath(dItem, _name);
        }

        protected override XmlDocument BuildXmlDocment(string operatorStr) {
            XmlDocument doc = CreateXmlSchema(_name, _dEBusinessItem, operatorStr);
            //Version
            DataTable dt = GetVersionDt(_dEBusinessItem);
            dt.WriteXml(_filePath);
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            var node = doc.ImportNode(docTemp.DocumentElement, true);
            string path = string.Format("ufinterface//{0}", _name);
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);
            //Part
            dt = GetPartDt(_dEBusinessItem);
            dt.WriteXml(_filePath);
            docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            node = doc.ImportNode(docTemp.DocumentElement, true);
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);

            //RoutingDetail/RoutingInsp/RoutingRes
            doc = GetChildDt(_dEBusinessItem, doc);

            return doc;
        }

        private XmlDocument GetChildDt(DEBusinessItem baseItem, XmlDocument doc) {
            if (baseItem == null || doc == null) {
                return doc;
            }
            var linkItems = GetLinks(baseItem, "GYGCKHGX");//todo：修改关系名(工艺路线和工序)
            if (linkItems == null || linkItems.Count == 0) {
                return doc;
            }
            DataTable dt;
            XmlDocument docTemp;
            string path = string.Format("ufinterface//{0}", _name);

            for (int i = 0; i < linkItems.BizItems.Count; i++) {
                var item = linkItems.BizItems[i] as DEBusinessItem;//工序对象
                var relation = linkItems.RelationList[i] as DERelation2;
                if (item == null) {
                    continue;
                }
                //RoutingDetail
                dt = GetRoutingDetailDt(baseItem, item, relation);
                dt.WriteXml(_filePath);
                docTemp = new XmlDocument();
                docTemp.Load(_filePath);
                doc.SelectSingleNode(path).AppendChild(doc.ImportNode(docTemp.DocumentElement, true).FirstChild);
                //RoutingInsp
                dt = GetRoutingInspDt(baseItem, item, relation);
                if (dt!=null) {
                    dt.WriteXml(_filePath);
                    docTemp = new XmlDocument();
                    docTemp.Load(_filePath);
                    doc.SelectSingleNode(path).AppendChild(doc.ImportNode(docTemp.DocumentElement, true).FirstChild);
                }
                
                //RoutingRes
                //dt = GetRoutingResDt(baseItem, item, relation);
                //dt.WriteXml(_filePath);
                //docTemp = new XmlDocument();
                //docTemp.Load(_filePath);
                //doc.SelectSingleNode(path).AppendChild(doc.ImportNode(docTemp.DocumentElement, false).FirstChild);
            }
            return doc;

        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetVersionDt(DEBusinessItem dEBusinessItem) {
            if (dEBusinessItem == null) {
                return null;
            }
            var dt = BuildVersionDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = dEBusinessItem.GetAttrValue(dEBusinessItem.ClassName, col.ColumnName.ToUpper());

                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "RountingType":
                        row[col] = val == null ? 1 : val;
                        break;
                    case "Version":
                        row[col] = dEBusinessItem.LastRevision;
                        break;
                    case "Status":
                        row[col] = val == null ? 3 : val;
                        break;
                    case "RunCardFlag":
                        row[col] = val == null ? false : val;
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
        private DataTable BuildVersionDt() {
            DataTable dt = new DataTable("Version");
            dt.Columns.Add("PRoutingId", typeof(int));// 	料品工艺路线ID 	int	4 
            dt.Columns.Add("RountingType", typeof(int));// 	类型	tinyint	1 
            dt.Columns.Add("Version", typeof(int));//  	// 	版本号"int	4 
            dt.Columns.Add("VersionDesc");// 	版本说明  	nvarchar	60 
            dt.Columns.Add("VersionEffDate", typeof(DateTime));// 	版本生效日期  	datetime	8 
            _dateNames.Add("VersionEffDate");
            dt.Columns.Add("VersionEndDate", typeof(DateTime));// 	版本失效日期  	datetime	8 
            _dateNames.Add("VersionEndDate");
            dt.Columns.Add("IdentCode");// 	替代标识  	nvarchar	20 
            dt.Columns.Add("IdentDesc");// 	替代说明  	nvarchar	60 
            dt.Columns.Add("Define1");// 	表头自定义项1	nvarchar	20 
            dt.Columns.Add("Define2");// 	表头自定义项2	nvarchar	20 
            dt.Columns.Add("Define3");// 	表头自定义项3	nvarchar	20 
            dt.Columns.Add("Define4", typeof(DateTime));// 	表头自定义项4	datetime	8 
            _dateNames.Add("Define4");
            dt.Columns.Add("Define5", typeof(int));// 	表头自定义项5	int	4 
            dt.Columns.Add("Define6", typeof(DateTime));// 	表头自定义项6	datetime	8 
            _dateNames.Add("Define6");
            dt.Columns.Add("Define7", typeof(float));// 	表头自定义项7	float	8 
            dt.Columns.Add("Define8");// 	表头自定义项8	nvarchar	4 
            dt.Columns.Add("Define9");// 	表头自定义项9	nvarchar	8 
            dt.Columns.Add("Define10");// 	表头自定义项10	nvarchar	60 
            dt.Columns.Add("Define11");// 	表头自定义项11	nvarchar	120 
            dt.Columns.Add("Define12");// 	表头自定义项12	nvarchar	120 
            dt.Columns.Add("Define13");// 	表头自定义项13	nvarchar	120 
            dt.Columns.Add("Define14");// 	表头自定义项14	nvarchar	120 
            dt.Columns.Add("Define15", typeof(int));// 	表头自定义项15	int	4 
            dt.Columns.Add("Define16", typeof(float));// 	表头自定义项15	float	8 
            dt.Columns.Add("Status", typeof(int));// 	原始状态(1:新建/3:审核/4:停用) 	tinyint	1 
            dt.Columns.Add("OpUnitCode");//	辅助计量单位 	nvarchar	35 
            dt.Columns.Add("ChangeRate", typeof(decimal));//	换算率 	Udt_ChangeRate	13 
            dt.Columns.Add("RunCardFlag", typeof(bool));//	启用流转卡 	bit	1 
            return dt;
        }

        /// <summary>
        /// 构建节点属性结构
        /// </summary>
        /// <returns></returns>
        private DataTable BuildPartDt() {
            DataTable dt = new DataTable("Part");
            dt.Columns.Add("PRoutingId", typeof(int));// 	料品工艺路线表头ID  	int	4 
            dt.Columns.Add("PartId", typeof(int));// 	料品Id  	int	4 
            dt.Columns.Add("InvCode");// 	存货编码  	nvarchar	20 
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetPartDt(DEBusinessItem dEBusinessItem) {
            if (dEBusinessItem == null) {
                return null;
            }
            var dt = BuildPartDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = dEBusinessItem.GetAttrValue(dEBusinessItem.ClassName, col.ColumnName.ToUpper());

                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
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
        private DataTable BuildRoutingDetailDt() {
            DataTable dt = new DataTable("RoutingDetail");//
            dt.Columns.Add("PRoutingId", typeof(int));// 	料品工艺路线资料ID 	int	4
            dt.Columns.Add("PRoutingDId", typeof(int));// 	料品工艺路线资料ID 	int	
            dt.Columns.Add("OpSeq");// 	工序序号  	nchar	4 
            dt.Columns.Add("OperationCode");// 	标准工序Id  	int	4 
            dt.Columns.Add("Description");// 	标准工序说明  	nvarchar	60 
            dt.Columns.Add("WcCode");// 	工作中心代号  	int	4 
            dt.Columns.Add("EffBegDate");// 	生效日期  	datetime	8 
            dt.Columns.Add("EffEndDate");// 	失效日期  	datetime	8 
            dt.Columns.Add("SubFlag", typeof(bool));// 	是/否委外工序(1/0)  	bit	1 
            dt.Columns.Add("SVendorCode");// 	委外商代号  	nvarchar	20 
            dt.Columns.Add("RltOptionFlag", typeof(bool));// 	是否选项相关  	bit	1 
            dt.Columns.Add("LtPercent", typeof(int));// 	制造提前百分比  	int	4 
            dt.Columns.Add("Remark");// 	备注  	nvarchar	255 
            dt.Columns.Add("PRoutinginspId", typeof(int));// 	料品工艺路线检验资料Id 	int	4 
            dt.Columns.Add("Define22");// 	表体自定义项1	nvarchar	60 
            dt.Columns.Add("Define23");// 	表体自定义项2	nvarchar	60 
            dt.Columns.Add("Define24");// 	表体自定义项3	nvarchar	60 
            dt.Columns.Add("Define25");// 	表体自定义项4	nvarchar	60 
            dt.Columns.Add("Define26");// 	表体自定义项5	float	8 
            dt.Columns.Add("Define27");// 	表体自定义项6	float	8 
            dt.Columns.Add("Define28");// 	表体自定义项7	nvarchar	120 
            dt.Columns.Add("Define29");// 	表体自定义项8	nvarchar	120 
            dt.Columns.Add("Define30");// 	表体自定义项9	nvarchar	120 
            dt.Columns.Add("Define31");// 	表体自定义项10	nvarchar	120 
            dt.Columns.Add("Define32");// 	表体自定义项11	nvarchar	120 
            dt.Columns.Add("Define33");// 	表体自定义项12	nvarchar	120 
            dt.Columns.Add("Define34", typeof(int));// 	表体自定义项13	int	4 
            dt.Columns.Add("Define35", typeof(int));// 	表体自定义项14	int	4 
            dt.Columns.Add("Define36");// 	表体自定义项15	datetime	8 
            dt.Columns.Add("Define37");// 	表体自定义项16	datetime	8 
            dt.Columns.Add("ReportFlag", typeof(bool));// 	报告点 	bit	1 
            dt.Columns.Add("BFFlag", typeof(bool));// 	倒冲工序 	bit	1 
            dt.Columns.Add("FeeFlag", typeof(bool));// 	计费点 	bit	1 
            dt.Columns.Add("PlanSubFlag", typeof(bool));// 	计划委外工序 	bit	1 
            dt.Columns.Add("DeliveryDays", typeof(int));// 	交货天数 	int	4 
            dt.Columns.Add("AuxUnitCode");// 	辅助计量单位 	nvarchar	35 
            dt.Columns.Add("ChangeRate");// 	换算率 	Udt_ChangeRate	13 
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private DataTable GetRoutingDetailDt(DEBusinessItem baseItem, DEBusinessItem item, DERelation2 relation) {
            if (item == null) {
                return null;
            }
            var dt = BuildRoutingDetailDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = item.GetAttrValue(item.ClassName, col.ColumnName.ToUpper());
                var rltVal = relation.GetAttrValue(col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "PRoutingId":
                        val = baseItem.GetAttrValue(baseItem.ClassName, col.ColumnName.ToUpper());
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "PRoutingDId":
                        val = item.GetAttrValue(item.ClassName, "OPERATIONID");
                        row[col] = val == null ? DBNull.Value : val;
                        
                        break;
                    case "OperationCode":
                        val = item.GetAttrValue(item.ClassName, "OPCODE");
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "PRoutinginspId":
                        val = item.GetAttrValue(item.ClassName, "OPERATIONINSPID");
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "Description":
                        row[col] = item.Name;
                        break;
                    case "WcCode":
                    case "EffBegDate":
                    case "EffEndDate":
                    case "OpSeq":
                        row[col] = rltVal == null ? DBNull.Value : rltVal;
                        break;
                    case "SubFlag":
                    case "RltOptionFlag":
                    case "BFFlag":
                    case "FeeFlag":
                    case "PlanSubFlag":
                        row[col] = rltVal == null ? false : val;
                        break;
                    case "DeliveryDays":
                    case "LtPercent":
                        row[col] = rltVal == null ? 0 : val;
                        break;
                    case "ReportFlag":
                        row[col] = rltVal == null ? true : val;
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
        private DataTable BuildRoutingInspDt() {
            DataTable dt = new DataTable("RoutingInsp");//
            dt.Columns.Add("PRoutinginspId", typeof(int));// 	料品工艺路线工序检验资料ID 	int	4 
            dt.Columns.Add("QtMethod", typeof(int));// 	检验方式  	int	4 
            dt.Columns.Add("DtMethod", typeof(int));// 	抽检方案  	smallint	2 
            dt.Columns.Add("DtRate", typeof(decimal));// 	抽检率%  	Udt_Rate100	5 
            dt.Columns.Add("DtNum", typeof(decimal));// 	抽检数量  	Udt_QTY	13 
            dt.Columns.Add("DtStyle", typeof(int));// 	抽检方式 	smallint	2 
            dt.Columns.Add("DtUnit");// 	检验计量单位 	nvarchar	35 
            dt.Columns.Add("QtLevel", typeof(int));// 	检验水平  	smallint	2 
            dt.Columns.Add("QcProjectCode", typeof(int));// 	质量检验方案  	int	4 
            dt.Columns.Add("AqlVal");// 	AQL值  	nvarchar	20 
            dt.Columns.Add("CruleCode");// 	自定义抽检规则 	nvarchar	20 
            dt.Columns.Add("ItestRule", typeof(int));// 	检验规则 	tinyint	1 
            dt.Columns.Add("OpTransType", typeof(int));// 	工序转移 	tinyint	1 
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetRoutingInspDt(DEBusinessItem baseItem, DEBusinessItem item, DERelation2 relation) {
            if (item == null) {
                return null;
            }
            var dt = BuildRoutingInspDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = item.GetAttrValue(item.ClassName, col.ColumnName.ToUpper());
                var rltVal = relation.GetAttrValue(col.ColumnName);
                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "PRoutinginspId":
                        val = item.GetAttrValue(item.ClassName, "OPERATIONINSPID");
                        if (val==null) {
                            return null;
                        }
                        row[col] = val;
                        break;
                    case "OpTransType"://工序转移默认“手动”
                    case "QtMethod":
                    case "ItestRule":
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
        private DataTable BuildRoutingResDt() {
            DataTable dt = new DataTable("RoutingRes");//
            dt.Columns.Add("PRoutingResId", typeof(int));// 	料品工艺路线工序资源资料ID 	int	4 
            dt.Columns.Add("PRoutingDId", typeof(int));// 	料品工艺路线明细ID  	int	4 
            dt.Columns.Add("ResSeq", typeof(int));// 	序号  	int	4 
            dt.Columns.Add("ResCode", typeof(int));// 	资源代号  	int	4 
            dt.Columns.Add("BaseType", typeof(int));// 	基准类型 	tinyint	1 
            dt.Columns.Add("BaseQtyN", typeof(decimal));// 	基本用量-分子  	Udt_QTY	13 
            dt.Columns.Add("BaseQtyD", typeof(decimal));// 	Udt_QTY	13 	Tru
            dt.Columns.Add("PlanFlag", typeof(int));// 	计划否:是/否/前一个/下一个 	tinyint	1 
            dt.Columns.Add("ResActivity");// 	资源活动  	nvarchar	60 
            dt.Columns.Add("ResQty", typeof(decimal));// 	资源个数 	Udt_QTY	13 
            dt.Columns.Add("EffRate", typeof(decimal));// 	效率% 	Udt_Rate100	5 
            dt.Columns.Add("FeeType", typeof(int));// 	计费类型 	tinyint	1 
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetRoutingResDt(DEBusinessItem baseItem, DEBusinessItem item, DERelation2 relation) {
            if (item == null) {
                return null;
            }
            var dt = BuildRoutingInspDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = item.GetAttrValue(item.ClassName, col.ColumnName.ToUpper());
                row[col] = val == null ? DBNull.Value : val;
            }
            dt.Rows.Add(row);
            return dt;
        }

    }
}
