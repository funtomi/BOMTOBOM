using ExportBOMToERP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thyt.TiPLM.DEL.Operation;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Common.Operation;

namespace ExportToERPPluginCLT {
    public class ExportToERPFilter : IOperationFilter {
        //菜单过滤器
        //20181113 modified by kexp 修改为导入完成后统一提示一次；
        public bool Filter(PLMOperationArgs args, DEOperationItem item) {
            if (args.BizItems == null || args.BizItems.Length == 0) {
                return false;
            }
            foreach (var iItem in args.BizItems) {
                var bItem = BusinessHelper.Instance.GetDEBusinessItem(iItem);
                if (bItem == null) {
                    return false;
                }

                BusinessType type;
                ExportService srv = new ExportService(bItem);
                var result =  DalFactory.Instance.TryGetBusinessType(bItem.ClassName,srv.IgnoreClasses, out type);
                if (!result) {
                    return false;
                }
            }
            return true;
            
        }
    }
}
