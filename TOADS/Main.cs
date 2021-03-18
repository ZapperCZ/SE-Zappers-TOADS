using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using VRage.Game.ObjectBuilders.Definitions;

namespace SpaceEngineers
{
    public sealed class Program : MyGridProgram
    {
        List<IMyTextPanel> OutputLCDs;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            OutputLCDs = new List<IMyTextPanel>();
            List<IMyTerminalBlock> TempBlockList = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName("TOADS", TempBlockList);
            foreach(IMyTerminalBlock TerminalBlock in TempBlockList)
            {
                if(TerminalBlock as IMyTextPanel != null)
                {
                    OutputLCDs.Add(TerminalBlock as IMyTextPanel);
                }
            }
            Echo("TOADS\nDetected " + OutputLCDs.Count + " LCDs");
        }

        public void Main(string args)
        {
            foreach(IMyTextPanel LCD in OutputLCDs)
            {
                LCD.WriteText("TOADS");
            }
        }
    }
}
