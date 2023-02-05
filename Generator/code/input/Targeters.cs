namespace Generator
{
    public static class Targeters
    {
        public static Targeter AbilityTargeter = new Targeter(
            new BoundAction(
                KeyBindings.A, 
                () => Globals.Player.Cast(
                    new AbilityInstance(
                        Selectors.AbilitySelector.GetSelection().Name,
                        Globals.Player,
                        AbilityTargeter.GetTarget()))),
            new BoundAction(
                KeyBindings.B, 
                () => GameControl.CurrentScreen = GameControl.GameScreen.AbilitySelector),
            target => new AbilityInstance(
                    Selectors.AbilitySelector.GetSelection().Name,
                    Globals.Player,
                    AbilityTargeter.GetTarget()
                ).GetTargetPositions()); 
        
        public static Targeter ItemTargeter = new Targeter(
            new BoundAction(
                KeyBindings.A, 
                () => Selectors.ItemSelector.GetSelection().Use(Globals.Player)),
            new BoundAction(
                KeyBindings.B, 
                () => GameControl.CurrentScreen = GameControl.GameScreen.ItemSelector));
        
        public static Targeter LookAroundTargeter = new Targeter(
            null,  // TODO: What happens when you select a dude? Let you select an action in range?
            new BoundAction(
                KeyBindings.B, 
                () => GameControl.CurrentScreen = GameControl.GameScreen.CombatOptionSelector));
    }
}