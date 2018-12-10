using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using TeamStor.Engine;
using TeamStor.Engine.Coroutine;
using static TeamStor.Engine.Graphics.SpriteBatch;
using SpriteBatch = TeamStor.Engine.Graphics.SpriteBatch;

namespace TeamStor.Nukesweeper.Gameplay
{
    /// <summary>
    /// Game state when a game session is being played.
    /// </summary>
    public class PlayingState : GameState
    {
        private enum FaceReaction
        {
            Idle, 
            Hover,
            Lose,
            Win
        }

        private FaceReaction _currentFace = FaceReaction.Idle;
        private bool _canInteract = true;

        public const int TILE_SIZE = 52;

        /// <summary>
        /// Playing field.
        /// </summary>
        public NukeField Field { get; private set; }

        /// <summary>
        /// Camera to move around the field.
        /// </summary>
        public Camera Camera { get; private set; }

        /// <summary>
        /// Point where the menu is being shown.
        /// </summary>
        public Point MenuTile = new Point(-1, -1);

        public PlayingState(NukeField field)
        {
            Field = field;
            Camera = new Camera(this);
        }

        public override void OnEnter(GameState previousState)
        {
            // preload assets
            Assets.Get<Texture2D>("placeholder/reactions/hover.png");
            Assets.Get<Texture2D>("placeholder/reactions/idle.png");
            Assets.Get<Texture2D>("placeholder/reactions/lose.png");
            Assets.Get<Texture2D>("placeholder/reactions/win.png");

            Assets.Get<Texture2D>("placeholder/tiles/empty.png");
            Assets.Get<Texture2D>("placeholder/tiles/flag.png");
            Assets.Get<Texture2D>("placeholder/tiles/flagvisual.png");
            Assets.Get<Texture2D>("placeholder/tiles/nuke.png");
            Assets.Get<Texture2D>("placeholder/tiles/revealed.png");

            Assets.Get<Texture2D>("icons/explode.png");
            Assets.Get<Texture2D>("icons/flag.png");
            Assets.Get<Texture2D>("icons/flagvisual.png");
        }

        public override void OnLeave(GameState nextState)
        {

        }

        private IEnumerator<ICoroutineOperation> OnExplodePressed()
        {
            _currentFace = FaceReaction.Hover;
            yield return Wait.Seconds(Game, 0.3);
            _currentFace = FaceReaction.Idle;
        }

        private IEnumerator<ICoroutineOperation> RevealAndAnimate(int x, int y, bool updateInteract = true)
        {
            if(updateInteract)
                _canInteract = false;

            bool wasRevealed = Field.IsRevealed(x, y);
            Field.SetRevealed(x, y, true);

            if(!wasRevealed && Field.SurroundingNukesAt(x, y) == 0)
            {
                for(int rX = x - 1; rX <= x + 1; rX++)
                {
                    for(int rY = y - 1; rY <= y + 1; rY++)
                    {
                        if(!(rX == x && rY == y) &&
                            rX >= 0 &&
                            rY >= 0 &&
                            rX < Field.Width &&
                            rY < Field.Height &&
                            !Field[rX, rY] &&
                            !Field.IsRevealed(rX, rY))
                        {
                            Coroutine.AddExisting(RevealAndAnimate(rX, rY, false));
                            yield return Wait.Seconds(Game, 0.03);
                        }
                    }
                }
            }

            if(updateInteract)
                _canInteract = true;
        }

        public override void Update(double deltaTime, double totalTime, long count)
        {
            Camera.UpdatePosition(Input, LastInput);

            Vector2 viewport = new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height) / (float)Game.Scale;

            if(_canInteract && !Camera.IsMoving && !Camera.JustDidPan && !Camera.JustDidZoom && LastInput.Count > 0 && Input.Count == 0)
            {
                foreach(TouchLocation location in LastInput)
                {
                    bool setMenuTile = false;
                    Point zeroPoint = Vector2.Transform(Vector2.Zero, Camera.Transform).ToPoint();

                    Rectangle menuRect = new Rectangle(
                        Vector2.Transform(new Vector2(MenuTile.X * (TILE_SIZE + 2), MenuTile.Y * (TILE_SIZE + 2)), Camera.TransformWithoutScale).ToPoint(),
                        new Point((int)(((TILE_SIZE + 2) * 2 + TILE_SIZE) * Camera.Zoom), (int)(TILE_SIZE * Camera.Zoom)));

                    if(menuRect.Right >= viewport.X)
                        menuRect.X -= (int)((TILE_SIZE + 2) * 2 * Camera.Zoom);

                    if(menuRect.Contains(location.Position))
                    {
                        bool explodePressed = location.Position.X < menuRect.X + (int)(TILE_SIZE * Camera.Zoom);
                        bool flagPressed = location.Position.X >= menuRect.X + (int)(TILE_SIZE * Camera.Zoom) && location.Position.X < menuRect.X + (int)(TILE_SIZE * Camera.Zoom) * 2;
                        bool flagvisualPressed = location.Position.X >= menuRect.X + (int)(TILE_SIZE * Camera.Zoom) * 2;

                        if(explodePressed && Field.FlagAt(MenuTile.X, MenuTile.Y) != FlagType.Flag)
                        {
                            bool anyRevealed = false;
                            int x, y;
                            for(x = 0; x < Field.Width; x++)
                            {
                                for(y = 0; y < Field.Height; y++)
                                {
                                    if(Field.IsRevealed(x, y))
                                    {
                                        anyRevealed = true;
                                        break;
                                    }
                                }
                            }

                            if(!anyRevealed)
                                Field[MenuTile.X, MenuTile.Y] = false;

                            Coroutine.Start(OnExplodePressed);
                            Coroutine.AddExisting(RevealAndAnimate(MenuTile.X, MenuTile.Y));

                            MenuTile = new Point(-1, -1);
                        }

                        if(flagPressed)
                        {
                            if(Field.FlagAt(MenuTile.X, MenuTile.Y) != FlagType.Flag && Field.FlagsLeft > 0)
                            {
                                Field.SetFlagAt(MenuTile.X, MenuTile.Y, FlagType.Flag);
                                MenuTile = new Point(-1, -1);
                            }
                            else if(Field.FlagAt(MenuTile.X, MenuTile.Y) == FlagType.Flag)
                            {
                                Field.SetFlagAt(MenuTile.X, MenuTile.Y, FlagType.None);
                                MenuTile = new Point(-1, -1);
                            }
                        }

                        if(flagvisualPressed)
                        {
                            Field.SetFlagAt(MenuTile.X, MenuTile.Y, 
                                Field.FlagAt(MenuTile.X, MenuTile.Y) == FlagType.FlagVisual ? FlagType.None : FlagType.FlagVisual);

                            MenuTile = new Point(-1, -1);
                        }
                    }
                    else
                    {
                        int x, y;
                        for(x = 0; x < Field.Width; x++)
                        {
                            for(y = 0; y < Field.Height; y++)
                            {
                                Rectangle area = new Rectangle(
                                    Vector2.Transform(new Vector2(x * (TILE_SIZE + 2), y * (TILE_SIZE + 2)), Camera.TransformWithoutScale).ToPoint(),
                                    new Point((int)(TILE_SIZE * Camera.Zoom), (int)(TILE_SIZE * Camera.Zoom)));

                                if(area.Contains(location.Position) && !Field.IsRevealed(x, y))
                                {
                                    MenuTile = new Point(x, y);
                                    setMenuTile = true;
                                }
                            }
                        }

                        if(!setMenuTile)
                            MenuTile = new Point(-1, -1);
                    }
                }
            }
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

                    Rectangle rect = new Rectangle(
                        x * (TILE_SIZE + 2), y * (TILE_SIZE + 2),
                        TILE_SIZE, TILE_SIZE);

                    batch.Texture(rect,
                        Assets.Get<Texture2D>("placeholder/tiles/" + texture),
                        Color.White);

                    if(Field.IsRevealed(x, y) && !Field[x, y])
                    {
                        Vector2 measure = Game.DefaultFonts.Bold.Measure(32, Field.SurroundingNukesAt(x, y).ToString());
                        batch.Text(FontStyle.Bold, 32, Field.SurroundingNukesAt(x, y).ToString(), rect.Center.ToVector2() - measure / 2, Color.Red);
                    }
                }
            }

            if(MenuTile != new Point(-1, -1))
            {
                Rectangle rect = new Rectangle(MenuTile.X * (TILE_SIZE + 2), MenuTile.Y * (TILE_SIZE + 2), (TILE_SIZE + 2) * 2 + TILE_SIZE, TILE_SIZE);

                int oldX = rect.X;

                if(Vector2.Transform(new Vector2(rect.Right, 0), Camera.TransformWithoutScale).X >= screenSize.X)
                    rect.X -= (TILE_SIZE + 2) * 2;

                batch.Rectangle(rect, Color.Black);
                batch.Outline(rect, Color.White, 3, false);
                batch.Outline(new Rectangle(oldX, rect.Y, TILE_SIZE, TILE_SIZE), Color.White, 3, false);

                batch.Texture(new Rectangle(rect.X + 2, rect.Y + 2, TILE_SIZE - 4, TILE_SIZE - 4), Assets.Get<Texture2D>("icons/explode.png"), 
                    Color.White * (Field.FlagAt(MenuTile.X, MenuTile.Y) == FlagType.Flag ? 0.6f : 1.0f));
                batch.Texture(new Rectangle(rect.X + 4 + TILE_SIZE, rect.Y + 2, TILE_SIZE - 4, TILE_SIZE - 4), Assets.Get<Texture2D>("icons/flag.png"), 
                    Color.White * (Field.FlagsLeft == 0 && Field.FlagAt(MenuTile.X, MenuTile.Y) != FlagType.Flag ? 0.6f : 1.0f));
                batch.Texture(new Rectangle(rect.X + 6 + TILE_SIZE * 2, rect.Y + 2, TILE_SIZE - 4, TILE_SIZE - 4), Assets.Get<Texture2D>("icons/flagvisual.png"), 
                    Color.White);
            }

            batch.Transform = Matrix.Identity;

            batch.Rectangle(new Rectangle(0, 0, (int)screenSize.X, 180), Color.DarkGray);
            batch.Outline(new Rectangle(0, 0, (int)screenSize.X, 180), Color.White, 2, false);

            batch.SamplerState = SamplerState.PointClamp;

            string reaction = "idle.png";

            switch(_currentFace)
            {
                case FaceReaction.Hover:
                    reaction = "hover.png";
                    break;

                case FaceReaction.Win:
                    reaction = "win.png";
                    break;

                case FaceReaction.Lose:
                    reaction = "lose.png";
                    break;
            }

            batch.Texture(new Rectangle((int)screenSize.X / 2 - 64 / 2, 40, 64, 64), Assets.Get<Texture2D>("placeholder/reactions/" + reaction), Color.White);
            batch.SamplerState = SamplerState.LinearClamp;
        }
    }
} 