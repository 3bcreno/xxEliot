using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using static MrLeeSin.Common;

namespace LeeSinEnsoul
{
    class Program
    {

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs e)
        {
            MrLeeSin.Program.Game_OnGameLoad();
            Game.OnUpdate += Game_OnUpdate;

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            EnsoulSharp.Common.Orbwalking.Attack = true;
            EnsoulSharp.Common.Orbwalking.Move = true;

        }
    }

}
