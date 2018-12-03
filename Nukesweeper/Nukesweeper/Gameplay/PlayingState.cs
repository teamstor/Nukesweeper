using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TeamStor.Engine;

using SpriteBatch = TeamStor.Engine.Graphics.SpriteBatch;

namespace TeamStor.Nukesweeper.Gameplay
{
    /// <summary>
    /// Game state when a game session is being played.
    /// </summary>
    public class PlayingState : GameState
    {
        public const int TILE_SIZE = 52;

        /// <summary>
        /// Playing field.
        /// </summary>
        public NukeField Field { get; private set; }

        /// <summary>
        /// Camera to move around the field.
        /// </summary>
        public Camera Camera { get; private set; }

        public PlayingState(NukeField field)
        {
            Field = field;
            Camera = new Camera(this);
        }

        public override void OnEnter(GameState previousState)
        {

        }

        public override void OnLeave(GameState nextState)
        {

        }

        public override void Update(double deltaTime, double totalTime, long count)
        {
            Camera.UpdatePosition(Input, LastInput);
        }

        public override void FixedUpdate(long count)
        {

        }

        public override void Draw(SpriteBatch batch, Vector2 screenSize)
        {
            batch.Transform = Camera.Transform;

            int x, y;
            for(x = 0; x < Field.Width; x++)
            {
                for(y = 0; y < Field.Height; y++)
                {
                    string texture = "empty.png";

                    if(Field.FlagAt(x, y) == FlagType.Flag)
                        texture = "flag.png";
                    if(Field.FlagAt(x, y) == FlagType.FlagVisual)
                        texture = "flagvisual.png";

                    if(Field.IsRevealed(x, y))
                        texture = "revealed.png";

                    // TODO: när man förlorar revealas alla
                    if(Field.IsRevealed(x, y) && Field[x, y])
                        texture = "nuke.png";

                    batch.Texture(new Rectangle(
                        x * (TILE_SIZE + 2), y * (TILE_SIZE + 2),
                        TILE_SIZE, TILE_SIZE),
                        Assets.Get<Texture2D>("placeholder/tiles/" + texture),
                        Color.White);
                }
            }

            batch.Transform = Matrix.Identity;

            batch.Rectangle(new Rectangle(0, 0, (int)screenSize.X, 180), Color.DarkGray);
            batch.Outline(new Rectangle(0, 0, (int)screenSize.X, 180), Color.White, 2, false);

            batch.SamplerState = SamplerState.PointClamp;
            batch.Texture(new Rectangle((int)screenSize.X / 2 - 64 / 2, 40, 64, 64), Assets.Get<Texture2D>("placeholder/reactions/idle.png"), Color.White);
            batch.SamplerState = SamplerState.LinearClamp;
        }
    }
} 