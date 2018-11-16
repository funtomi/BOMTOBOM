using System.Collections;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Controls;

namespace ExportToERPPluginCLT {
    public class ExportHandlerHelper {
        public void OnExportToERP(object sender, PLMOperationArgs args) {
            if (((args != null) && (args.BizItems != null)) && (args.BizItems.Length != 0)) {
                //IBizItem item = args.BizItems[0];
                //var bItem = BusinessHelper.Instance.GetDEBusinessItem(item);
                //20181113 modified by kexp 修改为导入完成后统一提示一次；
                //BusinessHelper.Instance.ExportToERP(bItem,false);
                //DEBusinessItem theItem = PSConvert.ToBizItem(args.BizItems[0], args.Option.CurView, ClientData.LogonUser);
                string message = "ERP导入成功!";
                ArrayList list = new ArrayList(args.BizItems);
                foreach (object obj2 in list) {
                    if (typeof(IBizItem).IsInstanceOfType(obj2)) {
                        IBizItem item = (IBizItem)obj2;
                        //ExportExecute(item);
                        var bItem = BusinessHelper.Instance.GetDEBusinessItem(item);
                        //var ss = OperationConfigHelper.Instance;
                        string errText;
                        BusinessHelper.Instance.ExportToERP(bItem, false, out errText);
                        if (errText == "ERP导入成功!") {
                            continue;
                        } else {
                            message = errText;
                            break;
                        }
                    }
                }
                MessageBoxPLM.Show(message);
            }
        }

    }
}
