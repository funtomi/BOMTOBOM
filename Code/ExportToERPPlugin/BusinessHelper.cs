using ExportBOMToERP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Product2;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Controls;
using Thyt.TiPLM.UIL.Product.Common;

namespace ExportToERPPluginCLT {
    public class BusinessHelper {
        static BusinessHelper() {
            Instance = new BusinessHelper();
        }
        private BusinessHelper() {

        }
        public static BusinessHelper Instance;

        /// <summary>
        /// 获取当前对象
        /// </summary>
        /// <param name="iItem"></param>
        /// <returns></returns>
        public DEBusinessItem GetDEBusinessItem(IBizItem iItem) {
            IActionContext context = ActionContext2.GetInstance().GetCurrentContext();
            DEPSOption option = null;
            if (context != null && context.NavContext.Option != null) {
                option = context.NavContext.Option;
            }
            if (option == null)
                option = ClientData.UserGlobalOption;
            if (iItem == null)
                return null;
            DEBusinessItem bItem = iItem as DEBusinessItem;

            if (bItem == null && iItem is DESmartBizItem) {
                DESmartBizItem sb = iItem as DESmartBizItem;
                bItem = (DEBusinessItem)PLItem.Agent.GetBizItem(sb.MasterOid, sb.RevOid, sb.IterOid,
                                                                 option.CurView, ClientData.LogonUser.Oid,
                                                                 BizItemMode.BizItem);
            }
            return bItem;
        }

        //20181113 modified by kexp 修改为导入完成后统一提示一次；
        /// <summary>
        /// 导入到ERP
        /// </summary>
        /// <param name="bItem"></param>
        public void ExportToERP(DEBusinessItem bItem,out string errText) {
            errText = "";
            if (bItem==null) {
                errText="没有获取到对象！";
                return;
            }
            ExportService srv = new ExportService(bItem);
            srv.AddOrEditItem(out errText);
        }

        //20181113 modified by kexp 修改为导入完成后统一提示一次；
        public void ExportToERP(DEBusinessItem bItem, bool isRelease,out string errText) {
            errText = "";
            if (!isRelease) {
                var state = bItem.State;
                if (state!= ItemState.Release) {
                    errText=string.Format("{0}没有定版，不能直接导入ERP！", bItem.Name);
                    return;
                }
            }
            ExportToERP(bItem,out errText);
        }
    }
}
