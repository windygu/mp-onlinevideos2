﻿using MediaPortal.GUI.Video;
using MediaPortal.GUI.Library;

namespace OnlineVideos.MediaPortal1.Player
{
    public class GUIOnlineVideoOSD : GUIVideoOSD
    {
        public const int WINDOW_ONLINEVIDEOS_OSD = 4759;
        public override int GetID { get { return WINDOW_ONLINEVIDEOS_OSD; } set { } }

        public override string GetModuleName()
        {
            return PluginConfiguration.Instance.BasicHomeScreenName + " OSD";
        }

        public override bool Init()
        {
            bool bResult = Load(GUIGraphicsContext.Skin + @"\myonlinevideosOSD.xml");
            return bResult;
        }

        public override void OnAction(Action action)
        {
            if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
            {
                GUIOnlineVideoFullscreen videoWindow = (GUIOnlineVideoFullscreen)GUIWindowManager.GetWindow(GUIOnlineVideoFullscreen.WINDOW_FULLSCREEN_ONLINEVIDEO);
                videoWindow.OnAction(new Action(Action.ActionType.ACTION_SHOW_OSD, 0, 0));
                videoWindow.OnAction(action);
            }
            else
            {
                base.OnAction(action);
            }
        }
    }
}
