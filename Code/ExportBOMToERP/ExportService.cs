using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using Thyt.TiPLM.Common;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Admin.DataModel;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Controls;

namespace ExportBOMToERP {
    public class ExportService {
        public ExportService(DEBusinessItem item) {
            if (item==null) {
                throw new ArgumentNullException("item");
            } 
            _bItem = item;
            //20181102 added by kexp 添加账套属性
            GetSenderSeq(item);
        }

        /// <summary>
        /// 获取sender序号设置
        /// </summary>
        /// <param name="item"></param>
        private void GetSenderSeq(DEBusinessItem item) {
            var senderSeq = item.GetAttrValue(item.ClassName, EAI_SENDER_SEQ) == null ? "0" : item.GetAttrValue(item.ClassName, EAI_SENDER_SEQ).ToString();
            var seqArr = senderSeq.Split(',');
            if (seqArr == null || seqArr.Length == 0) {
                return;
            }
            _senderSeq = new int[seqArr.Length];
            for (int i = 0; i < seqArr.Length; i++) {
                var seq = Convert.ToInt16(seqArr[i].Trim());
                _senderSeq[i] = seq;
            }
        }
        private DEBusinessItem _bItem;
        private string _eaiAddress;

        public string[] Senders {
            get {
                if (_senders==null) {
                    GetEAIConfig();
                }
                return _senders; }
        }
        private string[] _senders;
        private int[] _senderSeq;
        //20180821 added by kexp 添加对已有且不需要导入的项定版时判断
        private static string SYS_ERROR = "sys_error";
        private static string EAI_SENDER_SEQ = "EAISENDERSEQ";
        protected string EaiAddress {
            get {
                if (string.IsNullOrEmpty(_eaiAddress)) {
                    _eaiAddress = GetEAIConfig();
                }
                return _eaiAddress;
            }
        }

        /// <summary>
        /// 获取可用ERP导出程序
        /// </summary>
        /// <param name="userOid"></param>
        /// <returns></returns>
        public string GetEAIConfig() {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string[] cfgs = Directory.GetFiles(dir, "*config.xml");
            if (cfgs == null || cfgs.Length == 0) {
                MessageBoxPLM.Show("ERP导入配置没有配置在客户端！");
                return null;
            }
            return CheckEAIConfig(cfgs[0]);
        }

        /// <summary>
        /// 检查配置文件合法性
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string CheckEAIConfig(string fileName) {
            if (fileName == null) {
                MessageBoxPLM.Show("ERP导入配置文件没有内容！");
                return null;
            }
            if (!fileName.EndsWith(".xml")) {
                MessageBoxPLM.Show("ERP导入配置文件格式不正确，只支持xml格式的配置！");
                return null;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            var eaiAddNode = doc.SelectSingleNode("ERPIntegratorConfig//config//EAIAddress");
            if (eaiAddNode == null) {
                MessageBoxPLM.Show("ERP导入配置文件没有EAI地址，请补充！");
                return null;
            }
            var add = eaiAddNode.InnerText;
            if (string.IsNullOrEmpty(add)) {
                MessageBoxPLM.Show("ERP导入配置文件没有EAI地址，请补充！");
                return null;
            }
            // added by kexp获取sender
            //20181102 added by kexp 添加多账套配置
            var accNode = doc.SelectSingleNode("ERPIntegratorConfig//config//AccountSets");
            if (accNode!=null||!string.IsNullOrEmpty(accNode.InnerText)) {
                var senderNodes = accNode.SelectNodes("sender");
                if (senderNodes == null || senderNodes.Count == 0) {
                    _senders = new string[] { "001" };
                } else { 
                    _senders = new string[senderNodes.Count];
                    for (int i = 0; i < senderNodes.Count; i++) {
                        if (string.IsNullOrEmpty(senderNodes[i].InnerText)) {
                            throw new ArgumentNullException("账套配置节点sender不能为空！");
                        }
                        _senders[i] = senderNodes[i].InnerText;
                    }
                }
            }
            return add;
        }
        #region 导出导入
        public void AddOrEditItem() {
            if (_bItem == null) {
                MessageBoxPLM.Show("没有获取到当前对象，请检查！");
                return;
            }
            //string oprt = item.LastRevision > 1 ? "Edit" : "Add";
            bool succeed;
            string errText;

            succeed = DoExport(out errText, _bItem);
            //20180821 added by kexp 添加对已有且不需要导入的项定版时判断
            if (!succeed&&!string.IsNullOrEmpty(errText)&&!errText.Equals(SYS_ERROR)) { 
                MessageBoxPLM.Show(errText);
                return;
            }
            MessageBoxPLM.Show("ERP导入成功!");
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="oprt"></param>
        /// <param name="errText"></param>
        /// <param name="bItem"></param>
        private bool DoExport(out string errText, DEBusinessItem bItem) {
            var exportResult = DoExportImplement(bItem, bItem.ClassName, out errText);
            if (!exportResult) {
                return false;
            }

            //20180927把bom挂在工艺路线下改为bom挂在零件下
            var currentName = _bItem.ClassName;
            var parent = ModelContext.MetaModel.GetParent(currentName);
            if (parent.Name.ToLower() == "part"&&currentName.ToLower()!="GZ") {//工装不需要导入
                return DoExportImplement(bItem, "Bom", out errText);
            }

            return true;
        }

        /// <summary>
        /// 导入到ERP
        /// </summary>
        /// <param name="xml"></param>
        private bool DoExportImplement(DEBusinessItem bItem, string typeStr, out string errText) {
            if (bItem == null) {
                errText = "";
                return false;
            }
            XmlDocument doc = new XmlDocument();
            errText = "";
            var dal = DalFactory.Instance.CreateDal(bItem, typeStr);
            bool succeed = true;
            string hasStr = "重复";
            string oprt = "Add";
            doc = dal.CreateXmlDocument(oprt);
            if (doc == null) {
                return false;
            }
            //20181102 added by kexp 添加账套属性配置
            if (_senderSeq == null || _senderSeq.Length == 0) {
                succeed = DoConnect(doc.OuterXml, ref errText);
                if (!succeed && errText.Contains(hasStr)) {
                    oprt = "Edit";
                    doc = dal.CreateXmlDocument(oprt);
                    succeed = DoConnect(doc.OuterXml, ref errText);
                }
            } else {
                for (int i = 0; i < _senderSeq.Length; i++) {
                    var xml = ModifyXmlHeaderByConfig(doc.OuterXml, _senderSeq[i]);
                    succeed = DoConnect(xml, ref errText);
                    if (!succeed && errText.Contains(hasStr)) {
                        oprt = "Edit";
                        doc = dal.CreateXmlDocument(oprt);
                        xml = ModifyXmlHeaderByConfig(doc.OuterXml, _senderSeq[i]);
                        succeed = DoConnect(xml, ref errText);
                    }
                    if (!succeed) {
                        return succeed;
                    }
                }
            }
            return succeed;
        }

        private bool DoConnect(string xml, ref string errText) {
            MSXML2.XMLHTTPClass xmlHttp = new MSXML2.XMLHTTPClass();
            xmlHttp.open("POST", EaiAddress, false, null, null);//TODO：地址需要改
            xmlHttp.send(xml);
            String responseXml = xmlHttp.responseText;
            //…… //处理返回结果 
            XmlDocument resultDoc = new XmlDocument();
            resultDoc.LoadXml(responseXml);
            var itemNode = resultDoc.SelectSingleNode("ufinterface//item");
            var s = ConstCommon.CURRENT_PRODUCTNAME;

            if (itemNode == null) {
                errText = "没有收到ERP回执";

                PLMEventLog.WriteLog("没有收到ERP回执！", EventLogEntryType.Error);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(xmlHttp); //COM释放
                return false;
            }
            var succeed = Convert.ToInt32(itemNode.Attributes["succeed"].Value);//成功标识：0：成功；非0：失败；
            var dsc = itemNode.Attributes["dsc"].Value.ToString();
            //var u8key =itemNode.Attributes["u8key"].ToString();
            //var proc = itemNode.Attributes["proc"].ToString();
            if (succeed != 0) {
                errText = string.Format("ERP导入失败，原因：{0}", dsc);

                PLMEventLog.WriteLog(dsc, EventLogEntryType.Error);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(xmlHttp); //COM释放
                return false;

            }
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(xmlHttp); //COM释放
            return true;
        }

        //20181102 modified by kexp 添加账套属性配置
        private string ModifyXmlHeaderByConfig(string xml,int senderSeq) {
            if (string.IsNullOrEmpty(xml)) {
                return xml;
            }
            string senderNum = "001";
            if (Senders != null && Senders.Length > 0 && senderSeq < Senders.Length) {
                senderNum = Senders[senderSeq];
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var sender = doc.SelectSingleNode("ufinterface");
            if (sender == null || string.IsNullOrEmpty(sender.Attributes["sender"].ToString())) {
                return xml;
            }
            sender.Attributes["sender"].Value = senderNum;
            return doc.OuterXml;

        }
        #endregion
    }
}
