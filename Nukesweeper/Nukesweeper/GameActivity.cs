using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content.PM;
using Microsoft.Xna.Framework;

using Game = TeamStor.Engine.Game;
using Android.Views;
using TeamStor.Nukesweeper.Menu;
using TeamStor.Nukesweeper.Gameplay;

namespace TeamStor.Nukesweeper
{
    // https://github.com/MonoGame/MonoGame.Samples/blob/develop/Platformer2D/Platforms/Android/Activity1.cs
    [Activity(
        Label = "Nukesweeper",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class GameActivity : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Game teamStorGame = new Game(new PlayingState(new NukeField(9, 9, 5)));
            SetContentView((View)teamStorGame.Services.GetService(typeof(View)));

            SystemUiFlags uiOptions = SystemUiFlags.HideNavigation |
                SystemUiFlags.LayoutHideNavigation |
                SystemUiFlags.Fullscreen |
                SystemUiFlags.LayoutStable |
                SystemUiFlags.LowProfile |
                SystemUiFlags.Immersive |
                SystemUiFlags.ImmersiveSticky;

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;

            teamStorGame.Stats |= Game.DebugStats.FPS;
            //teamStorGame.Stats |= Game.DebugStats.General;
            teamStorGame.Run();
        }
    }
}