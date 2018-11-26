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
using Microsoft.Xna.Framework.Graphics;
using TeamStor.Engine;
using TeamStor.Engine.Graphics;

using SpriteBatch = TeamStor.Engine.Graphics.SpriteBatch;

namespace TeamStor.Nukesweeper.Gameplay
{
    /// <summary>
    /// Game state when a game session is being played.
    /// </summary>
    public class PlayingState : GameState
    {
        private float _heldTime = 0;

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
            if(Input.Count > 0)
                _heldTime += (float)deltaTime;
            else if(Input.Count == 0 && LastInput.Count > 0)
            {
                Vector2 screenSize = Game.GraphicsDevice.Viewport.Bounds.Size.ToVector2() / (float)Game.Scale;

                int size = 60;
                // 20 on each side
                while(size * Field.Width > screenSize.X - 40)
                {
                    size -= 5;

                    if(size < 5)
                    {
                        size = 5;
                        break;
                    }
                }

                int width = size * Field.Width;
                int height = size * Field.Height;

                int x = (int)(screenSize.X / 2) - width / 2;
                int y = (int)(screenSize.Y / 2) - height / 2;

                for(int fx = 0; fx < Field.Width; fx++)
                {
                    for(int fy = 0; fy < Field.Height; fy++)
                    {
                        if(new Rectangle(x + fx * size, y + fy * size, size, size).Contains(LastInput[0].Position))
                        {
                            if(_heldTime > 0.1)
                            {

                                switch(Field.FlagAt(fx, fy))
                                {
                                    case FlagType.None:
                                        Field.SetFlagAt(fx, fy, FlagType.Flag);
                                        break;

                                    case FlagType.Flag:
                                        Field.SetFlagAt(fx, fy, FlagType.FlagVisual);
                                        break;

                                    case FlagType.FlagVisual:
                                        Field.SetFlagAt(fx, fy, FlagType.None);
                                        break;
                                }
                            }
                            else
                                Field.SetRevealed(fx, fy, true);
                        }
                    }
                }

                _heldTime = 0;
            }
        }

        public override void FixedUpdate(long count)
        {

        }

        public override void Draw(SpriteBatch batch, Vector2 screenSize)
        {
            int size = 60;
            // 20 on each side
            while(size * Field.Width > screenSize.X - 40)
            {
                size -= 5;

                if(size < 5)
                {
                    size = 5;
                    break;
                }
            }

            int width = size * Field.Width;
            int height = size * Field.Height;

            int x = (int)(screenSize.X / 2) - width / 2;
            int y = (int)(screenSize.Y / 2) - height / 2;

            for(int fx = 0; fx < Field.Width; fx++)
            {
                for(int fy = 0; fy < Field.Height; fy++)
                {
                    string tex = "empty";

                    if(Field.FlagAt(fx, fy) == FlagType.Flag)
                        tex = "flag";
                    if(Field.FlagAt(fx, fy) == FlagType.FlagVisual)
                        tex = "flagvisual";

                    if(Field.IsRevealed(fx, fy))
                        tex = "revealed";

                    if(Field[fx, fy])
                        tex = "nuke";

                    batch.Texture(
                        new Rectangle(x + fx * size, y + fy * size, size, size),
                        Assets.Get<Texture2D>("placeholder/" + tex + ".png"),
                        Color.White);
                }
            }
        }
    }
}