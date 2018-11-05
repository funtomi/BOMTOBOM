using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thyt.TiPLM.DEL.Admin.DataModel;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Admin.DataModel;

namespace ExportBOMToERP {
    public class DalFactory {
        private DalFactory() {
        }

        public static DalFactory Instance;
        static DalFactory() {
            Instance = new DalFactory();
        }
        /// <summary>
        /// 创建数据类型
        /// </summary>
        /// <param name="typeStr"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public BaseDal CreateDal(DEBusinessItem item,string typeStr) {
            if (item==null) {
                return null;
            }
            BaseDal dal = null;
            BusinessType type;
            var hasDefine = this.TryGetBusinessType(typeStr, out type);
            if (!hasDefine) {
                return null;
            }
            switch (type) {
                case BusinessType.UnitGroup:
                    dal = new UnitGroupDal(item);
                    break;
                case BusinessType.Unit:
                    dal = new UnitDal(item);
                    break;
                case BusinessType.Inventory:
                    dal = new InventoryDal(item);
                    break;
                case BusinessType.InventoryClass:
                    dal = new InventoryClassDal(item);
                    break;
                case BusinessType.Resource:
                    dal = new ResourceDal(item);
                    break;
                case BusinessType.Routing:
                    dal = new RoutingDal(item);
                    break;
                case BusinessType.WorkCenter:
                    dal = new WorkCenterDal(item);
                    break;
                case BusinessType.Bom:
                    dal = new BomDal(item);
                    break;
                case BusinessType.Operation:
                    dal = new OperationDal(item);
                    break;
            }
            return dal;
        }

        /// <summary>
        /// 获取业务类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="businessType"></param>
        /// <returns></returns>
        public bool TryGetBusinessType(string type,out BusinessType businessType) {
            businessType = BusinessType.Unit;
            if (string.IsNullOrEmpty(type)) {
                return false;
            }
            switch (type.ToLower()) {
                default:
                    var parent = ModelContext.MetaModel.GetParent(type);
                    if (parent==null) {
                        return false;
                    }
                    return TryGetBusinessType(parent.Name, out businessType);
                case "unit":
                    businessType = BusinessType.Unit;
                    break;
                case "unitgroup":
                    businessType= BusinessType.UnitGroup;
                    break;
                case "inventoryclass":
                    businessType = BusinessType.InventoryClass;
                    break;
                case "tipart":
                case "tigz":
                case "part":
                    businessType = BusinessType.Inventory;
                    break;
                //case "gxk":
                //case "gx":
                //    businessType = BusinessType.Operation;
                //    break;
                //case "gygck":
                //    businessType = BusinessType.Routing;
                //    break;
                case "bom":
                    businessType = BusinessType.Bom;
                    break;
                case "resourcedoc":
                    businessType = BusinessType.Resource;
                    break;
                case "workcenters":
                    businessType = BusinessType.WorkCenter;
                    break;
            }
            return true;
        }

        
    }

    public enum BusinessType{
        UnitGroup,Unit,Inventory,InventoryClass,Resource,Routing,WorkCenter,Bom,Operation
    }
}
