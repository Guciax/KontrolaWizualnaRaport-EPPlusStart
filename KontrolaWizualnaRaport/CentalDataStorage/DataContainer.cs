using MST.MES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaWizualnaRaport
{
    public class DataContainer
    {
        public static Dictionary<string, OrderStructureByOrderNo.OneOrderData> sqlDataByOrder = new Dictionary<string, OrderStructureByOrderNo.OneOrderData>();
        public static OrderStructureByOrderNo.DataByProcess sqlDataByProcess = new OrderStructureByOrderNo.DataByProcess();
        public static Dictionary<int, string> testerIdToName = new Dictionary<int, string>();
        public static Dictionary<string, string> nc12ToName = new Dictionary<string, string>();
        public static Dictionary<string, ModelInfo.ModelSpecification> mesModels = new Dictionary<string, ModelInfo.ModelSpecification>();

    }
}
