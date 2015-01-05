﻿using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace PrinceTalon
{
    internal class AssassinManager
    {
        public AssassinManager()
        {
            Load();
        }

        private static void Load()
        {
            MenuHandler.TalonConfig.AddSubMenu(new Menu("Assassin Manager", "AssassinManager"));
            MenuHandler.TalonConfig.SubMenu("AssassinManager")
                .AddItem(new MenuItem("AssassinActive", "Assassin Active").SetValue(true));
            MenuHandler.TalonConfig.SubMenu("AssassinManager")
                .AddItem(new MenuItem("AssassinSetClick", "Use Click Add/Remove").SetValue(true));
            MenuHandler.TalonConfig.SubMenu("AssassinManager")
                .AddItem(
                    new MenuItem("AssassinRangeColor", "Assassin Range Color").SetValue(new Circle(true,
                        Color.GreenYellow)));
            MenuHandler.TalonConfig.SubMenu("AssassinManager")
                .AddItem(
                    new MenuItem("AssassinInRangeColor", "Range Enemy Color").SetValue(new Circle(true,
                        Color.GreenYellow)));
            MenuHandler.TalonConfig.SubMenu("AssassinManager")
                .AddItem(
                    new MenuItem("AssassinInCloseColor", "Nearest Enemy Color").SetValue(new Circle(true,
                        Color.DarkSeaGreen)));
            MenuHandler.TalonConfig.SubMenu("AssassinManager")
                .AddItem(
                    new MenuItem("AssassinReset", "Reset Assassin List").SetValue(new KeyBind("J".ToCharArray()[0],
                        KeyBindType.Press)));

            MenuHandler.TalonConfig.SubMenu("AssassinManager").AddSubMenu(new Menu("Assassin 1st :", "AssassinMode"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                MenuHandler.TalonConfig.SubMenu("AssassinManager")
                    .SubMenu("AssassinMode")
                    .AddItem(
                        new MenuItem("Assassin" + enemy.ChampionName, enemy.ChampionName).SetValue(
                            TargetSelector.GetPriority(enemy) > 3));
            }
            MenuHandler.TalonConfig.SubMenu("AssassinManager")
                .AddItem(new MenuItem("AssassinRange", "Assassin Range")).SetValue(new Slider(1000, 2000));

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        static void ClearAssassinList()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                MenuHandler.TalonConfig.Item("Assassin" + enemy.ChampionName).SetValue(false);
            } 
        }
        private static void OnGameUpdate(EventArgs args)
        {
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {

            if (MenuHandler.TalonConfig.Item("AssassinReset").GetValue<KeyBind>().Active && args.Msg == 257)
            {
                ClearAssassinList();
                Game.PrintChat(
                    "<font color='#FFFFFF'>Reset Assassin List is Complete! Click on the enemy for Add/Remove.</font>");
            }

            if (args.Msg != 0x201)
            {
                return;
            }

            if (MenuHandler.TalonConfig.Item("AssassinSetClick").GetValue<bool>())
            {
                foreach (var objAiHero in from hero in ObjectManager.Get<Obj_AI_Hero>()
                                          where hero.IsValidTarget()
                                          select hero
                                              into h
                                              orderby h.Distance(Game.CursorPos) descending
                                              select h
                                                  into enemy
                                                  where enemy.Distance(Game.CursorPos) < 150f
                                                  select enemy)
                {
                    if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
                    {
                        ClearAssassinList();
                        MenuHandler.TalonConfig.Item("Assassin" + objAiHero.ChampionName).SetValue(true);
                        Game.PrintChat(
                            string.Format(
                                "<font color='FFFFFF'>Added to Assassin List</font> <font color='#09F000'>{0} ({1})</font>",
                                objAiHero.Name, objAiHero.ChampionName));

                        /*
                        var menuStatus = MenuHandler.TalonConfig.Item("Assassin" + objAiHero.ChampionName).GetValue<bool>();
                        MenuHandler.TalonConfig.Item("Assassin" + objAiHero.ChampionName).SetValue(!menuStatus);
                        Game.PrintChat(string.Format("<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
                                            !menuStatus ? "#FFFFFF" : "#FF8877",
                                            !menuStatus ? "Added to Assassin List:" : "Removed from Assassin List:",
                                            objAiHero.Name, objAiHero.ChampionName));
                         * */
                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!MenuHandler.TalonConfig.Item("AssassinActive").GetValue<bool>())
                return;

            var drawRangeColor = MenuHandler.TalonConfig.Item("AssassinRangeColor").GetValue<Circle>();
            var drawRangeEnemyColor = MenuHandler.TalonConfig.Item("AssassinInRangeColor").GetValue<Circle>();
            var drawNearestEnemyColor = MenuHandler.TalonConfig.Item("AssassinInCloseColor").GetValue<Circle>();

            var assassinRange = MenuHandler.TalonConfig.Item("AssassinRange").GetValue<Slider>().Value;
            if (drawRangeColor.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, assassinRange, drawRangeColor.Color, 1, 15);
            }

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>()
                .Where(enemy => enemy.Team != ObjectManager.Player.Team)
                .Where(enemy => enemy.IsVisible && MenuHandler.TalonConfig.Item("Assassin" + enemy.ChampionName) != null && !enemy.IsDead)
                .Where(enemy => MenuHandler.TalonConfig.Item("Assassin" + enemy.ChampionName).GetValue<bool>()))
            {
                if (ObjectManager.Player.Distance(enemy) < assassinRange)
                {
                    if (drawRangeEnemyColor.Active)
                        Utility.DrawCircle(enemy.Position, 85f, drawRangeEnemyColor.Color, 1, 15);
                }
                else if (ObjectManager.Player.Distance(enemy) > assassinRange && ObjectManager.Player.Distance(enemy) < assassinRange + 400)
                {
                    if (drawNearestEnemyColor.Active)
                        Utility.DrawCircle(enemy.Position, 85f, drawNearestEnemyColor.Color, 1, 15);
                }
            }
        }
    }
}