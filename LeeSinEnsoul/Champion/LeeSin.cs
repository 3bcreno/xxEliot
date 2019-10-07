using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using static MrLeeSin.Common;

namespace LeeSinEnsoul.Champion
{

    internal class LeeSin
    {
        // Default Setting
        public static int ErrorTime;
        public const string ChampName = "LeeSin";   // Edit
        public static AIHeroClient Player;
        public static Spell _Q, _W, _E, _R;
        public static MrLeeSin.HpBarIndicator Indicator = new MrLeeSin.HpBarIndicator();
        // Default Setting
        public static float QCastTime;
        private static Vector3 use_ward, ward_pos, use_minion, InsecST, InsecED, InsecPOS;
        private static float Ward_Time, WW_Time, QTime, WTime, ETime, InsecTime = 0;
        private static bool WW = true;
        private static string InsecText, InsecType = "Wait";

        private void SkillSet()
        {
            try
            {
                _Q = new Spell(SpellSlot.Q, 1200f);
                _Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);
                _W = new Spell(SpellSlot.W, 700f);
                _E = new Spell(SpellSlot.E, 425f);
                _R = new Spell(SpellSlot.R, 375f);
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Chat.Print(ChampName + " in MrLeeSin isn't Load. Error Code 01");
                    ErrorTime = TickCount(10000);
                }
            }
        }
        private void Menu()
        {
            try
            {
                var Combo = new Menu("Combo", "Combo");
                {
                    Combo.AddItem(new MenuItem("LeeSin_CUse_Q", "Use Q").SetValue(true));
                    Combo.AddItem(new MenuItem("LeeSin_CUse_W", "Use W").SetValue(true));
                    Combo.AddItem(new MenuItem("LeeSin_CUse_E", "Use E").SetValue(true));
                    Combo.AddItem(new MenuItem("LeeSin_CUse_R", "Use R").SetValue(true));
                    Combo.AddItem(new MenuItem("LeeSin_CUseQ_Hit", "Q HitChance").SetValue(new Slider(4, 1, 6)));
                    Combo.AddItem(new MenuItem("CKey", "Combo Key").SetValue(new KeyBind("32".ToArray()[0], KeyBindType.Press)));
                }
                _MainMenu.AddSubMenu(Combo);

                var Harass = new Menu("Harass", "Harass");
                {
                    Harass.AddItem(new MenuItem("LeeSin_HUse_Q", "Use Q").SetValue(true));
                    Harass.AddItem(new MenuItem("LeeSin_HUse_W", "Use W").SetValue(true));
                    Harass.AddItem(new MenuItem("LeeSin_HUse_E", "Use E").SetValue(true));
                    Harass.AddItem(new MenuItem("HKey", "Harass Key").SetValue(new KeyBind(_MainMenu.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
                }
                _MainMenu.AddSubMenu(Harass);

                var LaneClear = new Menu("LaneClear", "LaneClear");
                {
                    LaneClear.AddItem(new MenuItem("LeeSin_LUse_Q", "Use Q").SetValue(true));
                    LaneClear.AddItem(new MenuItem("LeeSin_LUse_W", "Use W").SetValue(true));
                    LaneClear.AddItem(new MenuItem("LeeSin_LUse_E", "Use E").SetValue(true));
                    LaneClear.AddItem(new MenuItem("LKey", "LaneClear Key").SetValue(new KeyBind("V".ToArray()[0], KeyBindType.Press)));
                }
                _MainMenu.AddSubMenu(LaneClear);

                var JungleClear = new Menu("JungleClear", "JungleClear");
                {
                    JungleClear.AddItem(new MenuItem("LeeSin_JUse_Q", "Use Q").SetValue(true));
                    JungleClear.AddItem(new MenuItem("LeeSin_JUse_W", "Use W").SetValue(true));
                    JungleClear.AddItem(new MenuItem("LeeSin_JUse_E", "Use E").SetValue(true));
                    JungleClear.AddItem(new MenuItem("JKey", "JungleClear Key").SetValue(new KeyBind("V".ToArray()[0], KeyBindType.Press)));
                }
                _MainMenu.AddSubMenu(JungleClear);

                var Misc = new Menu("Misc", "Misc");
                {
                    Misc.AddItem(new MenuItem("LeeSin_Ward_W", "W to Ward").SetValue(new KeyBind("A".ToArray()[0], KeyBindType.Press)));
                    Misc.AddItem(new MenuItem("LeeSin_InsecKick", "Insec Kick").SetValue(new KeyBind("G".ToArray()[0], KeyBindType.Press)));
                    Misc.AddItem(new MenuItem("LeeSin_KickAndFlash", "When use InsecKick, Do you use Flash + R?").SetValue(true));
                    Misc.AddItem(new MenuItem("LeeSin_KUse_R", "KillSteal, Use R").SetValue(true));
                    Misc.AddItem(new MenuItem("LeeSin_AutoKick", "When possible hit something, Use _R").SetValue(new Slider(3, 0, 5)));
                    Misc.AddItem(new MenuItem("LeeSin_AntiGab", "Anti-Gapcloser, If My hp < someone, Use _R").SetValue(new Slider(15, 0, 100)));
                }
                _MainMenu.AddSubMenu(Misc);

                var Draw = new Menu("Draw", "Draw");
                {
                    Draw.AddItem(new MenuItem("LeeSin_Draw_Q", "Draw Q").SetValue(false));
                    Draw.AddItem(new MenuItem("LeeSin_Draw_W", "Draw W").SetValue(false));
                    Draw.AddItem(new MenuItem("LeeSin_Draw_E", "Draw E").SetValue(false));
                    Draw.AddItem(new MenuItem("LeeSin_Draw_Ward", "Draw Ward").SetValue(true));
                    Draw.AddItem(new MenuItem("LeeSin_PredictR", "_R Prediction Location").SetValue(true));
                    Draw.AddItem(new MenuItem("LeeSin_Indicator", "Draw Damage Indicator").SetValue(true));
                }
                _MainMenu.AddSubMenu(Draw);
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Chat.Print(ChampName + " in MrLeeSin isn't Load. Error Code 02");
                    ErrorTime = TickCount(10000);
                }
            }
        }
        public static void Drawing_OnDraw(EventArgs args)
        {
            try
            {
                if (Player.IsDead) return;
                if (_MainMenu.Item("LeeSin_Draw_Q").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _Q.Range, Color.White, 1);
                if (_MainMenu.Item("LeeSin_Draw_W").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _W.Range, Color.White, 1);
                if (_MainMenu.Item("LeeSin_Draw_E").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, _E.Range, Color.White, 1);
                if (_MainMenu.Item("LeeSin_Draw_Ward").GetValue<bool>())
                    Render.Circle.DrawCircle(Player.Position, 625, Color.White, 1);
                //if (TargetSelector.GetSelectedTarget() != null) Render.Circle.DrawCircle(TargetSelector.GetSelectedTarget().Position, 375, Color.Green, 2);                
                if (_MainMenu.Item("LeeSin_InsecKick").GetValue<KeyBind>().Active && TargetSelector.SelectedTarget != null && _MainMenu.Item("LeeSin_PredictR").GetValue<bool>())
                {
                    var GetTarget = TargetSelector.SelectedTarget;
                    if (GetTarget == null || GetTarget.IsDead) return;
                    var Turrets = ObjectManager.Get<Obj_Turret>()
                    .OrderBy(obj => obj.Position.Distance(Player.Position))
                    .FirstOrDefault(obj => obj.IsAlly && !obj.IsDead);
                    var AllyChampion = ObjectManager.Get<AIHeroClient>().FirstOrDefault(obj => obj.IsAlly && !obj.IsMe && !obj.IsDead && obj.Distance(Player.Position) < 2000);
                    if (Turrets == null && AllyChampion == null) return;
                    if (AllyChampion != null)
                    { var InsecPOS = InsecST.Extend(InsecED, +InsecED.Distance(InsecST) + 230); }
                    Render.Circle.DrawCircle(InsecPOS, 50, Color.Gold);
                    if (GetTarget.Distance(Player.Position) < 625)
                    {
                        Render.Circle.DrawCircle(Player.Position, 525, Color.LightGreen);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(Player.Position, 525, Color.IndianRed);
                    }
                    Drawing.DrawLine(Drawing.WorldToScreen(InsecST)[0], Drawing.WorldToScreen(InsecST)[1], Drawing.WorldToScreen(InsecED)[0], Drawing.WorldToScreen(InsecED)[1], 2, Color.Green);
                }
                if (_Q.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQTwo")
                {
                    var target = ObjectManager.Get<AIHeroClient>().FirstOrDefault(f => f.IsEnemy && !f.IsZombie && f.Distance(Player.Position) <= _Q.Range && f.HasBuff("BlindMonkQOne"));
                    if (target != null)
                        Render.Circle.DrawCircle(target.Position, 175, Color.YellowGreen);
                }
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Chat.Print(ChampName + " in MrLeeSin isn't Load. Error Code 03");
                    ErrorTime = TickCount(10000);
                }
            }
        }
        public LeeSin()
        {
            Player = ObjectManager.Player;
            SkillSet();
            Menu();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Gapclosers.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            AIBaseClient.OnProcessSpellCast += OnProcessSpell;
            Interrupters.OnInterrupter += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            EnsoulSharp.Player.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            LeeSinEnsoul.Champion.DamageIndicator.DamageToUnit = ComboDamage;
        }

        private float ComboDamage(AIHeroClient enemy)
        {
            var damage = 0d;

            if (QStage == QCastStage.IsReady)
            {
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) * 2;
            }

            if (EStage == ECastStage.IsReady)
            {
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (_R.IsReady())
            {
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float)damage;
        }

        internal enum QCastStage { NotReady, IsReady, IsCasted }
        internal enum WCastStage { NotReady, IsReady, IsCasted }
        internal enum ECastStage { NotReady, IsReady, IsCasted }
        public static QCastStage QStage
        {
            get
            {
                if (!_Q.IsReady()) { return QCastStage.NotReady; }
                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne" ? QCastStage.IsReady : QCastStage.IsCasted);
            }
        }

        public static WCastStage WStage
        {
            get
            {
                if (!_W.IsReady()) { return WCastStage.NotReady; }
                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "BlindMonkWTwo" ? WCastStage.IsCasted : WCastStage.IsReady);
            }
        }

        public static ECastStage EStage
        {
            get
            {
                if (!_E.IsReady()) { return ECastStage.NotReady; }
                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "BlindMonkEOne" ? ECastStage.IsReady : ECastStage.IsCasted);
            }
        }

        static float getComboDamage(AIBaseClient enemy)
        {
            try
            {
                if (enemy != null)
                {
                    float damage = 0;

                    if (_Q.IsReady())
                        damage += _Q.GetDamage(enemy);
                    if (_W.IsReady())
                        damage += _W.GetDamage(enemy);
                    if (_E.IsReady())
                        damage += _E.GetDamage(enemy);
                    if (_R.IsReady())
                        damage += _R.GetDamage(enemy);

                    if (!Player.Spellbook.IsAutoAttack)
                        damage += (float)Player.GetAutoAttackDamage(enemy, true);

                    return damage;
                }
                return 0;
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Chat.Print(ChampName + " in MrLeeSin isn't Load. Error Code 04");
                    ErrorTime = TickCount(10000);
                }
                return 0;
            }
        }
        public static void Drawing_OnEndScene(EventArgs args)
        {
            try
            {
                if (Player.IsDead) return;
                foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(ene => ene.IsValidTarget() && !ene.IsZombie))
                {
                    if (_MainMenu.Item("LeeSin_Indicator").GetValue<bool>())
                    {
                        Indicator.unit = enemy;
                        Indicator.drawDmg(getComboDamage(enemy), new ColorBGRA(255, 204, 0, 160));
                    }
                }
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Chat.Print(ChampName + " in MrLeeSin isn't Load. Error Code 05");
                    ErrorTime = TickCount(10000);
                }
            }
        }


        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
        }

        private void Obj_AI_Base_OnIssueOrder(AIBaseClient sender, PlayerIssueOrderEventArgs args)
        {
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        { }

        private void Interrupter2_OnInterruptableTarget(ActiveInterrupter interrupter)
        {
        }

        private static void OnProcessSpell(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            foreach (var Me in HeroManager.Allies)
            {
                if (Me.CharacterName == Player.CharacterName)
                {
                    //if (MainMenu._MainMenu.Item("KickAndFlash").GetValue<KeyBind>().Active && args.SData.Name == "BlindMonkRKick")                        
                    //if (MainMenu._MainMenu.Item("KickAndFlash").GetValue<KeyBind>().Active && args.SData.Name == "summonerflash") RF = true;
                    if (_MainMenu.Item("LeeSin_InsecKick").GetValue<KeyBind>().Active && args.SData.Name == "BlindMonkRKick" && InsecType == "RF" && InsecType != "WF" && InsecType != "WF1")
                    {
                        var Flash = Player.GetSpellSlot("SummonerFlash");
                        Player.Spellbook.CastSpell(Flash, InsecPOS, true);
                    }
                    //if (MainMenu._MainMenu.Item("InsecKick").GetValue<KeyBind>().Active && args.SData.Name == "blindmonkwtwo")
                }

            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_MainMenu.Item("LeeSin_AntiGab").GetValue<Slider>().Value == 0 || !_R.IsReady()) return;
            var Anti = _MainMenu.Item("LeeSin_AntiGab").GetValue<Slider>().Value;
            var Myhp = Player.HealthPercent;
            var Target = gapcloser.Sender;
            if (Target != null && Player.Distance(Target) < 375 && Myhp < Anti && _R.Level > 0 && _R.IsReady()) _R.Cast(Target, true);
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            var KTarget = ObjectManager.Get<AIHeroClient>().OrderByDescending(x => x.Health).FirstOrDefault(x => x.IsEnemy && x.Distance(Player) < 375);
            if (KTarget != null && _MainMenu.Item("LeeSin_KUse_R").GetValue<bool>() && KTarget.Health < _R.GetDamage(KTarget) && _R.IsReady())
                _R.Cast(KTarget, true);
            if (InsecTime < Environment.TickCount) InsecType = "Wait"; // 인섹킥 초기화
            if (Ward_Time < Environment.TickCount) WW = true;   // 와드방호 초기화
            if (_MainMenu.Item("LeeSin_AutoKick").GetValue<Slider>().Value != 0 && _R.Level > 0 && _R.IsReady() && !_MainMenu.Item("LeeSin_InsecKick").GetValue<KeyBind>().Active)
            {
                AutoKick();
            }
            if (_MainMenu.Item("CKey").GetValue<KeyBind>().Active) // Combo
            {
                Combo();
                var QTarget = TargetSelector.GetTarget(_Q.Range, TargetSelector.DamageType.Physical);
                var ETarget = TargetSelector.GetTarget(_E.Range, TargetSelector.DamageType.Physical);
             
                if (ETarget != null && _E.IsReady() && !Orbwalking.CanAttack() && Orbwalking.CanMove(10) && ETime < Environment.TickCount && _MainMenu.Item("LeeSin_CUse_E").GetValue<bool>())
                {
                    _E.Cast(true);
                    ETime = TickCount(1000);
                }
                if (!_Q.IsReady() && !_E.IsReady() && !Orbwalking.CanAttack() && Orbwalking.CanMove(10) && WTime < Environment.TickCount && _MainMenu.Item("LeeSin_CUse_W").GetValue<bool>())
                {
                    _W.Cast(Player, true);
                    WTime = TickCount(1000);
                }
            }
            if (_MainMenu.Item("HKey").GetValue<KeyBind>().Active) // Hafass
            {
                var QTarget = TargetSelector.GetTarget(_Q.Range, TargetSelector.DamageType.Physical);
                var ETarget = TargetSelector.GetTarget(_E.Range, TargetSelector.DamageType.Physical);
                if (QTarget != null && _Q.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne" && _MainMenu.Item("LeeSin_HUse_Q").GetValue<bool>())
                {
                    var HC = HitChance.Medium;
                    _Q.CastIfHitchanceEquals(QTarget, HC, true);
                    QTime = TickCount(2000);
                }
                if (ETarget != null && _E.IsReady() && !Orbwalking.CanAttack() && Orbwalking.CanMove(10) && ETime < Environment.TickCount && _MainMenu.Item("LeeSin_HUse_E").GetValue<bool>())
                {
                    _E.Cast(true);
                    ETime = TickCount(1000);
                }
                if (!_Q.IsReady() && !_E.IsReady() && !Orbwalking.CanAttack() && Orbwalking.CanMove(10) && WTime < Environment.TickCount && _MainMenu.Item("LeeSin_HUse_W").GetValue<bool>())
                {
                    _W.Cast(Player, true);
                    WTime = TickCount(1000);
                }
            }
            if (_MainMenu.Item("LKey").GetValue<KeyBind>().Active) // LaneClear
            {
                var MinionTarget = MinionManager.GetMinions(1100, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                foreach (var minion in MinionTarget)
                {
                    if (_Q.IsReady() && _MainMenu.Item("LeeSin_LUse_Q").GetValue<bool>() && minion != null && Environment.TickCount > QTime)
                    {
                        _Q.CastIfHitchanceEquals(minion, HitChance.Medium, true);
                        QTime = TickCount(1000);
                    }
                    if (_E.IsReady() && _MainMenu.Item("LeeSin_LUse_E").GetValue<bool>() && minion != null && Environment.TickCount > ETime
                        && !Orbwalking.CanAttack() && Orbwalking.CanMove(10))
                    {
                        _E.Cast(true);
                        ETime = TickCount(1000);
                    }
                    if (_W.IsReady() && _MainMenu.Item("LeeSin_LUse_W").GetValue<bool>() && minion != null && Environment.TickCount > WTime
                        && !Orbwalking.CanAttack() && Orbwalking.CanMove(10))
                    {
                        _W.Cast(Player, true);
                        WTime = TickCount(1000);
                    }
                }
            }
            if (_MainMenu.Item("JKey").GetValue<KeyBind>().Active) // JungleClear
            {
                var JungleTarget = MinionManager.GetMinions(1100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                foreach (var minion in JungleTarget)
                {
                    if (_Q.IsReady() && _MainMenu.Item("LeeSin_JUse_Q").GetValue<bool>() && minion != null && Environment.TickCount > QTime)
                    {
                        _Q.CastIfHitchanceEquals(minion, HitChance.Medium, true);
                        QTime = TickCount(1500);
                    }
                    if (_E.IsReady() && _MainMenu.Item("LeeSin_JUse_E").GetValue<bool>() && minion != null && Environment.TickCount > ETime
                        && !Orbwalking.CanAttack() && Orbwalking.CanMove(10))
                    {
                        _E.Cast(true);
                        ETime = TickCount(1500);
                    }
                    if (_W.IsReady() && _MainMenu.Item("LeeSin_JUse_W").GetValue<bool>() && minion != null && Environment.TickCount > WTime
                        && !Orbwalking.CanAttack() && Orbwalking.CanMove(10))
                    {
                        _W.Cast(Player, true);
                        WTime = TickCount(1500);
                    }
                }
            }
            if (_MainMenu.Item("LeeSin_InsecKick").GetValue<KeyBind>().Active)    // 인섹킥
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosCenter);
                var GetTarget = TargetSelector.SelectedTarget;
                if (GetTarget == null || GetTarget.IsDead) return;
                if (_Q.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne" && _Q.GetPrediction(GetTarget).Hitchance >= HitChance.Medium)
                    _Q.CastOnUnit(GetTarget, true);
                var Turrets = ObjectManager.Get<Obj_Turret>()
                .OrderBy(obj => obj.Position.Distance(Player.Position))
                .FirstOrDefault(obj => obj.IsAlly && obj.Health > 1);
                var AllyChampion = ObjectManager.Get<AIHeroClient>().FirstOrDefault(obj => obj.IsAlly && !obj.IsMe && !obj.IsDead && obj.Distance(Player.Position) < 2000);
                if (Turrets == null && AllyChampion == null) return;
                if (AllyChampion != null)
                {
                    InsecST = AllyChampion.Position;
                }
                else
                {
                    InsecST = Turrets.Position;
                }
                InsecED = GetTarget.Position;
                InsecPOS = InsecST.Extend(InsecED, +InsecED.Distance(InsecST) + 230);
                MovingPlayer(InsecPOS);
                if (!_R.IsReady())
                    return;

                if (_MainMenu.Item("LeeSin_KickAndFlash").GetValue<bool>() && InsecPOS.Distance(Player.Position) < 425
                    && GetTarget.Distance(Player.Position) < 375 && InsecType == "Wait" && _R.Level > 0 && _R.IsReady() &&
                    InsecType != "WF" && InsecType != "WF1" && Player.GetSpellSlot("SummonerFlash").IsReady())
                {
                    InsecTime = TickCount(2000);
                    InsecText = "Flash";
                    InsecType = "RF";
                    _R.Cast(GetTarget, true);
                    return;
                }
                if (InsecPOS.Distance(Player.Position) < 625 && _R.Level > 0 && _R.IsReady() && InsecType != "RF")
                {
                    InsecText = "Ward";
                    if (InsecType == "Wait" && InsecType != "WF" && InsecType != "WF1" && _W.IsReady())
                    {
                        if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "blindmonkwtwo") return;
                        InsecTime = TickCount(2000);
                        InsecType = "WF";
                        var Ward = Items.GetWardSlot();
                        Player.Spellbook.CastSpell(Ward.SpellSlot, InsecPOS);
                    }
                    if (InsecType == "WF" && _W.IsReady())
                    {
                        if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "blindmonkwtwo") return;
                        var WardObj = ObjectManager.Get<AIBaseClient>()  // 커서근처 와드 유무
                                .OrderBy(obj => obj.Distance(InsecPOS))
                                .FirstOrDefault(obj => obj.IsAlly && !obj.IsMe
                                && obj.Distance(InsecPOS) <= 110 && obj.Name.ToLower().Contains("ward"));
                        if (WardObj != null)
                        {
                            InsecType = "WF1";
                            _W.Cast(WardObj, true);
                        }
                    }
                    if (InsecType == "WF1")
                    {
                        if (GetTarget.Distance(Player.Position) < 375)
                        {
                            _R.Cast(GetTarget, true);
                        }
                        else
                        {
                            EnsoulSharp.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosCenter);
                        }
                    }
                    return;
                }

                // 플 425, 와드 625
            }
            if (_MainMenu.Item("LeeSin_Ward_W").GetValue<KeyBind>().Active)   // 와드 방호
            {
                //와드방호는 WW로 정의
                var Cursor = Game.CursorPosCenter;
                var Ward = Items.GetWardSlot();
                EnsoulSharp.Player.IssueOrder(GameObjectOrder.MoveTo, Cursor);
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "BlindMonkWTwo") return;
                if (Player.Distance(Cursor) > 700) Cursor = Game.CursorPosCenter.Extend(Player.Position, +Player.Distance(Game.CursorPosCenter) - 700);
                //Render.Circle.DrawCircle(Cursor, 50, Color.Black, 2);
                //Drawing.DrawText(200, 200, Color.White, "WW is: " + WW.ToString());                    
                if (_W.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name.ToLower() == "BlindMonkWOne")
                {
                    var Object = ObjectManager.Get<AIHeroClient>().FirstOrDefault(obj => obj.IsAlly && !obj.IsMe && obj.Distance(Cursor) < 110); // 커서근처 챔프유무
                    var Minion = MinionManager.GetMinions(Cursor, 110, MinionTypes.All, MinionTeam.Ally); // 아군 미니언 유무
                    var WardObj = ObjectManager.Get<AIBaseClient>()  // 커서근처 와드 유무
                            .OrderBy(obj => obj.Distance(Cursor))
                            .FirstOrDefault(obj => obj.IsAlly && !obj.IsMe
                            && obj.Distance(Cursor) <= 110 && obj.Name.ToLower().Contains("ward"));
                    if (WardObj != null && WTime < Environment.TickCount)
                    {
                        _W.Cast(WardObj, true);
                        Ward_Time = TickCount(2000);
                        WW = true;
                        WTime = TickCount(2000);
                        return;
                    }
                    if (Object != null && WTime < Environment.TickCount)
                    {
                        _W.Cast(Object, true);
                        Ward_Time = TickCount(2000);
                        WW = true;
                        WTime = TickCount(2000);
                        return;
                    }
                    if (Minion != null && WTime < Environment.TickCount)
                    {
                        foreach (var minion in Minion)
                        {
                            if (minion != null)
                            {
                                _W.Cast(minion, true);
                                Ward_Time = TickCount(2000);
                                WW = true;
                                WTime = TickCount(2000);
                                return;
                            }
                        }
                    }
                    if (Player.Distance(Cursor) > 625) Cursor = Game.CursorPosCenter.Extend(Player.Position, +Player.Distance(Game.CursorPosCenter) - 625);
                    //Render.Circle.DrawCircle(Cursor, 50, Color.Black, 2);                            
                    if (WW && Ward != null && Ward_Time < Environment.TickCount)
                    {
                        Player.Spellbook.CastSpell(Ward.SpellSlot, Cursor);
                        WW = false;
                        Ward_Time = TickCount(2000);
                    }
                    WardObj = ObjectManager.Get<AIBaseClient>()  // 커서근처 와드 유무
                            .OrderBy(obj => obj.Distance(Cursor))
                            .FirstOrDefault(obj => obj.IsAlly && !obj.IsMe
                            && obj.Distance(Cursor) <= 110 && obj.Name.ToLower().Contains("ward"));
                    if (WardObj != null && WTime < Environment.TickCount)
                    {
                        _W.Cast(WardObj, true);
                        Ward_Time = TickCount(2000);
                        WW = true;
                        WTime = TickCount(2000);
                        return;
                    }
                }
            }
        }

        private void Combo()
        {
            var tar = TargetSelector.GetTarget(_Q.Range, TargetSelector.DamageType.Physical);
            if (!tar.IsValidTarget())
            {
                return;
            }
            if (tar.HasBuff("BlindMonkQOne") || tar.HasBuff("BlindMonkQTwo") && !tar.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65) && QStage == QCastStage.IsCasted)
            {
                _Q.Cast();
            }
            if (tar.IsValidTarget() && QStage == QCastStage.IsReady && tar.IsValidTarget(_Q.Range))
            {
                _Q.Cast(tar);
            }
            QTime = Environment.TickCount;
            if (tar.HasBuff("BlindMonkQOne") || tar.HasBuff("BlindMonkQTwo") && (ComboDamage(tar) > tar.Health || Environment.TickCount > QCastTime + 2500))
                _Q.Cast();
            var HC = HitChance.Medium;
            switch (_MainMenu.Item("LeeSin_CUseQ_Hit").GetValue<Slider>().Value)
            {
                case 1:
                    HC = HitChance.OutOfRange;
                    break;
                case 2:
                    HC = HitChance.Impossible;
                    break;
                case 3:
                    HC = HitChance.Low;
                    break;
                case 4:
                    HC = HitChance.Medium;
                    break;
                case 5:
                    HC = HitChance.High;
                    break;
                case 6:
                    HC = HitChance.VeryHigh;
                    break;

            }
            _Q.CastIfHitchanceEquals(tar, HC, true);
            
        }

        private static void AutoKick()
        {
            if (_MainMenu.Item("LeeSin_AutoKick").GetValue<Slider>().Value == 0 || _MainMenu.Item("LeeSin_InsecKick").GetValue<KeyBind>().Active) return;

            var target =
                HeroManager.Enemies.Where(x => x.Distance(Player) < 375 && !x.IsDead && x.IsValidTarget(375))
                    .OrderBy(x => x.Distance(Player)).FirstOrDefault();
            if (target == null) return;

            var ultPoly = new Geometry.Polygon.Rectangle(Player.Position,
                Player.Position.Extend(target.Position, 1100),
                target.BoundingRadius + 10);

            var count =
                HeroManager.Enemies.Where(x => x.Distance(Player) < 1100 && x.IsValidTarget(1100))
                    .Count(h => h.NetworkId != target.NetworkId && ultPoly.IsInside(h.Position));

            if (count >= _MainMenu.Item("LeeSin_AutoKick").GetValue<Slider>().Value && _R.IsReady())
            {
                _R.Cast(target);
            }

        }

    }
}