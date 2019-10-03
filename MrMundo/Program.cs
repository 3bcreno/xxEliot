using System;
using System.Collections.Generic;
using System.Linq;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.Utils;

using Color = System.Drawing.Color;
using SharpDX;


namespace MrMundo
{
    internal class Program
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot Ignite;

        public static Menu Menu, ComboMenu, HarassMenu, FarmMenu, DrawMenu;
        public static int ResetTime;
        public static bool HasW
        {
            get { return Player.HasBuff("BurningAgony") || ResetTime > Environment.TickCount; }
        }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        

        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "DrMundo") return;
            
            Menu = new Menu("MrMundoEnsoul", "MrMundoEnsoul", true);
            ComboMenu = new Menu("Comb", "Mr Mundo - Comb");
            ComboMenu.Add(new MenuBool("useQCombo", "Use Q"));
            ComboMenu.Add(new MenuBool("useWCombo", "Use W"));
            ComboMenu.Add(new MenuBool("useECombo", "Use E"));
            ComboMenu.Add(new MenuBool("useRCombo", "Use R"));
            ComboMenu.Add(new MenuSlider("useRComboHPPercent", "HP %", 30));
            ComboMenu.Add(new MenuSlider("useRComboEnemies", "Min Enemies", 0, 0, 5));
            Menu.Add(ComboMenu);
            HarassMenu = new Menu("Harass", "Mr Mundo - Harass");
            HarassMenu.Add(new MenuBool("useQHarass", "Use Q"));
            HarassMenu.Add(new MenuBool("useWHarass", "Use W"));
            HarassMenu.Add(new MenuBool("useEHarass", "Use E"));
            Menu.Add(HarassMenu);
            FarmMenu = new Menu("Farming", "Mr Mundo - Farming");
            FarmMenu.Add(new MenuSeparator("Last Hit", "Last Hit"));
            FarmMenu.Add(new MenuBool("useQLH", "Use Q"));
            FarmMenu.Add(new MenuSeparator("Wave Clear", "Wave Clear"));
            FarmMenu.Add(new MenuBool("useQWC", "Use Q"));
            Menu.Add(FarmMenu);
            DrawMenu = new Menu("Drawing Settings", "Drawing Settings");
            DrawMenu.Add(new MenuBool("drawQ", "Draw Q", false));
            DrawMenu.Add(new MenuBool("drawW", "Draw W", false));
            DrawMenu.Add(new MenuBool("drawE", "Draw E", false));
            Menu.Add(DrawMenu);
            Menu.Attach();
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 320);
            E = new Spell(SpellSlot.E, Player.Instance.GetRealAutoAttackRange());
            R = new Spell(SpellSlot.R);
            Ignite = _Player.GetSpellSlot("summonerdot");

            Q.SetSkillshot(0.5f, 75f, 1500, true, true, SkillshotType.Line);

            AIBaseClient.OnProcessSpellCast += AIBaseClient_OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;

            Chat.Print("xxEliot Mr.Mundo", System.Drawing.Color.Black);
            
        }

        private static void OnUpdate(EventArgs args)
        {

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
            {
                WaveClear();
            }
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LastHit))
            {
                LastHit();
            }
            //switch (Orbwalker.ActiveMode)
            //{
            //    case OrbwalkerMode.Combo:
            //        Combo();
            //        break;
            //    case OrbwalkerMode.Harass:
            //        Harass();
            //        break;
            //    case OrbwalkerMode.LastHit:
            //        LastHit();
            //        break;
            //    case OrbwalkerMode.LaneClear:
            //        WaveClear();
            //        break;
            //}
        }

        private static void WaveClear()
        {
            var minions = ObjectManager.Get<AIBaseClient>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            var minionQ = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range) && e.IsMinion())
                         .Cast<AIBaseClient>().ToList();
            if (!Q.IsReady() || !Program.FarmMenu["useQWC"].GetValue<MenuBool>().Enabled) return;
            var useQ = FarmMenu["useQWC"].GetValue<MenuBool>().Enabled;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget() && _Player.Distance(minion) <= Q.Range)
                {
                    Q.Cast(minion.Position , EntityManager.MinionsAndMonsters.EnemyMinions.OrderByDescending(a => a.MaxHealth).Any(
       a => a.Health <= Program.QDamage(a)));
                }
            }

        }

        private static void LastHit()
        {
            var minions = ObjectManager.Get<AIBaseClient>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            var minionQ = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range) && e.IsMinion())
                         .Cast<AIBaseClient>().ToList();
            if (!Q.IsReady() || !Program.FarmMenu["useQLH"].GetValue<MenuBool>().Enabled) return;
            var useQ = FarmMenu["useQLH"].GetValue<MenuBool>().Enabled;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget() && _Player.Distance(minion) <= Q.Range)
                {
                    Q.Cast(minion.Position, EntityManager.MinionsAndMonsters.EnemyMinions.OrderByDescending(a => a.MaxHealth).Any(
                    a => a.Health <= Program.QDamage(a)));

                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range + 100, DamageType.Physical);

            if (target == null) return;

            if (E.IsReady() && ComboMenu["useECombo"].GetValue<MenuBool>().Enabled && target.Distance(_Player) <= Player.Instance.GetRealAutoAttackRange(target))
            {
                Program.E.Cast();
            }
            if (Q.IsReady() && Program.HarassMenu["useQHarass"].GetValue<MenuBool>().Enabled)
            {
                Q.Cast(target);
            }

            if (W.IsReady() && !Program.HasW && Program.HarassMenu["useWHarass"].GetValue<MenuBool>().Enabled)
            {
                W.Cast();
            }

        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Program.Q.Range + 100, DamageType.Physical);

            if (target == null) return;
            if (E.IsReady() && ComboMenu["useECombo"].GetValue<MenuBool>().Enabled && target.Distance(_Player) <= Player.Instance.GetRealAutoAttackRange(target))
            {
                Program.E.Cast();
            }

            if (Program.Q.IsReady() && Program.ComboMenu["useQCombo"].GetValue<MenuBool>().Enabled)
            {
                Program.Q.Cast(target);
            }
            if (Program.W.IsReady() && !Program.HasW && Program.ComboMenu["useWCombo"].GetValue<MenuBool>().Enabled)
            {
                Program.W.Cast();
            }
            if (Program.R.IsReady() && Program.ComboMenu["useRCombo"].GetValue<MenuBool>().Enabled && Player.Instance.HealthPercent <= Program.ComboMenu["useRComboHPPercent"].GetValue<MenuSlider>().Value && Player.Instance.CountEnemyHeroesInRange(1000) >= Program.ComboMenu["useRComboEnemies"].GetValue<MenuSlider>().Value)
            {
                Program.R.Cast();
            }
        }


        private static void AIBaseClient_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {

            if (!sender.IsMe) return;
            
            if (args.SData.Name == Player.GetSpell(SpellSlot.E).Name)
            {
                Console.WriteLine(Player.GetSpell(SpellSlot.E).Name);
                Orbwalker.ResetAutoAttackTimer();
                
                
            }
            if (args.SData.Name == Player.GetSpell(SpellSlot.W).Name && !Player.HasBuff("BurningAgony"))
            {
                ResetTime = Environment.TickCount + 500;
                
            }
        }



        private static void OnDraw(EventArgs args)
        {
            if (DrawMenu["drawQ"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Instance.Position, Q.Range , Color.LawnGreen);
            }
            if (DrawMenu["drawW"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Instance.Position, W.Range, Color.LimeGreen);
            }
            if (DrawMenu["drawE"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Instance.Position, E.Range, Color.LimeGreen);
            }

        }



        public static double QDamage(AIBaseClient target)
        {
            var level = Player.GetSpell(SpellSlot.Q).Level;
            if (level < 1) return 0;
            var value = new[]
            {
                (new[] {80, 130, 180, 230, 280}[level - 1]),
                (float) (new[] {0.15, 0.175, 0.21, 0.225, 0.25}[level - 1]*target.Health)
            }.Max();
            if (EntityManager.Heroes.Enemies.Any(a => a.NetworkId == target.NetworkId))
                return Player.Instance.CalculateDamage(target, DamageType.Magical, value);

            var maxMonsters = new[] { 300, 350, 400, 450, 500 }[level - 1];
            if (maxMonsters < value)
            {
                value = maxMonsters;
            }
            return Player.Instance.CalculateDamage(target, DamageType.Magical, value);
        }
    }
}
