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
        IMyBlockGroup TOADSGroup;
        bool hasAzimuthStab;
        int timeToNextRefresh;

        string echoString;
        string refreshString;


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Once | UpdateFrequency.Update10 | UpdateFrequency.Update100;
            hasAzimuthStab = false;
            timeToNextRefresh = -1;
            echoString = "";
            refreshString = "";
        }

        public void Main(string args, UpdateType updateType)
        {
            //=====Setup=====
            if ((updateType & UpdateType.Once | UpdateType.Update100) != 0 && (updateType & UpdateType.Update10)==0)
            {
                if (timeToNextRefresh >= 0)
                {
                    timeToNextRefresh--;
                }
                else
                {
                    timeToNextRefresh = 10;

                    TOADSGroup = GridTerminalSystem.GetBlockGroupWithName("TOADS");
                    if (TOADSGroup == null)
                    {
                        echoString = "No group with the name \"TOADS\" found on the vehicle";
                    }
                    else
                    {

                        echoString = "";
                    }
                }
                refreshString = "\nTime to next block refresh: " + timeToNextRefresh;
            }

            //=====Update=====
            if((updateType & UpdateType.Update10) != 0)
            {

            }
            Echo(echoString + refreshString);
        }
    }
}