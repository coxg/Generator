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
        
        public static Selector<GameControl.GameScreen> CombatScreenSelector = new Selector<GameControl.GameScreen>(
            new List<GameControl.GameScreen>
            {
                GameControl.GameScreen.AbilitySelector,
                GameControl.GameScreen.ItemSelector,
                GameControl.GameScreen.CombatMovement,
                GameControl.GameScreen.CombatLookAround
            }, 
            KeyBindings.Up,
            KeyBindings.Down, 
            activationAction: new BoundAction(
                KeyBindings.A, 
                () => GameControl.CurrentScreen = CombatScreenSelector.GetSelection()),
            cancelAction: new BoundAction(
                KeyBindings.B, 
                () => GameControl.CurrentScreen = GameControl.GameScreen.CombatLookAround));

        public static Selector<Ability> AbilitySelector = new Selector<Ability>(
            Globals.Player.Abilities,
            KeyBindings.Up,
            KeyBindings.Down,
            activationAction: new BoundAction(
                KeyBindings.A,
                () => GameControl.CurrentScreen = GameControl.GameScreen.AbilityTargeter),
            cancelAction: new BoundAction(
                KeyBindings.B, 
                () => GameControl.CurrentScreen = GameControl.GameScreen.CombatOptionSelector));
                
        public static Selector<Item> ItemSelector = new Selector<Item>(
            Globals.Party.Value.Inventory,
            KeyBindings.Up,
            KeyBindings.Down,
            activationAction: new BoundAction(
                KeyBindings.A, 
                () => ItemSelector.GetSelection().Use(Globals.Player)),
            cancelAction: new BoundAction(
                KeyBindings.B, 
                () => GameControl.CurrentScreen = GameControl.GameScreen.CombatOptionSelector));
        
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