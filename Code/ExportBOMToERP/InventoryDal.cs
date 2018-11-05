using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using Thyt.TiPLM.Common;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Product2;
using Thyt.TiPLM.UIL.Controls;

namespace ExportBOMToERP {
    public class InventoryDal : BaseDal {
        public InventoryDal(DEBusinessItem dItem) {
            _dEBusinessItem = dItem;
            this._name = "inventory";
            _filePath = BuildFilePath(dItem, _name);
        }
        /// <summary>
        /// 父节点名
        /// </summary>
        public String Name {
            get { return _name; }
        }

        /// <summary>
        /// 获取header节点数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetHeaderTable(DEBusinessItem dEBusinessItem) {
            if (dEBusinessItem == null) {
                return null;
            }
            var dt = BuildHeaderDt();
            DataRow row = dt.NewRow();

            #region 普通节点，默认ERP列名和PLM列名一致
            foreach (DataColumn col in dt.Columns) {
                var val = dEBusinessItem.GetAttrValue(dEBusinessItem.ClassName, col.ColumnName.ToUpper());
                switch (col.ColumnName) {
                    default:
                        row[col] = val == null ? DBNull.Value : val;
                        break; 
                    case "CreatePerson":
                        row[col] = PrintUtil.GetUserName(dEBusinessItem.Creator);
                        break;
                    case "ModifyPerson":
                        row[col] = PrintUtil.GetUserName(dEBusinessItem.LatestUpdator);
                        break;
                    case "ModifyDate":
                        row[col] = dEBusinessItem.LatestUpdateTime;
                        break;
                    case "unitgroup_code":
                        row[col] = val == null ? "01" : val;
                        break;
                    case "cPlanMethod":
                        row[col] = val == null ? "L" : val;
                        break;
                    case "cSRPolicy":
                        row[col] = val == null ? "PE" : val;
                        break;
                    case "iSupplyType":
                        row[col] = val == null ? 0 : val;
                        break;
                }
            }
            #endregion

            dt.Rows.Add(row);
            return dt;
        }

        /// <summary>
        /// 创建Header节点结构
        /// </summary>
        /// <returns></returns>
        private DataTable BuildHeaderDt() {
            DataTable dt = new DataTable("header");
            dt.Columns.Add("code");//存货编码
            dt.Columns.Add("name");//	存货名称
            dt.Columns.Add("InvAddCode");//	存货代码
            dt.Columns.Add("specs");//	规格型号
            dt.Columns.Add("sort_code");//	所属分类码
            dt.Columns.Add("main_supplier");//	主要供货单位
            dt.Columns.Add("main_measure");//	主计量单位编码
            dt.Columns.Add("switch_item");//	替换件
            dt.Columns.Add("inv_position");//货位编码
            dt.Columns.Add("sale_flag", typeof(int));//	是否内销
            dt.Columns.Add("purchase_flag", typeof(int));//	是否采购
            dt.Columns.Add("selfmake_flag", typeof(int));//	是否自制
            dt.Columns.Add("prod_consu_flag", typeof(int));//	是否生产耗用
            dt.Columns.Add("in_making_flag", typeof(int));//	是否在制
            dt.Columns.Add("tax_serv_flag", typeof(int));//	是否应税劳务
            dt.Columns.Add("suit_flag", typeof(int));//	是否成套件
            dt.Columns.Add("tax_rate", typeof(decimal));//销项税率
            dt.Columns.Add("unit_weight", typeof(decimal));//	单位重量
            dt.Columns.Add("unit_volume", typeof(decimal));//	单位体积
            dt.Columns.Add("pro_sale_price", typeof(decimal));//	计划价/售价
            dt.Columns.Add("ref_cost", typeof(decimal));//参考成本
            dt.Columns.Add("ref_sale_price", typeof(decimal));//	参考售价
            dt.Columns.Add("bottom_sale_price", typeof(decimal));//	最低售价
            dt.Columns.Add("new_cost", typeof(decimal));//最新成本
            dt.Columns.Add("advance_period", typeof(decimal));//	提前期
            dt.Columns.Add("ecnomic_batch", typeof(decimal));//	固定供应量(经济批量 )
            dt.Columns.Add("safe_stock", typeof(decimal));//	安全库存
            dt.Columns.Add("top_stock", typeof(decimal));//	最高库存
            dt.Columns.Add("bottom_stock", typeof(decimal));//最低库存
            dt.Columns.Add("backlog", typeof(decimal));//	积压标准
            dt.Columns.Add("ABC_type");//ABC分类
            dt.Columns.Add("qlty_guarantee_flag", typeof(int));//	是否保质期管理
            dt.Columns.Add("batch_flag", typeof(int));//	是否批次管理
            dt.Columns.Add("entrust_flag", typeof(int));//是否受托代销
            dt.Columns.Add("backlog_flag", typeof(int));//是否呆滞积压
            dt.Columns.Add("start_date", typeof(DateTime));//	启用日期
            _dateNames.Add("start_date");
            dt.Columns.Add("end_date", typeof(DateTime));//停用日期
            _dateNames.Add("end_date");
            dt.Columns.Add("free_item1", typeof(int));//	自由项1(存货是否有自由项1)
            dt.Columns.Add("free_item2", typeof(int));//	自由项2(存货是否有自由项2)
            dt.Columns.Add("self_define1");//自定义项1
            dt.Columns.Add("self_define2");//自定义项2
            dt.Columns.Add("self_define3");//自定义项3
            dt.Columns.Add("discount_flag", typeof(int));//	是否折扣
            dt.Columns.Add("top_source_price", typeof(decimal));//最高进价
            dt.Columns.Add("quality");//	质量要求说明
            dt.Columns.Add("retailprice", typeof(decimal));//	零售单价
            dt.Columns.Add("price1", typeof(decimal));//	一级批发价
            dt.Columns.Add("price2", typeof(decimal));//	二级批发价
            dt.Columns.Add("price3", typeof(decimal));//	三级批发价
            dt.Columns.Add("CreatePerson");//	建档人
            dt.Columns.Add("ModifyPerson");//变更人
            dt.Columns.Add("ModifyDate", typeof(DateTime));//	变更日期
            _dateNames.Add("ModifyDate");
            dt.Columns.Add("subscribe_point", typeof(decimal));//	订货点
            dt.Columns.Add("avgquantity", typeof(decimal));//	平均耗用量
            dt.Columns.Add("pricetype");//	计价方式
            dt.Columns.Add("bfixunit", typeof(int));//是否为固定换算率
            dt.Columns.Add("outline", typeof(decimal));//	出库超额上限
            dt.Columns.Add("inline", typeof(decimal));//	入库超额上限
            dt.Columns.Add("overdate", typeof(int));//保质期
            dt.Columns.Add("warndays", typeof(int));//保质期预警天数
            dt.Columns.Add("expense_rate", typeof(decimal));//费用率
            dt.Columns.Add("btrack", typeof(int));//	是否出库跟踪入库
            dt.Columns.Add("bserial", typeof(int));//	是否有序列号管理
            dt.Columns.Add("bbarcode", typeof(int));//是否条形码管理
            dt.Columns.Add("barcode");//	对应条形码中的编码
            dt.Columns.Add("auth_class", typeof(int));//	所属权限分类
            dt.Columns.Add("self_define4");//自定义项4
            dt.Columns.Add("self_define5");//自定义项5
            dt.Columns.Add("self_define6");//自定义项6
            dt.Columns.Add("self_define7");//自定义项7
            dt.Columns.Add("self_define8");//自定义项8
            dt.Columns.Add("self_define9");//自定义项9
            dt.Columns.Add("self_define10");//	自定义项10
            dt.Columns.Add("self_define11", typeof(int));//	自定义项11
            dt.Columns.Add("self_define12", typeof(int));//	自定义项12
            dt.Columns.Add("self_define13", typeof(decimal));//	自定义项13
            dt.Columns.Add("self_define14", typeof(decimal));//	自定义项14
            dt.Columns.Add("self_define15", typeof(DateTime));//	自定义项15
            _dateNames.Add("self_define15");
            dt.Columns.Add("self_define16", typeof(DateTime));//	自定义项16
            _dateNames.Add("self_define16");
            dt.Columns.Add("free_item3", typeof(int));//	自由项3
            dt.Columns.Add("free_item4", typeof(int));//	自由项4
            dt.Columns.Add("free_item5", typeof(int));//	自由项5
            dt.Columns.Add("free_item6", typeof(int));//	自由项6
            dt.Columns.Add("free_item7", typeof(int));//	自由项7
            dt.Columns.Add("free_item8", typeof(int));//	自由项8
            dt.Columns.Add("free_item9", typeof(int));//	自由项9
            dt.Columns.Add("free_item10", typeof(int));//	自由项10
            dt.Columns.Add("unitgroup_type", typeof(int));//	计量单位组类别
            dt.Columns.Add("unitgroup_code");//	计量单位组编码
            dt.Columns.Add("puunit_code");//	采购默认计量单位编码
            dt.Columns.Add("saunit_code");//	销售默认计量单位编码
            dt.Columns.Add("stunit_code");//	库存默认计量单位编码
            dt.Columns.Add("caunit_code");//	成本默认计量单位编码
            dt.Columns.Add("unitgroup_name");//	计量单位组名称
            dt.Columns.Add("puunit_name");//	采购默认计量单位名称
            dt.Columns.Add("saunit_name");//	销售默认计量单位名称
            dt.Columns.Add("stunit_name");//	库存默认计量单位名称
            dt.Columns.Add("caunit_name");//	成本默认计量单位名称
            dt.Columns.Add("puunit_ichangrate");//	采购默认计量单位换算率
            dt.Columns.Add("saunit_ichangrate");//	销售默认计量单位换算率
            dt.Columns.Add("stunit_ichangrate");//	库存默认计量单位换算率
            dt.Columns.Add("caunit_ichangrate");//	成本默认计量单位换算率
            dt.Columns.Add("check_frequency");//	盘点周期单位
            dt.Columns.Add("frequency", typeof(int));//	盘点周期
            dt.Columns.Add("check_day", typeof(int));//	盘点日
            dt.Columns.Add("lastcheck_date", typeof(DateTime));//	上次盘点日期
            _dateNames.Add("lastcheck_date");
            dt.Columns.Add("wastage", typeof(decimal));//	合理损耗率
            dt.Columns.Add("solitude", typeof(int));//	是否单独存放
            dt.Columns.Add("enterprise");//	生产企业
            dt.Columns.Add("address");//	产地
            dt.Columns.Add("file");//批准文号
            dt.Columns.Add("brand");//	注册商标
            dt.Columns.Add("checkout_no");//	合格证号
            dt.Columns.Add("licence");//	许可证号
            dt.Columns.Add("specialties", typeof(int));//	特殊存货标志
            dt.Columns.Add("defwarehouse");//	默认仓库
            dt.Columns.Add("salerate", typeof(decimal));//	销售加成率
            dt.Columns.Add("advanceDate", typeof(int));//	累计提前期
            dt.Columns.Add("currencyName");//	通用名称
            dt.Columns.Add("ProduceAddress");//	生产地点
            dt.Columns.Add("produceNation");//	生产国别
            dt.Columns.Add("RegisterNo");//	进口药品注册证号
            dt.Columns.Add("EnterNo");//	入关证号
            dt.Columns.Add("PackingType");//	包装规格
            dt.Columns.Add("EnglishName");//	英文名
            dt.Columns.Add("PropertyCheck", typeof(int));//	是否质检
            dt.Columns.Add("PreparationType");//	剂型
            dt.Columns.Add("Commodity");//	注册商品批件
            dt.Columns.Add("RecipeBatch", typeof(int));//	处方药（处方药或非处方药 ）
            dt.Columns.Add("NotPatentName");//	国际非专利名
            dt.Columns.Add("cAssComunitCode");//	辅计量单位编码
            dt.Columns.Add("ROPMethod", typeof(int));//	再订货点确定方法
            dt.Columns.Add("SubscribePoint", typeof(decimal));//	再订货点
            dt.Columns.Add("BatchRule", typeof(int));//	批量规则
            dt.Columns.Add("AssureProvideDays", typeof(int));//	保证供应天数
            dt.Columns.Add("VagQuantity", typeof(decimal));//	日均耗量
            dt.Columns.Add("TestStyle", typeof(int));//	检验方式
            dt.Columns.Add("DTMethod", typeof(int));//	抽检方案
            dt.Columns.Add("DTRate", typeof(decimal));//	抽检率
            dt.Columns.Add("DTNum", typeof(decimal));//	抽检量
            dt.Columns.Add("DTUnit");//	检验计量单位
            dt.Columns.Add("DTStyle", typeof(int));//	抽检方式
            dt.Columns.Add("QTMethod", typeof(int));//质量检验方案
            dt.Columns.Add("bPlanInv", typeof(int));//是否计划品
            dt.Columns.Add("bProxyForeign", typeof(int));//	是否委外
            dt.Columns.Add("bATOModel", typeof(int));//	是否ATO模型
            dt.Columns.Add("bCheckItem", typeof(int));//	是否选项类
            dt.Columns.Add("bPTOModel", typeof(int));//	是否PTO模型
            dt.Columns.Add("bequipment", typeof(int));//	是否备件
            dt.Columns.Add("cProductUnit");//生产计量单位
            dt.Columns.Add("fOrderUpLimit", typeof(decimal));//	订货超额上限
            dt.Columns.Add("cMassUnit", typeof(int));//	保质期单位
            dt.Columns.Add("fRetailPrice", typeof(decimal));//零售价格
            dt.Columns.Add("cInvDepCode");//	生产部门
            dt.Columns.Add("iAlterAdvance", typeof(int));//	变动提前期
            dt.Columns.Add("fAlterBaseNum", typeof(decimal));//	变动基数
            dt.Columns.Add("cPlanMethod");//	计划方法
            dt.Columns.Add("bMPS", typeof(int));//是否MPS件
            dt.Columns.Add("bROP", typeof(int));//是否ROP件
            dt.Columns.Add("bRePlan", typeof(int));//是否重复计划
            dt.Columns.Add("cSRPolicy");//	供需政策
            dt.Columns.Add("bBillUnite", typeof(int));//	是否令单合并
            dt.Columns.Add("iSupplyDay", typeof(int));//	供应期间
            dt.Columns.Add("fSupplyMulti", typeof(decimal));//供应倍数
            dt.Columns.Add("fMinSupply", typeof(decimal));//	最低供应量
            dt.Columns.Add("bCutMantissa", typeof(int));//	是否切除尾数
            dt.Columns.Add("cInvPersonCode");//	计划员
            dt.Columns.Add("iInvTfId", typeof(int));//时栅代号
            dt.Columns.Add("cEngineerFigNo");//	工程图号
            dt.Columns.Add("bInTotalCost", typeof(int));//	成本累计否
            dt.Columns.Add("iSupplyType", typeof(int));//	供应类型
            dt.Columns.Add("bConfigFree1", typeof(int));//	结构性自由项1
            dt.Columns.Add("bConfigFree2", typeof(int));//	结构性自由项2
            dt.Columns.Add("bConfigFree3", typeof(int));//	结构性自由项3
            dt.Columns.Add("bConfigFree4", typeof(int));//	结构性自由项4
            dt.Columns.Add("bConfigFree5", typeof(int));//	结构性自由项5
            dt.Columns.Add("bConfigFree6", typeof(int));//	结构性自由项6
            dt.Columns.Add("bConfigFree7", typeof(int));//	结构性自由项7
            dt.Columns.Add("bConfigFree8", typeof(int));//	结构性自由项8
            dt.Columns.Add("bConfigFree9", typeof(int));//	结构性自由项9
            dt.Columns.Add("bConfigFree10", typeof(int));//	结构性自由项10
            dt.Columns.Add("iDTLevel", typeof(int));//	检验水平
            dt.Columns.Add("cDTAQL");//	AQL
            dt.Columns.Add("bOutInvDT", typeof(int));//	是否发货检验
            dt.Columns.Add("bPeriodDT", typeof(int));//	是否周期检验
            dt.Columns.Add("cDTPeriod");//	检验周期
            dt.Columns.Add("bBackInvDT", typeof(int));//	是否退货检验
            dt.Columns.Add("iEndDTStyle", typeof(int));//	产品抽检方式（自制品检验严格度）
            dt.Columns.Add("bDTWarnInv", typeof(int));//	保质期存货是否检验
            dt.Columns.Add("fBackTaxRate", typeof(decimal));//	退税率
            dt.Columns.Add("cCIQCode");//	海关代码
            dt.Columns.Add("cWGroupCode");//	重量计量组
            dt.Columns.Add("cWUnit");//	重量单位
            dt.Columns.Add("fGrossW", typeof(decimal));//毛重
            dt.Columns.Add("cVGroupCode");//	体积计量组
            dt.Columns.Add("cVUnit");//	体积单位
            dt.Columns.Add("fLength", typeof(decimal));//	长（CM）
            dt.Columns.Add("fWidth", typeof(decimal));//	宽（CM）
            dt.Columns.Add("fHeight", typeof(decimal));//	高（CM）
            dt.Columns.Add("cpurpersoncode");//	采购员
            dt.Columns.Add("iBigMonth", typeof(int));//	不大于月
            dt.Columns.Add("iBigDay", typeof(int));//	不大于天
            dt.Columns.Add("iSmallMonth", typeof(int));//不小于月
            dt.Columns.Add("iSmallDay", typeof(int));//	不小于天
            dt.Columns.Add("cshopunit");//	零售计量单位
            dt.Columns.Add("bimportmedicine", typeof(int));//	是否进口药品
            dt.Columns.Add("bfirstbusimedicine", typeof(int));//	是否首营药品
            dt.Columns.Add("bforeexpland", typeof(int));//	预测展开
            dt.Columns.Add("cinvplancode");//	计划品
            dt.Columns.Add("fconvertrate", typeof(decimal));//	转换因子
            dt.Columns.Add("dreplacedate", typeof(DateTime));//	替换日期
            _dateNames.Add("dreplacedate");
            dt.Columns.Add("binvmodel", typeof(int));//	模型
            dt.Columns.Add("iimptaxrate", typeof(decimal));//	进项税率
            dt.Columns.Add("iexptaxrate", typeof(decimal));//	出口税率
            dt.Columns.Add("bexpsale", typeof(int));//	外销
            dt.Columns.Add("idrawbatch", typeof(decimal));//	领料批量
            dt.Columns.Add("bcheckbsatp", typeof(int));//检查售前ATP
            dt.Columns.Add("cinvprojectcode");//	售前ATP方案
            dt.Columns.Add("itestrule", typeof(int));//	检验规则
            dt.Columns.Add("crulecode");//	自定义抽检规则
            dt.Columns.Add("bcheckfree1", typeof(int));//	核算自由项1
            dt.Columns.Add("bcheckfree2", typeof(int));//	核算自由项2
            dt.Columns.Add("bcheckfree3", typeof(int));//	核算自由项3
            dt.Columns.Add("bcheckfree4", typeof(int));//	核算自由项4
            dt.Columns.Add("bcheckfree5", typeof(int));//	核算自由项5
            dt.Columns.Add("bcheckfree6", typeof(int));//	核算自由项6
            dt.Columns.Add("bcheckfree7", typeof(int));//核算自由项7
            dt.Columns.Add("bcheckfree8", typeof(int));//	核算自由项8
            dt.Columns.Add("bcheckfree9", typeof(int));//	核算自由项9
            dt.Columns.Add("bcheckfree10", typeof(int));//	核算自由项10
            dt.Columns.Add("bbommain", typeof(int));//	允许BOM母件
            dt.Columns.Add("bbomsub", typeof(int));//	允许BOM子件
            dt.Columns.Add("bproductbill", typeof(int));//	允许生产订单
            dt.Columns.Add("icheckatp", typeof(int));//	检查ATP
            dt.Columns.Add("iinvatpid", typeof(int));//	ATP规则
            dt.Columns.Add("iplantfday", typeof(int));//	计划时栅天数
            dt.Columns.Add("ioverlapday", typeof(int));//	重叠天数
            dt.Columns.Add("fmaxsupply", typeof(decimal));//	最高供应量
            dt.Columns.Add("bpiece", typeof(int));//	计件
            dt.Columns.Add("bsrvitem", typeof(int));//	服务项目
            dt.Columns.Add("bsrvfittings", typeof(int));//	服务配件
            dt.Columns.Add("fminsplit", typeof(decimal));//	最小分割量
            dt.Columns.Add("bspecialorder", typeof(int));//	客户订单专用
            dt.Columns.Add("btracksalebill", typeof(int));//	销售跟单
            return dt;

        }

        private DataTable BuildEntryDt() {
            DataTable dt = new DataTable("entry");
            dt.Columns.Add("partid");//	自增量。去掉<partid />,否则会导致通过xml导入存货结构自由项时，无法追加 后来要支持差异更新，partid不能去掉
            dt.Columns.Add("invcode");//	存货编码
            dt.Columns.Add("free1");//	自由项1
            dt.Columns.Add("free2");//	自由项2
            dt.Columns.Add("free3");//	自由项3
            dt.Columns.Add("free4");//	自由项4
            dt.Columns.Add("free5");//	自由项5
            dt.Columns.Add("free6");//	自由项6
            dt.Columns.Add("free7");//	自由项7
            dt.Columns.Add("free8");//	自由项8
            dt.Columns.Add("free9");//	自由项9
            dt.Columns.Add("free10");//	自由项10
            dt.Columns.Add("safeqty");//	安全库存
            dt.Columns.Add("minqty");//	最低供应量
            dt.Columns.Add("mulqty");//	供应倍数
            dt.Columns.Add("fixqty");//	固定供应量
            dt.Columns.Add("cbasengineerfigno");//	工程图号
            dt.Columns.Add("fbasmaxsupply");//	最高供应量
            dt.Columns.Add("isurenesstype");//	安全库存方法
            dt.Columns.Add("idatetype");//	期间类型
            dt.Columns.Add("idatesum	");//期间数
            dt.Columns.Add("idynamicsurenesstype	");//动态安全库存方法
            dt.Columns.Add("ibestrowsum");//	覆盖天数
            dt.Columns.Add("ipercentumsum");//	百分比
            dt.Columns.Add("bfreestop", typeof(int));//	停用
            return dt;
        }

        protected override XmlDocument BuildXmlDocment(string operatorStr) {
            XmlDocument doc = CreateXmlSchema(_name, _dEBusinessItem, operatorStr);
            var headerDt = GetHeaderTable(_dEBusinessItem);
            headerDt.WriteXml(_filePath);
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            //MessageBoxPLM.Show(docTemp.OuterXml);
            //PLMEventLog.WriteLog(docTemp.OuterXml, System.Diagnostics.EventLogEntryType.Warning);
            var node = doc.ImportNode(docTemp.DocumentElement, true);
            string path = string.Format("ufinterface//{0}", _name);
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);//append head节点

            var entryDt = GetEntryDt(_dEBusinessItem);
            entryDt.WriteXml(_filePath);
            XmlDocument entryDoc = new XmlDocument();
            entryDoc.Load(_filePath);
            var entryNode = doc.ImportNode(entryDoc.DocumentElement, true);

            var bodyNode = doc.CreateElement("body");
            bodyNode.AppendChild(entryNode.FirstChild);
            doc.SelectSingleNode(path).AppendChild(bodyNode);
            //for (int j = 0; j < node.ChildNodes.Count; j++) {
            //    var childNode = node.ChildNodes[j];
            //    doc.SelectSingleNode(path).AppendChild(childNode);
            //    j--;
            //}
            return doc;
        }

        /// <summary>
        /// 获取entry节点数据
        /// </summary>
        /// <param name="dEBusinessItem"></param>
        /// <returns></returns>
        private DataTable GetEntryDt(DEBusinessItem dEBusinessItem) {
            if (dEBusinessItem == null) {
                return null;
            }
            var dt = BuildEntryDt();
            DataRow row = dt.NewRow();

            #region 普通节点，默认ERP列名和PLM列名一致
            foreach (DataColumn col in dt.Columns) {
                
                var val = dEBusinessItem.GetAttrValue(dEBusinessItem.ClassName, col.ColumnName.ToUpper());
                if (col.ColumnName == "invcode") {
                    val = dEBusinessItem.GetAttrValue(dEBusinessItem.ClassName, "CODE");
                }
                row[col] = val == null ? DBNull.Value : val;
            }
            #endregion

            dt.Rows.Add(row);
            return dt;
        }
    }
}
