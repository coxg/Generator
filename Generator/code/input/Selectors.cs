using System.Collections.Generic;
using System.Linq;

namespace Generator
{
    public static class Selectors
    {
        public static Selector<int> ZoomSelector = new Selector<int>(
            new List<int> { 5, 10, 25, 50, 100, 200, 500 }, // If I update this I also need to update the default
            KeyBindings.LB,
            KeyBindings.RB,
            () => GameControl.camera.Height = ZoomSelector.GetSelection(),
            startIndex: 2);
        
        public static Selector<CombatManager.CombatScreen> CombatScreenSelector = new Selector<CombatManager.CombatScreen>(
            new List<CombatManager.CombatScreen>
            {
                CombatManager.CombatScreen.AbilitySelectionScreen,
                CombatManager.CombatScreen.ItemSelectionScreen,
                CombatManager.CombatScreen.MovementScreen,
                CombatManager.CombatScreen.LookAroundScreen
            }, 
            KeyBindings.Down, 
            KeyBindings.Up,
            activationAction: new BoundAction(
                KeyBindings.A, 
                () => CombatManager.SelectedScreen = CombatScreenSelector.GetSelection()),
            cancelAction: new BoundAction(
                KeyBindings.B, 
                () => CombatManager.SelectedScreen = CombatManager.CombatScreen.LookAroundScreen));

        public static Selector<Ability> AbilitySelector = new Selector<Ability>(
            Globals.Player.Abilities,
            KeyBindings.Down,
            KeyBindings.Up,
            activationAction: new BoundAction(
                KeyBindings.A,
                () => CombatManager.SelectedScreen = CombatManager.CombatScreen.AbilityTargetingScreen),
            cancelAction: new BoundAction(
                KeyBindings.B, 
                () => CombatManager.SelectedScreen = CombatManager.CombatScreen.SelectionScreen));
                
        public static Selector<Item> ItemSelector = new Selector<Item>(
            Globals.Party.Value.Inventory,
            KeyBindings.Down,
            KeyBindings.Up,
            activationAction: new BoundAction(
                KeyBindings.A, 
                () => ItemSelector.GetSelection().Use(Globals.Player)),
            cancelAction: new BoundAction(
                KeyBindings.B, 
                () => CombatManager.SelectedScreen = CombatManager.CombatScreen.SelectionScreen));
        
        public static Selector<Tile> CreativeTileSelector = new Selector<Tile>(
            Globals.TileManager.TileSheet.Tiles,
            KeyBindings.Left,
            KeyBindings.Right);
        
        public static Selector<GameObject> PlayerSelector = new Selector<GameObject>(
            Globals.Party.Value.GetMembers().ToList(),
            KeyBindings.Left,
            KeyBindings.Right,
            () => Globals.Party.Value.LeaderID = PlayerSelector.GetSelection().ID);
    }
}