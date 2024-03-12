using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckGame;

namespace DuckGame.WMD
{
    internal class WMDMain : IAutoUpdate
    {
        private static string[] blacklist_levels = new string[]
        {
            "DuckGame.TitleScreen",
            "DuckGame.HighlightLevel",
            "DuckGame.RockIntro",
            "DuckGame.TeamSelect2",
            "DuckGame.ArcadeLevel"
        };

        private Mod _parentmod;

        public WMDMain(Mod parent)
        {
            _parentmod = parent;
            AutoUpdatables.Add(this);

        }
        public void Update()
        {

        }
    }
}
