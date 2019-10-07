using System;
using EnsoulSharp;
using EnsoulSharp.Common;
using static MrLeeSin.Common;
using System.Reflection.Emit;



namespace MrLeeSin
{
    class Loader
    {
        public static void Load()
        {
            _MainMenu = new Menu("EnsoulSharp (" + ObjectManager.Player.CharacterName + ")", "EnsoulSharp", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.Aqua);
            Menu orbwalkerMenu = new Menu("OrbWalker", "OrbWalker");
            _OrbWalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _MainMenu.AddSubMenu(orbwalkerMenu);
            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _MainMenu.AddSubMenu(targetSelectorMenu);
           Type t = Type.GetType("LeeSinEnsoul.Champion." + ObjectManager.Player.CharacterName);
            if (t != null)
            {
                //Object obj = Activator.CreateInstance(t);
                var target = t.GetConstructor(Type.EmptyTypes);
                var dynamic = new DynamicMethod(string.Empty, t, new Type[0], target.DeclaringType);
                var il = dynamic.GetILGenerator();
                il.DeclareLocal(target.DeclaringType);
                il.Emit(OpCodes.Newobj, target);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);
                var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
                method();
            }
            else
            {
                Chat.Print("It's Not LeeSin");
            }
        }
    }
}
