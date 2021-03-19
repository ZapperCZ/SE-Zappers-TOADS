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
        IMyMotorStator AzimuthRotor;
        IMyMotorStator AzimuthStabilizer;
        IMyBlockGroup TOADSGroup;
        bool hasAzimuthStab;
        float azimuthOffset;
        float stabilizerOffset;
        int timeToNextRefresh;

        string echoString;
        string refreshString;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Once | UpdateFrequency.Update10 | UpdateFrequency.Update100;
            hasAzimuthStab = false;
            timeToNextRefresh = 0;
            azimuthOffset = 0;
            stabilizerOffset = 0;
            echoString = "";
            refreshString = "";
        }

        public void Main(string args, UpdateType updateType)
        {
            //=====Setup=====
            if ((updateType & UpdateType.Once | UpdateType.Update100) != 0 && (updateType & UpdateType.Update10)==0)
            {
                if (timeToNextRefresh > 0)
                {
                    timeToNextRefresh--;
                }
                else
                {
                    timeToNextRefresh = 10;

                    if (Me.CustomData == "")
                    {
                        SetCustomData("Has_Stabilizer", "false");
                        SetCustomData("Azimuth_Offset", "0");
                        SetCustomData("Stabilizer_Offset", "0");
                    }

                    hasAzimuthStab = Convert.ToBoolean(GetCustomData("Has_Stabilizer"));
                    azimuthOffset = Convert.ToSingle(GetCustomData("Azimuth_Offset"));
                    if (hasAzimuthStab)
                    {
                        stabilizerOffset = Convert.ToSingle(GetCustomData("Stabilizer_Offset"));
                    }

                    TOADSGroup = GridTerminalSystem.GetBlockGroupWithName("TOADS");
                    if (TOADSGroup == null)
                    {
                        echoString = "No group with the name \"TOADS\" found on the vehicle";
                    }
                    else
                    {
                        List<IMyMotorStator> TempRotorList = new List<IMyMotorStator>();

                        TOADSGroup.GetBlocksOfType<IMyTextPanel>(OutputLCDs);
                        TOADSGroup.GetBlocksOfType(TempRotorList);
                        foreach(IMyMotorStator Rotor in TempRotorList)
                        {
                            if (Rotor.CustomName.ToLower().Contains("azimuth"))
                            {
                                if (Rotor.CustomName.ToLower().Contains("stabilization"))
                                {
                                    AzimuthStabilizer = Rotor;
                                }
                                else
                                {
                                    AzimuthRotor = Rotor;
                                }
                            }
                        }
                    }
                }
                refreshString = "\nTime to next block refresh: " + timeToNextRefresh;
            }

            //=====Update=====
            if((updateType & UpdateType.Update10) != 0)
            {
                echoString = "Turret angle to hull > " + Math.Round(CalculateTurretAngle(),0);
                UpdateLCD();
            }
            Echo(echoString + refreshString);
        }

        private void UpdateLCD()
        {
            List<IMyTextPanel> LCDs = new List<IMyTextPanel>();
            TOADSGroup.GetBlocksOfType(LCDs);
            foreach(IMyTextPanel LCD in LCDs)
            {
                LCD.WriteText(Math.Round(CalculateTurretAngle(),0).ToString());
            }
        }

        private double CalculateTurretAngle()
        {
            double result = 0;
            double azimuthAng = RadianToDegree(AzimuthRotor.Angle);
            if (hasAzimuthStab)
            {
                double stabAng = RadianToDegree(AzimuthStabilizer.Angle);
                if ((azimuthAng + azimuthOffset) + (stabAng + stabilizerOffset) > 360)
                {
                    result = ((azimuthAng + azimuthOffset) + (stabAng + stabilizerOffset)) - 360;
                }
                else
                {
                    result = ((azimuthAng + azimuthOffset) + (stabAng + stabilizerOffset));
                }

            }
            else
            {
                result = azimuthAng + azimuthOffset;
            }
            return result;
        }

        private string GetCustomData(string varName)
        {
            if (Me.CustomData != "")
            {
                try
                {
                    string data = Me.CustomData;
                    int startPosition = data.IndexOf(varName);
                    startPosition = data.IndexOf("=", startPosition) + 2;
                    string result = "";
                    try
                    {
                        result = data.Substring(startPosition, data.IndexOf("\n", startPosition) - startPosition);
                    }
                    catch
                    {
                        result = data.Substring(startPosition);
                    }
                    if (result != "") return result;
                }
                catch
                {
                    echoString = "Error while getting Custom Data, couldn't find requested Variable";
                }
            }
            return "invalid";
        }
    
        private void SetCustomData(string varName, string varValue)
        {
            string tempData = Me.CustomData;
            string data = Me.CustomData;
            if (data.Contains(varName))
            {
                int startPosition = data.IndexOf(varName) - 1;
                try
                {
                    data = data.Substring(0, data.IndexOf("\n", startPosition)) + "\n";
                    data = data + varName + " = " + varValue;
                }
                catch
                {
                    data = varName + " = " + varValue;
                }
                startPosition = tempData.IndexOf(varName);
                try
                {
                    tempData = tempData.Substring(tempData.IndexOf("\n", startPosition));
                }
                catch
                {
                    tempData = "";
                }
                Me.CustomData = data + tempData;
            }
            else
            {
                Me.CustomData = Me.CustomData + varName + " = " + varValue + "\n";
            }
        }
        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
    }
}