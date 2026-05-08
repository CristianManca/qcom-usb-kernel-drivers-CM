using System.Collections.Generic;

namespace QCDriverConstant
{
    public class WPPConstantItem
    {
        public uint Value { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }

        public WPPConstantItem(uint value, string name, string category)
        {
            Value = value;
            Name = name;
            Category = category;
        }

        public static List<WPPConstantItem> GetWPPConstants(string what)
        {
            List<WPPConstantItem> list = new List<WPPConstantItem>();
            switch(what)
            {
                case "WPP_DRV_MASK":
                    GetWPPMasks(ref list);
                    break;
                case "WPP_LEVEL":
                    GetWPPLevels(ref list);
                    break;
                default:
                    break;
            }
            return list;
        }

        private static void GetWPPMasks(ref List<WPPConstantItem> list)
        {
            list.Add(new WPPConstantItem(0x00000001, "WPP_DRV_MASK_CONTROL", "Control messages"));
            list.Add(new WPPConstantItem(0x00000002, "WPP_DRV_MASK_READ", "Read operations"));
            list.Add(new WPPConstantItem(0x00000004, "WPP_DRV_MASK_WRITE", "Write operations"));
            list.Add(new WPPConstantItem(0x00000008, "WPP_DRV_MASK_ENCAP", "Encapsulated operations"));
            list.Add(new WPPConstantItem(0x00000010, "WPP_DRV_MASK_POWER", "Power messages"));
            list.Add(new WPPConstantItem(0x00000020, "WPP_DRV_MASK_STATE", "State information"));
            list.Add(new WPPConstantItem(0x00000040, "WPP_DRV_MASK_RDATA", "Received data"));
            list.Add(new WPPConstantItem(0x00000080, "WPP_DRV_MASK_TDATA", "Transmitted data"));
            list.Add(new WPPConstantItem(0x00000100, "WPP_DRV_MASK_ENDAT", "Encapsulated data"));
            list.Add(new WPPConstantItem(0x00000200, "WPP_DRV_MASK_RIRP", "Read IRPs"));
            list.Add(new WPPConstantItem(0x00000400, "WPP_DRV_MASK_WIRP", "Write IRPs"));
            list.Add(new WPPConstantItem(0x00000800, "WPP_DRV_MASK_CIRP", "Control IRPs"));
            list.Add(new WPPConstantItem(0x00001000, "WPP_DRV_MASK_PIRP", "Power IRPs"));
            list.Add(new WPPConstantItem(0x00002000, "WPP_DRV_MASK_PROTOCOL", "General protocol"));
            list.Add(new WPPConstantItem(0x00004000, "WPP_DRV_MASK_MCONTROL", "Miniport control"));
            list.Add(new WPPConstantItem(0x00008000, "WPP_DRV_MASK_QOS", "Quality of service"));
            list.Add(new WPPConstantItem(0x00010000, "WPP_DRV_MASK_DATA_QOS", "Quality of service data"));
            list.Add(new WPPConstantItem(0x00020000, "WPP_DRV_MASK_DATA_WT", "Miniport write data"));
            list.Add(new WPPConstantItem(0x00040000, "WPP_DRV_MASK_DATA_RD", "Miniport read data"));
            list.Add(new WPPConstantItem(0x00080000, "WPP_DRV_MASK_FILTER", "Filter driver"));
        }

        private static void GetWPPLevels(ref List<WPPConstantItem> list)
        {
            list.Add(new WPPConstantItem(0x00, "0x00 WPP_LEVEL_FORCE", "Always output matching messages of this level"));
            list.Add(new WPPConstantItem(0x01, "0x01 WPP_LEVEL_CRITICAL", "Critical failures"));
            list.Add(new WPPConstantItem(0x02, "0x02 WPP_LEVEL_ERROR", "Error messages"));
            list.Add(new WPPConstantItem(0x03, "0x03 WPP_LEVEL_INFO", "Summary information"));
            list.Add(new WPPConstantItem(0x04, "0x04 WPP_LEVEL_DETAIL", "Detailed information"));
            list.Add(new WPPConstantItem(0x05, "0x05 WPP_LEVEL_TRACE", "Trace information"));
            list.Add(new WPPConstantItem(0xFF, "0x06 WPP_LEVEL_VERBOSE", "All information"));
        }
    }
}
