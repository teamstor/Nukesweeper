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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace TeamStor.Engine
    {
        public class Country
        {
            public string Name;
            public string ID;
            public string[] Nukes;
            public float NukeCountMultiplier;

            public Country(string name, string id, string[] nukes, float nukeCountMultiplier)
            {
                Name = name;
                ID = id;
                Nukes = nukes;
                NukeCountMultiplier = nukeCountMultiplier;
            }
        }
    } //kaspers kod
}