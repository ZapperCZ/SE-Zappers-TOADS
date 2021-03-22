using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VRageMath;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
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
        IMyMotorStator AzimuthCupolaRotor;
        IMyBlockGroup TOADSGroup;
        bool hasAzimuthStab;
        bool hasCupola;
        float azimuthOffset;
        float stabilizerOffset;
        int timeToNextRefresh;
        int tankOutlineThickness;

        string echoString;
        string refreshString;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Once | UpdateFrequency.Update10 | UpdateFrequency.Update100;
            OutputLCDs = new List<IMyTextPanel>();
            hasAzimuthStab = true;
            hasCupola = true;
            timeToNextRefresh = 0;
            azimuthOffset = 0;
            stabilizerOffset = 0;
            tankOutlineThickness = 10;
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
                        SetCustomData("Has_Stabilizer", "true");
                        SetCustomData("Has_Cupola","true");
                        SetCustomData("Azimuth_Offset", "0");
                        SetCustomData("Stabilizer_Offset", "0");
                        SetCustomData("Tank_Outline_Thickness", "10");
                    }

                    tankOutlineThickness = Convert.ToInt32(GetCustomData("Tank_Outline_Thickness"));
                    hasAzimuthStab = Convert.ToBoolean(GetCustomData("Has_Stabilizer"));
                    hasCupola = Convert.ToBoolean(GetCustomData("Has_Cupola"));
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

                        TOADSGroup.GetBlocksOfType(OutputLCDs);
                        TOADSGroup.GetBlocksOfType(TempRotorList);
                        foreach(IMyMotorStator Rotor in TempRotorList)
                        {
                            if (Rotor.CustomName.ToLower().Contains("azimuth"))
                            {
                                if (Rotor.CustomName.ToLower().Contains("stabilization"))
                                {
                                    AzimuthStabilizer = Rotor;
                                }
                                else if(Rotor.CustomName.ToLower().Contains("cupola"))
                                {
                                    AzimuthCupolaRotor = Rotor;
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
                echoString = "Turret angle to hull > " + Math.Round(CalculateTurretAngle(), 0);
                echoString = "Cupola angle to hull > " + Math.Round(CalculateCupolaAngle(), 0);

                foreach (IMyTextPanel LCDsurface in OutputLCDs)
                {
                    UpdateLCD(LCDsurface);
                }
            }
            Echo(echoString + refreshString);
        }

        private void UpdateLCD(IMyTextPanel surface)
        {
            RectangleF viewport = new RectangleF(
                (surface.TextureSize - surface.SurfaceSize) / 2,
                surface.SurfaceSize
            );
            MySpriteDrawFrame frame = surface.DrawFrame();
            DrawSprites(ref frame, viewport, surface);
            frame.Dispose();
        }

        private void DrawSprites(ref MySpriteDrawFrame frame, RectangleF viewport, IMyTextPanel surface)
        {
            float turretRotation = Convert.ToSingle(Math.Round(CalculateTurretAngle(),0));
            int cupolaRotation = Convert.ToInt32(Math.Round(CalculateCupolaAngle(),0));
            float turretRotationRad = Convert.ToSingle(DegreeToRadian(turretRotation));
            float gunRotationRad = turretRotationRad + Convert.ToSingle(DegreeToRadian(90));
            int gunLength = 180;
            int gunWidth = 20;
            int turretLength = 140;
            int turretWidth = 100;

            float gunOffset = (turretLength / 2) + (gunLength / 2);
            double gunPositionX = 0;
            double gunPositionY = 0;

            gunPositionX = Math.Cos(gunRotationRad) * gunOffset * -1;

            //Switching Y values because radians
            if (turretRotation >= 90 && turretRotation <= 270)
            {
                gunPositionY = Math.Round(Math.Sqrt(Math.Pow(gunOffset, 2) - Math.Pow(gunPositionX, 2)));
            }
            else
            {
                gunPositionY = Math.Round(Math.Sqrt(Math.Pow(gunOffset, 2) - Math.Pow(gunPositionX, 2))) * -1;
            }

            Vector2 turretHeadingPosition = new Vector2(256, 15) + viewport.Position;
            Vector2 hullPosition = new Vector2(0,40) + viewport.Center;
            Vector2 turretPosition = new Vector2(0, 55) + hullPosition;
            Vector2 gunPosition = new Vector2((Int32)gunPositionX ,(Int32)gunPositionY) + turretPosition;

            MySprite turretHeading = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = "Turret rotation > " + turretRotation.ToString(),
                Position = turretHeadingPosition,
                RotationOrScale = 1.1f,
                Color = surface.ScriptForegroundColor,
                Alignment = TextAlignment.CENTER,
                FontId = "White"
            };

            MySprite cupolaHeading = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = "Cupola rotation > " + cupolaRotation.ToString(),
                Position = new Vector2(0,30) + turretHeadingPosition,
                RotationOrScale = 1.1f,
                Color = surface.ScriptForegroundColor,
                Alignment = TextAlignment.CENTER,
                FontId = "White"
            };

            MySprite hullSpriteOutline = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareHollow",
                Position = hullPosition,
                Size = new Vector2(140,300),
                Color = surface.ScriptForegroundColor,
                Alignment =
                TextAlignment.CENTER,
            };
            
            MySprite hullSpriteFill = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareTapered",
                Position = hullPosition,
                Size = new Vector2(140 - tankOutlineThickness, 300 - tankOutlineThickness),
                Color = surface.ScriptBackgroundColor,
                Alignment = TextAlignment.CENTER,
            };
            
            MySprite turretSpriteOutline = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareHollow",
                Position = turretPosition,
                RotationOrScale = turretRotationRad,
                Size = new Vector2(turretWidth, turretLength),
                Color = surface.ScriptForegroundColor,
                Alignment = TextAlignment.CENTER,
            };
            
            MySprite turretSpriteFill = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareTapered",
                Position = turretPosition,
                RotationOrScale = turretRotationRad,
                Size = new Vector2(turretWidth - tankOutlineThickness, turretLength - tankOutlineThickness),
                Color = surface.ScriptBackgroundColor,
                Alignment = TextAlignment.CENTER,
            };

            MySprite gunSprite = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareHollow",
                Position = gunPosition,
                RotationOrScale = gunRotationRad - Convert.ToSingle(DegreeToRadian(90)),
                Size = new Vector2(gunWidth, gunLength),
                Color = surface.ScriptForegroundColor,
                Alignment = TextAlignment.CENTER,
            };
            

            frame.Add(hullSpriteOutline);
            //frame.Add(hullSpriteFill);
            frame.Add(turretSpriteOutline);
            //frame.Add(turretSpriteFill);
            frame.Add(gunSprite);
            frame.Add(turretHeading);
            frame.Add(cupolaHeading);
        }

        private double CalculateTurretAngle()
        {
            double result = 0;
            double azimuthAng = RadianToDegree(AzimuthRotor.Angle);
            if (hasAzimuthStab)
            {
                double stabAng = RadianToDegree(AzimuthStabilizer.Angle);
                double calc = (azimuthAng + azimuthOffset) + (stabAng + stabilizerOffset);
                if ( calc > 360)
                {
                    result = calc - 360;
                }
                else
                {
                    result = calc;
                }

            }
            else
            {
                result = azimuthAng + azimuthOffset;
            }
            return result;
        }
        private double CalculateCupolaAngle()
        {
            double result = 0;
            double azimuthAng = RadianToDegree(AzimuthCupolaRotor.Angle);
            result = azimuthAng + CalculateTurretAngle();
            if(result >= 360)
            {
                result -= 360;
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
                    echoString += "Error while getting Custom Data, couldn't find requested Variable";
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
        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}