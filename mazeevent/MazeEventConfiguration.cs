using Rocket.API;
using UnityEngine;

namespace datathegenius.mazeevent
{
    public class MazeEventConfiguration : IRocketPluginConfiguration
    {
        public int announcementSeconds = 30;
        public uint expAmount = 1500;

        public Color ErrorColor = Color.red;
        public Color SuccessColor = Color.green;
        public Color AnnouncementColor = Color.cyan;


        public void LoadDefaults()
        {
            announcementSeconds = 30;

            Color ErrorColor = Color.red;
            Color SuccessColor = Color.green;
            Color AnnouncementColor = Color.cyan;
        }
    }
}
