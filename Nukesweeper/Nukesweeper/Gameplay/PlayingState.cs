using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using TeamStor.Engine;
using TeamStor.Engine.Graphics;

namespace TeamStor.Nukesweeper.Gameplay
{
    /// <summary>
    /// Game state when a game session is being played.
    /// </summary>
    public class PlayingState : GameState
    {
        /// <summary>
        /// Playing field.
        /// </summary>
        public NukeField Field { get; private set; }

        public PlayingState(NukeField field)
        {
            Field = field;
        }

        public override void OnEnter(GameState previousState)
        {

        }

        public override void OnLeave(GameState nextState)
        {

        }

        public override void Update(double deltaTime, double totalTime, long count)
        {

        }

        public override void FixedUpdate(long count)
        {

        }

        public override void Draw(SpriteBatch batch, Vector2 screenSize)
        {

        }
    }
}