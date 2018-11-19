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

namespace TeamStor.Nukesweeper.Gameplay
{
    /// <summary>
    /// Tile flag type.
    /// </summary>
    public enum FlagType : byte
    {
        /// <summary>
        /// Not flagged.
        /// </summary>
        None,

        /// <summary>
        /// Flag that takes up a point on the flag counter.
        /// </summary>
        Flag,

        /// <summary>
        /// Flag that only shows a question mark and doesn't prevent you from clicking on it.
        /// </summary>
        FlagVisual
    }
}