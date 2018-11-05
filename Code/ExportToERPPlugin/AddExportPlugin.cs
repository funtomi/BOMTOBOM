using ExportBOMToERP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Thyt.TiPLM.Common.Interface.Addin;
using Thyt.TiPLM.DEL.Admin.DataModel;
using Thyt.TiPLM.DEL.Operation;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Operation;
using Thyt.TiPLM.PLL.Product2;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Common.Operation;
using Thyt.TiPLM.UIL.Controls;
using Thyt.TiPLM.UIL.Product.Common;

namespace ExportToERPPluginCLT {
    public class AddExportPlugin : IAddinClientEntry, IAutoExec {
        public Delegate d_AfterReleased { get; set; }
        public Delegate d_AfterDeleted { get; set; }
        public static DEBusinessItem _item = null;

        #region 插入操作内容

        private const string OPERATION_ID = "PLM31_ExportToERP";
        private const string OPERATION_LABEL = "导入ERP";
        private const string OPERATION_TOOLTIP = "导入到ERP";
        private const string OPERATION_FILTER = "ExportToERPPluginCLT.dll,ExportToERPPluginCLT.ExportToERPFilter";
        private const string OPERATION_EVENTHANDLE = "ExportToERPPluginCLT.dll,ExportToERPPluginCLT.ExportHandlerHelper,OnExportToERP";
        private const string ROOT_BI = "ROOTBI";
        private const string SINGLE = "Single";
        //OPERATION_BINDING_COLLECTION
        private const string OBCOLLECTION_PRODUCT = "PLM";
        private static List<string> OBCOLLECTION_CLASSNAME = new List<string>() { "PART", "BASEDOC", "GYGCK", "GXK" };
        private static List<string> OBCOLLECTION_SENCE = new List<string>() { "CheckOutFolder", "CheckOutRootFolder", "ItemList", "PPCardTemplateList", "PublicFolder", "PublicRootFolder","RelationTreeRoot"
,"RelationTreeChild"};
        private static List<string> OBCOLLECTION_MODE = new List<string>() { "Single" };
        #endregion

        #region 初始化

        public void Init() {
            #region 绑定定版后事件
            this.d_AfterReleased = new PLMBizItemDelegate(AfterItemReleased);
            BizItemHandlerEvent.Instance.D_AfterReleased = (PLMBizItemDelegate)Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterReleased, this.d_AfterReleased);
            #endregion

            #region 添加右键菜单【导入到ERP】

            var list = PLOperationDef.Agent.GetAllOperationItems(Guid.NewGuid());
            if (list != null && list.Count != 0) {
                var opItem = list.Find(p => p.Id == OPERATION_ID);
                if (opItem == null) {
                    AddOperationItem();
                }
                AddOperationBinding();
            }
            #endregion
        }

        /// <summary>
        /// 添加操作类
        /// </summary>
        private void AddOperationItem() {
            DEOperationItem operationItem = new DEOperationItem() {
                Id = OPERATION_ID, Label = OPERATION_LABEL, Tooltip = OPERATION_TOOLTIP, Filter = OPERATION_FILTER, EventHandler = OPERATION_EVENTHANDLE, Option = 0
            };
            PLOperationDef.Agent.CreateOperationItem(operationItem, Guid.NewGuid());
        }

        private void AddOperationBinding() {
            var cols = PLOperationDef.Agent.GetAllBindingCollection(new Guid());
            //var oldCols = GetBindingCollections(cols);
            //PLOperationDef.Agent.SaveBindingCollection
            if (OBCOLLECTION_CLASSNAME.Count == 0) {
                return;
            }
            try {
                for (int i = 0; i < OBCOLLECTION_CLASSNAME.Count; i++) {
                    for (int j = 0; j < OBCOLLECTION_SENCE.Count; j++) {
                        for (int k = 0; k < OBCOLLECTION_MODE.Count; k++) {
                            var col = CreateDEBindingOprt(OBCOLLECTION_CLASSNAME[i], OBCOLLECTION_MODE[k], OBCOLLECTION_SENCE[j], cols);
                            PLOperationDef.Agent.SaveBindingCollection(col, Guid.NewGuid());
                        }
                    }
                }
            } catch (Exception ex) {
                MessageBoxPLM.Show(ex.Message);
            }
        }

        private List<DEBindingOperationCollection> GetBindingCollections(List<DEBindingOperationCollection> oldCols) {
            if (oldCols == null || oldCols.Count == 0) {
                return null;
            }
            List<DEBindingOperationCollection> cols = new List<DEBindingOperationCollection>();
            foreach (DEBindingOperationCollection item in oldCols) {
                if (OBCOLLECTION_SENCE.IndexOf(item.Scene) < 0 || OBCOLLECTION_MODE.IndexOf(item.Mode) < 0) {
                    cols.Add(item);
                    continue;
                }
                DEBindingOperationInfo info = new DEBindingOperationInfo();
            }
            return cols;
        }

        /// <summary>
        /// 创建绑定项
        /// </summary>
        /// <param name="className"></param>
        /// <param name="mode"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        private DEBindingOperationCollection CreateDEBindingOprt(string className, string mode, string scene,List<DEBindingOperationCollection> oldCols) {
            if (string.IsNullOrEmpty(className)) {
                return null;
            }
            DEBindingOperationCollection col = new DEBindingOperationCollection() {
                Product = OBCOLLECTION_PRODUCT, Mode = OperModeType.Single.ToString(), ClassName = className, Scene = scene
            };
            var oldCol = oldCols.Find(p => p.Scene == scene && (p.ClassName == ROOT_BI || p.ClassName == className));
            if (oldCol!=null&&oldCol.OperationInfos!=null&&oldCol.OperationInfos.Count>0) {
                col.OperationInfos.AddRange(oldCol.OperationInfos);
            }
            var currentCol=col.OperationInfos.Find(p=>p.Id==OPERATION_ID);
            if (currentCol==null) {
                DEBindingOperationInfo info = new DEBindingOperationInfo() {
                    Id = OPERATION_ID, Order = 600
                };
                col.OperationInfos.Add(info);
            }
            
            return col;
        }

        public void UnInit() {
            BizItemHandlerEvent.Instance.D_AfterReleased = (PLMBizItemDelegate)Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterReleased, this.d_AfterReleased);
        }

        /// <summary>
        /// 定版后启动
        /// </summary>
        /// <param name="bizItems"></param>
        private void AfterItemReleased(IBizItem[] bizItems) {
            if (bizItems != null) {
                ArrayList list = new ArrayList(bizItems);
                foreach (object obj2 in list) {
                    if (typeof(IBizItem).IsInstanceOfType(obj2)) {
                        IBizItem item = (IBizItem)obj2;
                        //ExportExecute(item);
                        var bItem = BusinessHelper.Instance.GetDEBusinessItem(item);
                        //var ss = OperationConfigHelper.Instance;

                        BusinessHelper.Instance.ExportToERP(bItem,true);
                    }
                }
            }
        }
        #region 测试菜单

        //private void GetMenuTest(IBizItem item) {
        //    PLMOperationArgs args = null;
        //    List<object> items = new List<object>();
        //    items.Add(item);
        //    DEPSOption option = ClientData.UserGlobalOption.CloneAsGlobal();
        //    FolderEnventArgs args2 = new FolderEnventArgs();
        //    args = new PLMOperationArgs(FrmLogon.PLMProduct, PLMLocation.PrivateFolder.ToString(), items, option, args2, null, null, null);
        //    //ContextMenuStrip contextMenu = MenuBuilder.Instance.GetContextMenu(args);
        //    DEBindingOperationCollection curBindInfo = null;
        //    var list = GetOperationList(args, out curBindInfo);
        //}

        //public List<DEOperationItem> GetOperationList(PLMOperationArgs args, out DEBindingOperationCollection curBindInfo) {
        //    curBindInfo = null;
        //    List<object> items = args.Items;
        //    string modeType = GetModeType(items);
        //    Dictionary<string, int> dictionary = new Dictionary<string, int>();
        //    List<string> list2 = new List<string>();
        //    List<DEOperationItem> list3 = new List<DEOperationItem>();
        //    foreach (object obj2 in items) {
        //        DEBindingOperationCollection operations = GetDEBindingOperationCollectionByItem(obj2, args.Location, modeType);
        //        if (operations != null) {
        //            if (curBindInfo == null) {
        //                curBindInfo = operations;
        //            }
        //            foreach (DEBindingOperationInfo info in operations.OperationInfos) {
        //                if (!dictionary.ContainsKey(info.Id)) {
        //                    dictionary[info.Id] = 1;
        //                } else {
        //                    Dictionary<string, int> dictionary2;
        //                    string str2;
        //                    (dictionary2 = dictionary)[str2 = info.Id] = dictionary2[str2] + 1;
        //                }
        //                if (!list2.Contains(info.Id) || IsSpliter(info.Id)) {
        //                    list2.Add(info.Id);
        //                }
        //            }
        //        }
        //    }
        //    for (int i = 0; i < list2.Count; i++) {
        //        int num3 = dictionary[list2[i]];
        //        if ((num3.Equals(items.Count) || this.IsSpliter(list2[i])) && OperationConfigHelper.Instance.DEOperationItems.ContainsKey(list2[i])) {
        //            list3.Add(OperationConfigHelper.Instance.DEOperationItems[list2[i]]);
        //        }
        //    }
        //    if (items.Count == 0) {
        //        DEBindingOperationCollection operations2 = this.GetDEBindingOperationCollectionByItem(null, args.Location, modeType);
        //        if (operations2 == null) {
        //            return list3;
        //        }
        //        curBindInfo = operations2;
        //        foreach (DEBindingOperationInfo info2 in operations2.OperationInfos) {
        //            if (!dictionary.ContainsKey(info2.Id)) {
        //                dictionary[info2.Id] = 1;
        //            } else {
        //                Dictionary<string, int> dictionary3;
        //                string str3;
        //                (dictionary3 = dictionary)[str3 = info2.Id] = dictionary3[str3] + 1;
        //            }
        //            if (!list2.Contains(info2.Id) || this.IsSpliter(info2.Id)) {
        //                list2.Add(info2.Id);
        //            }
        //        }
        //        for (int j = 0; j < list2.Count; j++) {
        //            if (OperationConfigHelper.Instance.DEOperationItems.ContainsKey(list2[j])) {
        //                list3.Add(OperationConfigHelper.Instance.DEOperationItems[list2[j]]);
        //            }
        //        }
        //    }
        //    return list3;
        //}

        //private string GetModeType(List<object> objs) {
        //    if ((objs != null) && (objs.Count != 0)) {
        //        if (objs.Count == 1) {
        //            return OperModeType.Single.ToString();
        //        }
        //        if (objs.Count > 1) {
        //            return OperModeType.Multi.ToString();
        //        }
        //    }
        //    return OperModeType.Empty.ToString();
        //}

        //private DEBindingOperationCollection GetDEBindingOperationCollectionByItem(object item, string scene, string mode) {
        //    if (item is IBizItem) {
        //        IBizItem item2 = item as IBizItem;
        //        return OperationConfigHelper.Instance.GetDEBindingOperationCollection(item2.ClassName, null, scene, mode);
        //    }
        //    if (item is DERelationBizItem) {
        //        DERelationBizItem item3 = item as DERelationBizItem;
        //        return OperationConfigHelper.Instance.GetDEBindingOperationCollection(item3.BizItem.ClassName, item3.Relation.RelationName, scene, mode);
        //    }
        //    if (item is DataRow) {
        //        DataRow row = item as DataRow;
        //        if ((row.Table.Columns.Contains("PLM_CLASSNAME") && (row["PLM_CLASSNAME"] != null)) && (!row["PLM_CLASSNAME"].Equals(DBNull.Value) && !row["PLM_CLASSNAME"].Equals(""))) {
        //            return OperationConfigHelper.Instance.GetDEBindingOperationCollection(Convert.ToString(row["PLM_CLASSNAME"]), null, scene, mode);
        //        }
        //        return null;
        //    }
        //    if (!(item is DEFolder2) && (!(item is DEMetaClass) || !(scene == PLMLocation.CheckOutFolder.ToString()))) {
        //        return OperationConfigHelper.Instance.GetDEBindingOperationCollection(null, null, scene, mode);
        //    }
        //    return OperationConfigHelper.Instance.GetDEBindingOperationCollection("PSM_FOLDER", null, scene, mode);
        //}


        //private bool IsSpliter(string Id) {
        //    return (Id == "_SPLITTER");
        //}
        #endregion
        #endregion

    }
}
