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
using Microsoft.Xna.Framework.Input.Touch;

namespace TeamStor.Nukesweeper.Gameplay
{
    /// <summary>
    /// Pans and zooms over the nuke field.
    /// </summary>
    public class Camera
    {
        private bool _isPanning;
        private Vector2 _panStart;
        private Vector2 _currentPan;
        private Vector2 _extraVelocity = Vector2.Zero;
        private Vector2 _extraVelocityCurrent = Vector2.Zero;

        private bool _isZooming;
        private double _zoomStart;

        private bool _isFirstFrame = true;

        public PlayingState PlayingState
        {
            get; private set;
        }

        /// <summary>
        /// Current translation.
        /// </summary>
        public Vector2 Translation;

        /// <summary>
        /// Current zoom.
        /// </summary>
        public float Zoom = 1;

        /// <summary>
        /// Minimum amount that can be zoomed.
        /// </summary>
        public float MinimumZoom
        {
            get; private set;
        }

        /// <summary>
        /// Transform to apply for this camera.
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return Matrix.CreateScale(Zoom) * Matrix.CreateTranslation(
                    (Translation.X + _currentPan.X) * (float)PlayingState.Game.Scale,
                    (Translation.Y + _currentPan.Y + 180) * (float)PlayingState.Game.Scale, 
                    0);
            }
        }

        /// <summary>
        /// Transform to apply for this camera without Game.Scale.
        /// </summary>
        public Matrix TransformWithoutScale
        {
            get
            {
                return Matrix.CreateScale(Zoom) * Matrix.CreateTranslation(
                    (Translation.X + _currentPan.X),
                    (Translation.Y + _currentPan.Y + 180),
                    0);
            }
        }

        public bool IsMoving { get { return _currentPan != Vector2.Zero || _isZooming; } }

        /// <summary>
        /// If the camera just finished panning.
        /// In this case, no press should be made this frame.
        /// </summary>
        public bool JustDidPan
        {
            get; private set;
        }

        /// <summary>
        /// If the camera just finished zooming.
        /// In this case, no press should be made this frame.
        /// </summary>
        public bool JustDidZoom
        {
            get; private set;
        }

        private Vector2 AggregatePosition(TouchCollection collection)
        {
            Vector2 pos = new Vector2(0, 0);

            foreach(TouchLocation v in collection)
                pos += v.Position;

            pos /= collection.Count;
            return pos;
        }

        public Camera(PlayingState state)
        {
            PlayingState = state;
        }

        public void UpdatePosition(TouchCollection input, TouchCollection lastInput)
        {
            Vector2 screenSize = (PlayingState.Game.GraphicsDevice.Viewport.Bounds.Size.ToVector2() /
                (float)PlayingState.Game.Scale) - new Vector2(0, 180);

            Vector2 fieldSize = new Vector2(
                PlayingState.Field.Width * (PlayingState.TILE_SIZE + 2),
                PlayingState.Field.Height * (PlayingState.TILE_SIZE + 2));

            MinimumZoom = Math.Min(1, screenSize.X / (fieldSize.X + 60));

            fieldSize *= Zoom;

            JustDidPan = false;
            JustDidZoom = false;

            if(input.Count >= 1 && lastInput.Count == 0 && (fieldSize.X >= screenSize.X || fieldSize.Y >= screenSize.Y))
            {
                _isPanning = true;
                _panStart = AggregatePosition(input);
                _currentPan = new Vector2(0, 0);
                _extraVelocityCurrent = new Vector2(0, 0);
            }

            if(input.Count >= 2 && lastInput.Count <= 1)
            {
                _isZooming = true;
                _zoomStart = Vector2.Distance(input[0].Position, input[1].Position);
            }

            if(_isZooming && input.Count <= 1 && lastInput.Count >= 2)
            {
                _isZooming = false;
                JustDidZoom = true;
            }
            else if(_isZooming && input.Count >= 2 && lastInput.Count >= 2)
            {
                float zoomDiff = Zoom;
                Vector2 oldFieldSize = fieldSize;
                fieldSize /= Zoom;

                Zoom += (Vector2.Distance(input[0].Position, input[1].Position) - Vector2.Distance(lastInput[0].Position, lastInput[1].Position)) * 0.001f;
                Zoom = MathHelper.Clamp(Zoom, MinimumZoom, 1);
                zoomDiff = Zoom - zoomDiff;
                fieldSize *= Zoom;

                Vector2 mouseThing = ((AggregatePosition(input) - Translation - new Vector2(0, 180)) / oldFieldSize);
                Translation -= (fieldSize - oldFieldSize) * mouseThing;
            }

            if(_isPanning && input.Count == 0 && lastInput.Count >= 1)
            {
                JustDidPan = Math.Abs(AggregatePosition(lastInput).X - _panStart.X) > 4 ||
                    Math.Abs(AggregatePosition(lastInput).Y - _panStart.Y) > 4;

                if(JustDidPan)
                {
                    if(Math.Abs(AggregatePosition(lastInput).X - _panStart.X) > 4)
                        Translation.X += AggregatePosition(lastInput).X - _panStart.X;
                    if(Math.Abs(AggregatePosition(lastInput).Y - _panStart.Y) > 4)
                        Translation.Y += AggregatePosition(lastInput).Y - _panStart.Y;
                }

                _currentPan = new Vector2(0, 0);
                _extraVelocity += _extraVelocityCurrent;
                _isPanning = false;
            }
            else if(_isPanning)
            {
                _currentPan = new Vector2(0, 0);
                _extraVelocityCurrent = new Vector2(0, 0);

                if(input.Count != lastInput.Count)
                {
                    if(Math.Abs(AggregatePosition(lastInput).X - _panStart.X) > 8)
                        Translation.X += AggregatePosition(lastInput).X - _panStart.X;
                    if(Math.Abs(AggregatePosition(lastInput).Y - _panStart.Y) > 8)
                        Translation.Y += AggregatePosition(lastInput).Y - _panStart.Y;

                    _panStart = AggregatePosition(input);
                }

                if(Math.Abs(AggregatePosition(input).X - _panStart.X) > 8)
                {
                    _currentPan.X = AggregatePosition(input).X - _panStart.X;
                    if(input.Count == lastInput.Count)
                        _extraVelocityCurrent.X = (AggregatePosition(input).X - AggregatePosition(lastInput).X) * 0.6f;
                }

                if(Math.Abs(AggregatePosition(input).Y - _panStart.Y) > 4)
                {
                    _currentPan.Y = AggregatePosition(input).Y - _panStart.Y;
                    if(input.Count == lastInput.Count)
                        _extraVelocityCurrent.Y = (AggregatePosition(input).Y - AggregatePosition(lastInput).Y) * 0.6f;
                }

                Vector2 extraOutsideBounds = new Vector2(0, 0);

                if(Translation.X + _currentPan.X < -(fieldSize.X - screenSize.X) - 40)
                    extraOutsideBounds.X = (-(fieldSize.X - screenSize.X) - 40) - (Translation.X + _currentPan.X);
                if(Translation.X + _currentPan.X > 40)
                    extraOutsideBounds.X = 40 - (Translation.X + _currentPan.X);

                if(Translation.Y + _currentPan.Y < -(fieldSize.Y - screenSize.Y) - 40)
                    extraOutsideBounds.Y = (-(fieldSize.Y - screenSize.Y) - 40) - (Translation.Y + _currentPan.Y);
                if(Translation.Y + _currentPan.Y > 40)
                    extraOutsideBounds.Y = 40 - (Translation.Y + _currentPan.Y);

                if(extraOutsideBounds.X != 0)
                    _currentPan.X = _currentPan.X + extraOutsideBounds.X;

                if(extraOutsideBounds.Y != 0)
                    _currentPan.Y = _currentPan.Y + extraOutsideBounds.Y;
            }

            if(!_isPanning && !_isZooming)
            {
                if(Math.Abs(_extraVelocity.X) > 0)
                {
                    Translation.X += _extraVelocity.X;
                    _extraVelocity.X = MathHelper.LerpPrecise(_extraVelocity.X, 0, (float)PlayingState.Game.DeltaTime * 10f);
                }

                if(Math.Abs(_extraVelocity.Y) > 0)
                {
                    Translation.Y += _extraVelocity.Y;
                    _extraVelocity.Y = MathHelper.LerpPrecise(_extraVelocity.Y, 0, (float)PlayingState.Game.DeltaTime * 10f);
                }
            }

            for(float i = 1; i >= 0.8; i -= 0.05f)
            {
                Vector2 zFieldSize = new Vector2(
                    PlayingState.Field.Width * (PlayingState.TILE_SIZE + 2),
                    PlayingState.Field.Height * (PlayingState.TILE_SIZE + 2)) * i;

                if(screenSize.X > zFieldSize.X + 60 && screenSize.Y > zFieldSize.Y + 60)
                {
                    Zoom = i;
                    fieldSize = zFieldSize;
                    break;
                }
            }

            if(_isFirstFrame || screenSize.X > fieldSize.X)
            {
                Translation.X = screenSize.X / 2 - fieldSize.X / 2;
                _panStart.X = AggregatePosition(input).X;
                _currentPan.X = 0;
            }
            else
                Translation.X = MathHelper.Clamp(Translation.X, -(fieldSize.X - screenSize.X) - 40, 40);
            if(_isFirstFrame || screenSize.Y > fieldSize.Y)
            {
                Translation.Y = screenSize.Y / 2 - fieldSize.Y / 2;
                _panStart.Y = AggregatePosition(input).Y;
                _currentPan.Y = 0;
            }
            else
                Translation.Y = MathHelper.Clamp(Translation.Y, -(fieldSize.Y - screenSize.Y) - 40, 40);

            _isFirstFrame = false;
        }
    }
}
 