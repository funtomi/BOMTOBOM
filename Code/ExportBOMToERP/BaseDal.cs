using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Product2;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Controls;

namespace ExportBOMToERP {
    public abstract class BaseDal {
        private const string FILE_PATH="..\\ExportFiles";
        protected DEBusinessItem _dEBusinessItem;
        protected string _name;
        protected string _filePath;
        protected List<string> _dateNames = new List<string>();

        /// <summary>
        /// 构建保存路径
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string BuildFilePath(DEBusinessItem item, string name) {
            string fPath = Path.GetFullPath(FILE_PATH);
            string newFile = Path.Combine(fPath, item.Id + "_" + name + ".xml");
            FileInfo info = new FileInfo(newFile);
            var di =info.Directory;
            if (!di.Exists) {
                di.Create();
            }
            return newFile;
        }
        private string GetFamilyStr(string tableName) {
            if (string.IsNullOrEmpty(tableName)) {
                return "";
            }
            switch (tableName) {
                default:
                case "Bom":
                case "WorkCenters":
                case "Operation":
                case "Routing":
                    return "生产制造";
                case "inventory":
                case "unitgroup":
                case "unit":
                case "inventoryclass":
                    return "基础档案";
            }
        }

        /// <summary>
        /// 创建xml框架(ufinterface节点和父节点)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        protected XmlDocument CreateXmlSchema(string tableName, DEBusinessItem dEBusinessItem, string operatorStr) {
            if (string.IsNullOrEmpty(tableName) || dEBusinessItem == null) {
                return null;
            }
            XmlDocument doc = new XmlDocument();
            var dec = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            doc.AppendChild(dec);
            //var node = doc.SelectSingleNode(tableName);
            var root = doc.CreateElement("ufinterface");

            #region 默认，无需修改
            root.SetAttribute("receiver", "U8");//接收方 
            //档案或单据模版名，填档案或单据的唯一标识，如:客商档案：customer，客商分类：customerclass ，具体名称由总体确定，在数据交换中该名称要经常使用；
            root.SetAttribute("roottag", tableName);
            //proc：操作类型，分为“增删改查”，对应填Add / Delete /Edit /Query（必填），该字段导入操作，请填写Add / Delete /Edit，导出操作，请填写Query；
            root.SetAttribute("proc", operatorStr);
            //编码是否已转换，该字段在导入的时候使用，如果已转换即已和U8基础数据编码一致填Y，将不会通过对照表的转换，如果没有转换即和U8基础数据编码不一致填N，将会自动通过对照表转换之后，进行相应的操作
            root.SetAttribute("codeexchanged", "N");
            root.SetAttribute("paginate", "0");
            root.SetAttribute("family", GetFamilyStr(tableName));
            #endregion
            //发送方，填外部系统注册码（必填）
            root.SetAttribute("sender", "001");
            var tableNode = doc.CreateElement(tableName);

            root.AppendChild(tableNode);
            doc.AppendChild(root);
            return doc;
        }

        /// <summary>
        /// 创建xml文档
        /// </summary>
        /// <returns></returns>
        public XmlDocument CreateXmlDocument(string operatorStr) {
            var doc = BuildXmlDocment(operatorStr);
            doc = DateTimeFormat(doc, _dateNames);
            if (doc == null) {
                return null;
            }
            doc.Save(_filePath);
            return doc;
        }
        /// <summary>
        /// 格式化日期类型
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private XmlDocument DateTimeFormat(XmlDocument doc, List<string> eles) {
            if (eles == null || eles.Count == 0 || doc == null) {
                return doc;
            }
            XmlDocument newDoc = new XmlDocument();
            try {
                // 建立一个 XmlTextReader 对象来读取 XML 数据。
                using (XmlTextReader myXmlReader = new XmlTextReader(doc.OuterXml, XmlNodeType.Element, null)) {
                    // 使用指定的文件与编码方式来建立一个 XmlTextWriter 对象。
                    using (System.Xml.XmlTextWriter myXmlWriter = new System.Xml.XmlTextWriter(_filePath, Encoding.UTF8)) {
                        myXmlWriter.Formatting = Formatting.Indented;
                        myXmlWriter.Indentation = 4;
                        myXmlWriter.WriteStartDocument();

                        string elementName = "";

                        // 解析并显示每一个节点。
                        while (myXmlReader.Read()) {
                            switch (myXmlReader.NodeType) {
                                case XmlNodeType.Element:
                                    myXmlWriter.WriteStartElement(myXmlReader.Name);
                                    myXmlWriter.WriteAttributes(myXmlReader, true);
                                    elementName = myXmlReader.Name;

                                    break;
                                case XmlNodeType.Text:
                                    if (eles.Contains(elementName)) {
                                        myXmlWriter.WriteString(XmlConvert.ToDateTime(myXmlReader.Value, XmlDateTimeSerializationMode.Local).ToString("yyyy-MM-dd HH:mm:ss"));
                                        break;
                                    }
                                    myXmlWriter.WriteString(myXmlReader.Value);
                                    break;
                                case XmlNodeType.EndElement:
                                    myXmlWriter.WriteEndElement();
                                    break;
                            }
                        }
                    }
                }
                newDoc.Load(_filePath);
                return newDoc;
            } catch (Exception ex) {
                MessageBoxPLM.Show(string.Format("导入ERP文件格式化失败，错误信息：{0}",ex.Message));
                return null;
            }
        }

        protected abstract XmlDocument BuildXmlDocment(string operatorStr);

        /// <summary>
        /// 获取关联
        /// </summary>
        /// <param name="item"></param>
        /// <param name="relation"></param>
        /// <returns></returns>
        protected DERelationBizItemList GetLinks(DEBusinessItem item, string relation) {
            DERelationBizItemList relationBizItemList;
            try {
                relationBizItemList = item.Iteration.LinkRelationSet.GetRelationBizItemList(relation);
                if (relationBizItemList == null) {
                    relationBizItemList = PLItem.Agent.GetLinkRelationItems(item.Iteration.Oid, item.Master.ClassName, relation, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
                }
            } catch {
                return null;
            }

            return relationBizItemList;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="componentDt"></param>
        /// <returns></returns>
        protected XmlDocument AddComponentNode(XmlDocument doc, DataTable componentDt) {
            if (componentDt == null || componentDt.Rows.Count == 0) {
                return doc;
            }
            componentDt.WriteXml(_filePath);
            XmlDocument docTemp = new XmlDocument();
            docTemp.Load(_filePath);
            XmlNode node = doc.ImportNode(docTemp.DocumentElement, true);
            string path = string.Format("ufinterface//{0}", _name);
            doc.SelectSingleNode(path).AppendChild(node.FirstChild);
            return doc;
        }

        
    }
}
