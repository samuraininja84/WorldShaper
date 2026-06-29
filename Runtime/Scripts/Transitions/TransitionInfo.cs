using UnityEngine;
using Identifier = WorldShaper.TransitionIdentifier;

namespace WorldShaper
{
    public struct TransitionInfo 
    {
        public AreaHandle Area;
        public Connection Connection;
        public Settings Options;
        public Identifier TransitionIn;
        public Identifier TransitionOut;
        public Vector2 Delays;

        TransitionInfo(AreaHandle area, Connection connection, Settings settings, Identifier transitionIn, Identifier transitionOut, Vector2 delays)
        {
            Area = area;
            Connection = connection;
            Options = settings;
            TransitionIn = transitionIn;
            TransitionOut = transitionOut;
            Delays = delays;
        }

        public static TransitionInfo Create(AreaHandle area, Connection connection, Settings settings, Identifier @in, Identifier @out, Vector2 delays) => new(area, connection, settings, @in, @out, delays);

        [System.Flags]
        public enum Settings
        {
            Default = 0,
            Realtime = 1 << 0,
            ReloadActiveScene = 1 << 1,
            ReloadAdditiveScenes = 1 << 2,
            UnloadUnusedAssets = 1 << 3
        }
    }
}