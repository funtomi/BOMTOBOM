using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Thyt.TiPLM.CLT.Admin.BPM;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Product2;
using Thyt.TiPLM.UIL.Common;

namespace ExportBOMToERP {
    public class BomDal : BaseDal {
        public BomDal(DEBusinessItem dItem) {
            _dEBusinessItem = dItem;
            this._name = "Bom";
            _filePath = BuildFilePath(dItem, _name);
        }

        protected override XmlDocument BuildXmlDocment(string operatorStr) {
            var linkItems = GetLinks(_dEBusinessItem, "PARTTOPART");//todo：修改关系名(零件和零件)
            if (linkItems==null||linkItems.Count==0) {
                return null;
            }
            XmlDocument doc = CreateXmlSchema(_name, _dEBusinessItem, operatorStr);
            //string attr = "CODE";
            //var attrVal = _dEBusinessItem.GetAttrValue(_dEBusinessItem.ClassName, "INVCODE");
            //if (attr == null || string.IsNullOrEmpty(attr.ToString())) {
            //    return doc;
            //}
            //DEBusinessItem bomItem = GetItemByAttr("PART", attrVal.ToString(), attr);
            ////DEBusinessItem bomItem = GetItemById("PART", id.ToString());
            //if (bomItem == null) {
            //    bomItem = GetItemByAttr("TIGZ", attrVal.ToString(), attr);
            //    if (bomItem == null) {
            //        bomItem = GetItemByAttr("TIPART", attrVal.ToString(), attr);
            //        if (bomItem == null) {
            //            return doc;
            //        }
            //    }
            //}
            //Version
            DataTable dt = GetVertionDt(_dEBusinessItem);
            dt.WriteXml(_filePath);
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            var node = doc.ImportNode(docTemp.DocumentElement, true);
            string path = string.Format("ufinterface//{0}", _name);
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);
            //Parent
            dt = GetParentDt(_dEBusinessItem);
            dt.WriteXml(_filePath);
            docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            node = doc.ImportNode(docTemp.DocumentElement, true);
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);
            //component
            doc = BuildComponentXmlDocument(_dEBusinessItem, doc);

            return doc;
        }

        private DEBusinessItem GetItemByAttr(string className, string val, string attr) {
            if (string.IsNullOrEmpty(val)) {
                return null;
            }
            //var item = PLItem.Agent.GetBizItemByIteration((Guid)id, "TIPART", ClientData.UserGlobalOption.CurView, BPMClient.UserID, BizItemMode.SmartBizItem);
            Hashtable ht = new Hashtable();
            ht.Add("Id", val);
            DEBusinessItem[] items = PLItem.Agent.GetLatestBizItemsByAttr(className, ht, BPMClient.UserID);
            if (items == null || items.Length == 0) {
                return null;
            }
            foreach (DEBusinessItem item in items) {
                var attrVal = item.GetAttrValue(className, attr);
                if (attrVal != null && attrVal.Equals(val)) {
                    return item;
                }
            }
            //DEBusinessItem item = items.First(p => p.Id == id.ToString());
            return null;
        }

        /// <summary>
        /// 通过id获取物料对象
        /// </summary>
        /// <param name="baseItem"></param>
        /// <returns></returns>
        private DEBusinessItem GetItemById(string className, string id) {
            if (string.IsNullOrEmpty(id)) {
                return null;
            }
            //var item = PLItem.Agent.GetBizItemByIteration((Guid)id, "TIPART", ClientData.UserGlobalOption.CurView, BPMClient.UserID, BizItemMode.SmartBizItem);
            Hashtable ht = new Hashtable();
            ht.Add("Id", id);
            DEBusinessItem[] items = PLItem.Agent.GetLatestBizItemsByAttr(className, ht, BPMClient.UserID);
            if (items == null || items.Length == 0) {
                return null;
            }
            DEBusinessItem item = items.First(p => p.Id == id.ToString());
            return item;
        }

        /// <summary>
        /// 构建子节点，只传单层物料
        /// </summary>
        /// <param name="_dEBusinessItem"></param>
        /// <returns></returns>
        private XmlDocument BuildComponentXmlDocument(DEBusinessItem baseItem, XmlDocument doc) {
            if (baseItem == null || doc == null) {
                return doc;
            }
            var linkItems = GetLinks(baseItem, "PARTTOPART");//todo：修改关系名(零件和零件)
            if (linkItems == null || linkItems.Count == 0) {
                return doc;
            }
            List<string> itemIds = new List<string>();
            int seqNo = 0;
            for (int i = 0; i < linkItems.BizItems.Count; i++) {
                var item = linkItems.BizItems[i] as DEBusinessItem;//子件对象
                if (item == null) {
                    continue;
                }
                var relation = linkItems.RelationList[i] as DERelation2;//母件和子件关系关系
                //var links = GetLinks(item, "GXKWL");//工序和零件
                //if (links == null || links.Count == 0) {
                //    continue;
                //}
                //for (int j = 0; j < links.BizItems.Count; j++) {
                //    var itemBom = links.BizItems[j] as DEBusinessItem;//物料对象
                //    var rlt = links.RelationList[j] as DERelation2;//工序和物料关系
                //    if (itemIds.Contains(itemBom.Id)) {
                //        continue;
                //    }
                //    itemIds.Add(itemBom.Id);//过滤重复物料
                var componentDt = GetComponentDt(baseItem, relation, item,ref seqNo);
                    doc = AddComponentNode(doc, componentDt);
                    var cmpntOptDt = GetComponentOptDt(item, relation, seqNo);
                    doc = AddComponentNode(doc, cmpntOptDt);
                    //var cpntSubDt = GetComponentSubDt(baseItem, relation);
                    //doc = AddComponentNode(doc, cpntSubDt);
                    //var cpntLotDt = GetComponentLocDt(baseItem, relation);
                    //doc = AddComponentNode(doc, cpntLotDt);
                //}
            }
            return doc;
        }


        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private DataTable GetVertionDt(DEBusinessItem item) {
            if (item == null) {
                return null;
            }
            var dt = BuildVersionDt();
            var row = dt.NewRow();

            foreach (DataColumn col in dt.Columns) {
                var val = item.GetAttrValue(item.ClassName, col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "Version":
                        row[col] = item.LastRevision;
                        break;
                    case "Status":
                        row[col] = val == null ? 3 : val;
                        break;
                    case "BomType":
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
        private DataTable BuildVersionDt() {
            DataTable dt = new DataTable("Version");
            dt.Columns.Add("BomId", typeof(int));//		编码	int	4 
            dt.Columns.Add("BomType", typeof(int));//		物料清单类型	tinyint	1 
            dt.Columns.Add("Version", typeof(int));//		版本代号	int	4 
            dt.Columns.Add("VersionDesc");//		版本说明	nvarchar	60 
            dt.Columns.Add("VersionEffDate", typeof(DateTime));//		版本生效日	datetime	8 
            _dateNames.Add("VersionEffDate");
            dt.Columns.Add("IdentCode");//		替代标识	nvarchar	20 
            dt.Columns.Add("IdentDesc");//		替代说明	nvarchar	60 
            dt.Columns.Add("Define1");//		表头自定义项1 	nvarchar	20 
            dt.Columns.Add("Define2");//		表头自定义项2 	nvarchar	20 
            dt.Columns.Add("Define3");//		表头自定义项3 	nvarchar	20 
            dt.Columns.Add("Define4", typeof(DateTime));//		表头自定义项4 	datetime	8 
            _dateNames.Add("Define4");
            dt.Columns.Add("Define5", typeof(int));//		表头自定义项5 	int	4 
            dt.Columns.Add("Define6", typeof(DateTime));//		表头自定义项6 	datetime	8 
            _dateNames.Add("Define6");
            dt.Columns.Add("Define7", typeof(float));//		表头自定义项7 	float	8 
            dt.Columns.Add("Define8");//		表头自定义项8 	nvarchar	4 
            dt.Columns.Add("Define9");//		表头自定义项9 	nvarchar	4 
            dt.Columns.Add("Define10");//		表头自定义项10 	nvarchar	60 
            dt.Columns.Add("Define11");//		表头自定义项11 	nvarchar	120 
            dt.Columns.Add("Define12");//		表头自定义项12 	nvarchar	120 
            dt.Columns.Add("Define13");//		表头自定义项13 	nvarchar	120 
            dt.Columns.Add("Define14");//		表头自定义项14 	nvarchar	120 
            dt.Columns.Add("Define15", typeof(int));//		表头自定义项15 	int	4 
            dt.Columns.Add("Define16", typeof(float));//		表头自定义项16 	float	8 
            dt.Columns.Add("Define17", typeof(int));//		表头自定义项17 	int	4 
            dt.Columns.Add("Status", typeof(int));//		状态	tinyint	1 
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private DataTable GetParentDt(DEBusinessItem item) {
            if (item == null) {
                return null;
            }
            var dt = BuildParentDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = item.GetAttrValue(item.ClassName, col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "InvCode":
                        val = item.GetAttrValue(item.ClassName, "CODE");
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "ParentScrap":
                        row[col] = val == null ? 0m : val;
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
        private DataTable BuildParentDt() {
            DataTable dt = new DataTable("Parent");
            dt.Columns.Add("BomId", typeof(int));//	编码	int	4 
            dt.Columns.Add("InvCode");//	存货编码  	nvarchar	20 
            dt.Columns.Add("Free1");//			
            dt.Columns.Add("Free2");//			
            dt.Columns.Add("Free3");//			
            dt.Columns.Add("Free4");//			
            dt.Columns.Add("Free5");//			
            dt.Columns.Add("Free6");//			
            dt.Columns.Add("Free7");//			
            dt.Columns.Add("Free8");//			
            dt.Columns.Add("Free9");//			
            dt.Columns.Add("Free10");//			
            dt.Columns.Add("ParentScrap", typeof(decimal));//	母件损耗 	Udt_Rate	5 
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="bomItem"></param>
        /// <returns></returns>
        private DataTable GetComponentDt(DEBusinessItem baseItem, DERelation2 relation, DEBusinessItem bomItem, ref int seqNo) {
            if (bomItem == null) {
                return null;
            }
            var dt = BuildComponentDt();
            var row = dt.NewRow();
            seqNo++;
            foreach (DataColumn col in dt.Columns) {
                var val = bomItem.GetAttrValue(bomItem.ClassName, col.ColumnName.ToUpper());
                var rltVal = relation.GetAttrValue(col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "BomId":
                        val = baseItem.GetAttrValue(baseItem.ClassName, "BOMID");
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "InvCode":
                        val = bomItem.GetAttrValue(bomItem.ClassName, "CODE");
                        row[col] = val == null ? DBNull.Value : val;
                        break;
                    case "SortSeq":
                    case "OptionsId":
                    case "OpComponentId":
                        row[col] = (seqNo * 10);
                        break;
                    case "OpSeq":
                        row[col] = 0000;//工序不启用，默认为0000
                        break;
                    case "EffBegDate":
                        row[col] = rltVal == null ? DateTime.Parse("2000-01-01") : rltVal;
                        break;
                    case "EffEndDate":
                        row[col] = rltVal == null ? DateTime.Parse("2099-12-31") : rltVal;
                        break;
                    case "FVFlag":
                    case "ProductType":
                        row[col] = rltVal == null ? 1 : rltVal;
                        break;
                    case "BaseQtyN":
                    case "BaseQtyD":
                        row[col] = rltVal == null ? 1m : rltVal;
                        break;
                    case "CompScrap":
                        row[col] = rltVal == null ? 0.000m : rltVal;
                        break;
                    case "ByproductFlag":
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
        private DataTable BuildComponentDt() {
            DataTable dt = new DataTable("Component");
            dt.Columns.Add("OpComponentId", typeof(int));//	子件物料Id  	int
            dt.Columns.Add("BomId", typeof(int));//	编码	int
            dt.Columns.Add("SortSeq", typeof(int));//	序号  	int
            dt.Columns.Add("OpSeq");//	工序代号  	nchar
            dt.Columns.Add("InvCode");//	存货编码  	nvarchar
            dt.Columns.Add("Free1");//	自定义项	nvarchar
            dt.Columns.Add("Free2");//	自定义项	nvarchar
            dt.Columns.Add("Free3");//	自定义项	nvarchar
            dt.Columns.Add("Free4");//	自定义项	nvarchar
            dt.Columns.Add("Free5");//	自定义项	nvarchar
            dt.Columns.Add("Free6");//	自定义项	nvarchar
            dt.Columns.Add("Free7");//	自定义项	nvarchar
            dt.Columns.Add("Free8");//	自定义项	nvarchar
            dt.Columns.Add("Free9");//	自定义项	nvarchar
            dt.Columns.Add("Free10");//	自定义项	nvarchar
            dt.Columns.Add("EffBegDate", typeof(DateTime));//	子件生效日  	datetime
            _dateNames.Add("EffBegDate");
            dt.Columns.Add("EffEndDate", typeof(DateTime));//	子件失效日  	datetime
            _dateNames.Add("EffEndDate");
            dt.Columns.Add("FVFlag", typeof(int));//	固定/变动批量	tinyint
            dt.Columns.Add("BaseQtyN", typeof(decimal));//	基本用量-分子  	Udt_QTY
            dt.Columns.Add("BaseQtyD", typeof(decimal));//基本用量-分母  	Udt_QTY
            dt.Columns.Add("CompScrap", typeof(decimal));//	子件损耗率  	Udt_Rate
            dt.Columns.Add("ByproductFlag", typeof(bool));//	是否联副产品  	bit
            dt.Columns.Add("OptionsId", typeof(int));//	选项资料Id 	int
            dt.Columns.Add("AuxUnitCode");//	辅助单位	nvarchar
            dt.Columns.Add("ChangeRate", typeof(decimal));//	换算率	Udt_ChangeRate
            dt.Columns.Add("AuxBaseQtyN", typeof(decimal));//	辅助基本用量	Udt_QTY
            dt.Columns.Add("ProductType", typeof(int));//	产出类型	tinyint
            dt.Columns.Add("Define22");//	表体自定义项1 	nvarchar
            dt.Columns.Add("Define23");//	表体自定义项2 	nvarchar
            dt.Columns.Add("Define24");//	表体自定义项3 	nvarchar
            dt.Columns.Add("Define25");//	表体自定义项4 	nvarchar
            dt.Columns.Add("Define26", typeof(float));//	表体自定义项5 	float
            dt.Columns.Add("Define27", typeof(float));//	表体自定义项6 	float
            dt.Columns.Add("Define28");//	表体自定义项7 	nvarchar
            dt.Columns.Add("Define29");//	表体自定义项8 	nvarchar
            dt.Columns.Add("Define30");//	表体自定义项9 	nvarchar
            dt.Columns.Add("Define31");//	表体自定义项10 	nvarchar
            dt.Columns.Add("Define32");//表体自定义项11 	nvarchar
            dt.Columns.Add("Define33");//	表体自定义项12 	nvarchar
            dt.Columns.Add("Define34", typeof(int));//	表体自定义项13 	int
            dt.Columns.Add("Define35", typeof(int));//	表体自定义项14 	int
            dt.Columns.Add("Define36", typeof(DateTime));//	表体自定义项15 	datetime
            _dateNames.Add("Define36");
            dt.Columns.Add("Define37", typeof(DateTime));//	表体自定义项16 	datetime
            _dateNames.Add("Define37");
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="bomItem"></param>
        /// <returns></returns>
        private DataTable GetComponentOptDt(DEBusinessItem bomItem, DERelation2 relation, int seq) {
            if (bomItem == null) {
                return null;
            }
            var dt = BuildComponentOptDt();
            var row = dt.NewRow();
            foreach (DataColumn col in dt.Columns) {
                var val = bomItem.GetAttrValue(bomItem.ClassName, col.ColumnName.ToUpper());
                var rltVal = relation.GetAttrValue(col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = rltVal == null ? DBNull.Value : rltVal;
                        break;
                    case "OptionsId":
                        row[col] = seq * 10;
                        break;
                    case "Offset":
                        row[col] = rltVal == null ? 0 : rltVal;
                        break;
                    case "WIPType":
                        row[col] = rltVal == null ? 4 : rltVal;
                        break;
                    case "AccuCostFlag":
                        row[col] = rltVal == null ? true : rltVal;
                        break;
                    case "OptionalFlag":
                        row[col] = rltVal == null ? false : rltVal;
                        break;
                    case "MutexRule":
                        row[col] = rltVal == null ? 2 : rltVal;
                        break;
                    case "PlanFactor":
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
        private DataTable BuildComponentOptDt() {
            DataTable dt = new DataTable("ComponentOpt");
            dt.Columns.Add("OptionsId", typeof(int));//	子件选项档ID 自增量 	int
            dt.Columns.Add("Offset", typeof(int));// 	偏置期  	smallint
            dt.Columns.Add("WIPType", typeof(int));// 	WIP属性	tinyint
            dt.Columns.Add("AccuCostFlag", typeof(bool));// 	是/否累计成本  	bit
            dt.Columns.Add("DrawDeptCode");// 	领料部门  	nvarchar
            dt.Columns.Add("Whcode");// 	仓库代号 	nvarchar
            dt.Columns.Add("OptionalFlag", typeof(bool));// 	是否可选 	bit
            dt.Columns.Add("MutexRule", typeof(int));// 	互斥原则	tinyint
            dt.Columns.Add("PlanFactor", typeof(decimal));// 	计划比例  	Udt_Rate100
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetComponentSubDt(DEBusinessItem dEBusinessItem, DERelation2 relation) {
            if (dEBusinessItem == null) {
                return null;
            }
            var dt = BuildVersionDt();
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
        private DataTable BuildComponentSubDt() {
            DataTable dt = new DataTable("ComponentSub");
            dt.Columns.Add("OptionsId", typeof(int));//	子件选项档ID 自增量 	int
            dt.Columns.Add("ComponentSubId", typeof(int));// 	子件替代ID 主键ID 自增量 	int
            dt.Columns.Add("OpComponentId", typeof(int));// 	子件Id  	int
            dt.Columns.Add("Sequence", typeof(int));// 	替代序号  	int
            dt.Columns.Add("InvCode");// 	存货编码  	nvarchar
            dt.Columns.Add("Define22");//	自定义项22	nvarchar
            dt.Columns.Add("Define23	");//自定义项23	nvarchar
            dt.Columns.Add("Define24");//	自定义项24	nvarchar
            dt.Columns.Add("Define25");//	自定义项25	nvarchar
            dt.Columns.Add("Define26	", typeof(float));//自定义项26	float
            dt.Columns.Add("Define27	", typeof(float));//自定义项27	float
            dt.Columns.Add("Define28");//	自定义项28	nvarchar
            dt.Columns.Add("Define29	");//自定义项29	nvarchar
            dt.Columns.Add("Define30	");//自定义项30	nvarchar
            dt.Columns.Add("Define31	");//自定义项31	nvarchar
            dt.Columns.Add("Define32	");//自定义项32	nvarchar
            dt.Columns.Add("Define33");//	自定义项33	nvarchar
            dt.Columns.Add("Define34", typeof(int));//	自定义项34	int
            dt.Columns.Add("Define35", typeof(int));//	自定义项35	int
            dt.Columns.Add("Define36", typeof(DateTime));//	自定义项36	datetime
            _dateNames.Add("Define36");
            dt.Columns.Add("Define37	", typeof(DateTime));//自定义项37	datetime
            _dateNames.Add("Define37");
            dt.Columns.Add("Factor", typeof(decimal));// 	替代比例 	Udt_Rate100
            dt.Columns.Add("EffBegDate", typeof(DateTime));// 	生效日期  	datetime
            _dateNames.Add("EffBegDate");
            dt.Columns.Add("EffEndDate", typeof(DateTime));// 	失效日期	datetime
            _dateNames.Add("EffEndDate");
            return dt;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetComponentLocDt(DEBusinessItem dEBusinessItem, DERelation2 relation) {
            if (dEBusinessItem == null) {
                return null;
            }
            var dt = BuildVersionDt();
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
        private DataTable BuildComponentLocDt() {
            DataTable dt = new DataTable("ComponentLoc");
            dt.Columns.Add("LocationId", typeof(int));//	子件定位符资料ID 主键ID 自增量	int
            dt.Columns.Add("OpComponentId", typeof(int));// 子件Id 	int
            dt.Columns.Add("SortSeq", typeof(int));// 	序号  	int
            dt.Columns.Add("Loc");// 	定位符  	nvarchar
            return dt;
        }
    }
}
