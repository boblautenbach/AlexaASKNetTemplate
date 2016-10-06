using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EchoTemplate.Helpers
{
    //Should use this class to double-check any custom slots 
    //to ensure the intent is interpreting the slot value correctly
    public static class SlotChecker
    {
        public static List<string> SlotA = new List<string>();
        public static List<string> SlotB = new List<string>();
        public static List<string> SlotC = new List<string>();

        static SlotChecker()
        {
            SlotA.Add("");
            SlotA.Add("");

            SlotB.Add("");
            SlotB.Add("");

            SlotC.Add("");
            SlotC.Add("");
        }
    }
}