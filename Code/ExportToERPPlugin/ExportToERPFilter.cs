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
        public bool Filter(PLMOperationArgs args, DEOperationItem item) {
            if (args.BizItems == null || args.BizItems.Length == 0) {
                return false;
            }
            var iItem = args.BizItems[0];
            var bItem = BusinessHelper.Instance.GetDEBusinessItem(iItem);
            if (bItem == null) {
                return false;
            }
            BusinessType type;
            return DalFactory.Instance.TryGetBusinessType(bItem.ClassName,out type);
        }
    }
}
