using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TeamStor.Nukesweeper.Gameplay
{
    /// <summary>
    /// Field of nukes.
    /// </summary>
    public class NukeField
    {
        private BitArray _field;
        private BitArray _fieldRevealed;
        private FlagType[] _fieldFlags;

        /// <summary>
        /// Width of the nuke field in tiles.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the nuke field in tiles.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Nukes this nuke field was started with.
        /// </summary>
        public int NukeCount { get; private set; }

        /// <summary>
        /// The amount of normal flags left.
        /// </summary>
        public int FlagsLeft { get; private set; }

        public NukeField(int width, int height, int nukeCount)
        {
            Width = width;
            Height = height;
            NukeCount = nukeCount;
            FlagsLeft = nukeCount;

            _field = new BitArray(width * height);
            _fieldRevealed = new BitArray(width * height);
            _fieldFlags = new FlagType[width * height];

            Random rand = new Random();

            for(int i = 0; i < nukeCount; i++)
                _field[rand.Next() % _field.Length] = true;
        }

        public bool this[int x, int y]
        {
            get
            {
                if(x < 0 || x >= Width || y < 0 || y >= Height)
                    return false;
                return _field[(y * Width) + x];
            }
            set
            {
                if(x >= 0 && x < Width && y >= 0 && y < Height)
                    _field[(y * Width) + x] = value;
            }
        }

        /// <param name="x">The X position of the tile to check.</param>
        /// <param name="y">The Y position of the tile to check.</param>
        /// <returns>The flag at the specified tile.</returns>
        public FlagType FlagAt(int x, int y)
        {
            if(x < 0 || x >= Width || y < 0 || y >= Height)
                return FlagType.None;
            return _fieldFlags[(y * Width) + x];
        }

        /// <summary>
        /// Sets the flag at the specified tile.
        /// </summary>
        /// <param name="x">The X position of the tile to set.</param>
        /// <param name="y">The Y position of the tile to set.</param>
        /// <param name="value">New value to set.</param>
        public void SetFlagAt(int x, int y, FlagType value)
        {
            if(x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if(_fieldFlags[(y * Width) + x] == FlagType.Flag &&
                    value != FlagType.Flag)
                    FlagsLeft++;

                if(value == FlagType.Flag && FlagsLeft <= 0)
                    throw new Exception("Maximum amount of flags reached (flag count = nuke count).");

                _fieldFlags[(y * Width) + x] = value;

                if(value == FlagType.Flag)
                    FlagsLeft--;
            }
        }

        /// <param name="x">The X position of the tile to check.</param>
        /// <param name="y">The Y position of the tile to check.</param>
        /// <returns>If the tile is revealed or not.</returns>
        public bool IsRevealed(int x, int y)
        {
            if(x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            return _fieldRevealed[(y * Width) + x];
        }

        /// <summary>
        /// Sets if the specified tile is revealed.
        /// </summary>
        /// <param name="x">The X position of the tile to set.</param>
        /// <param name="y">The Y position of the tile to set.</param>
        /// <param name="value">New value to set.</param>
        public void SetRevealed(int x, int y, bool value)
        {
            if(x >= 0 && x < Width && y >= 0 && y < Height)
                _fieldRevealed[(y * Width) + x] = value;
        }

        /// <param name="x">The X position of the tile to check.</param>
        /// <param name="y">The Y position of the tile to check.</param>
        /// <returns>The amount of nukes surrounding the specified tile.</returns>
        public int SurroundingNukesAt(int x, int y)
        {
            int count = 0;

            for(int cx = x - 1; cx <= x + 1; cx++)
            {
                for(int cy = y - 1; cy <= y + 1; cy++)
                {
                    if(this[cx, cy]) count++;
                }
            }

            return count;
        }
    }
}