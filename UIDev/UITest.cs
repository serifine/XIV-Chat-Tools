using ImGuiNET;
using ImGuiScene;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    class UITest : IPluginUIMock
    {
        public List<FocusTab> FocusTabs = new List<FocusTab>() { new FocusTab("Siroh'a Relana", 0), new FocusTab("Lio'li Mewrillah", 0) };


        public static void Main(string[] args)
        {
            UIBootstrap.Inititalize(new UITest());
        }

        // private TextureWrap goatImage
        private SimpleImGuiScene scene;

        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here

            // eg, to load an image resource for use with ImGui 
            // this.goatImage = scene.LoadImage(@"goat.png");

            scene.OnBuildUI += Draw;

            this.Visible = true;

            // saving this only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            this.scene = scene;
        }

        public void Dispose()
        {
            // this.goatImage.Dispose();
        }

        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
        // don't want to imply that is easy or the best way to go usually, so it's not done here either
        private void Draw()
        {
            DrawMainWindow();
            DrawSettingsWindow();

            if (!Visible)
            {
                this.scene.ShouldQuit = true;
            }
        }

        #region Nearly a copy/paste of PluginUI
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        private bool autoScrollToBottom = false;
        public bool AutoScrollToBottom
        {
            get { return this.autoScrollToBottom; }
            set { this.autoScrollToBottom = value; }
        }

        // this is where you'd have to start mocking objects if you really want to match
        // but for simple UI creation purposes, just hardcoding values works
        private bool fakeConfigBool = true;

        private readonly Vector4 ORANGE_COLOR = new Vector4(0.950f, 0.500f, 0f, 1f);
        private readonly Vector4 LIGHT_ORANGE_COLOR = new Vector4(0.950f, 0.650f, 0f, 1f);

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ORANGE_COLOR);
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ORANGE_COLOR);

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));

            var scale = ImGui.GetIO().FontGlobalScale;

            if (ImGui.Begin("Text Tracking Demo", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Button("Add Focus Target"))
                {
                    SettingsVisible = true;
                }
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (190 * scale));
                ImGui.Checkbox("Auto scroll on new messages.", ref autoScrollToBottom);


                ImGui.BeginTabBar("MainTabs", ImGuiTabBarFlags.Reorderable);

                if (ImGui.BeginTabItem("All Messages"))
                {
                    MessagePanel();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Hover Over Target"))
                {
                    MessagePanel();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Keyword Scan"))
                {
                    MessagePanel();
                    ImGui.EndTabItem();
                }

                foreach (var tab in FocusTabs)
                {
                    if (ImGui.BeginTabItem(tab.Name, ref tab.Open, ImGuiTabItemFlags.None))
                    {
                        MessagePanel();
                        ImGui.EndTabItem();
                    }
                }

                FocusTabs.RemoveAll(t => t.Open == false);

                ImGui.EndTabBar();
            }
            ImGui.End();
        }

        public void MessagePanel()
        {
            ImGui.BeginChild("Messages");

            var isChatAtBottom = ImGui.GetScrollY() == ImGui.GetScrollMaxY();

            ImGui.Spacing();
            ImGui.Text("[4:26] Siroh'a Relana: ");
            ImGui.SameLine();
            ImGui.TextWrapped("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean nec sagittis enim. Vestibulum quis ex tellus. Aliquam finibus sem massa, in rutrum nisi feugiat eget. Quisque porta neque non metus vestibulum, sed rhoncus metus tempus. Integer imperdiet, ipsum at consequat sollicitudin, ipsum velit dictum enim, nec lobortis lacus libero sit amet est. Curabitur sem neque, vulputate a feugiat sed, porta sed nisl. Ut vulputate, tellus vitae ullamcorper ullamcorper, risus orci lobortis lectus, quis tincidunt sapien elit at nibh. Donec elementum mauris ut gravida tincidunt. Praesent placerat, quam vitae dapibus imperdiet, ipsum orci consequat erat, laoreet feugiat ante orci ac arcu. Donec rhoncus commodo elit, eget consectetur mauris auctor ac. Maecenas sodales nec dui eu cursus. Etiam hendrerit ante dolor, non dapibus elit sollicitudin vel.");

            ImGui.TextColored(ORANGE_COLOR, "[4:26] L'nikette Elran: ");
            ImGui.SameLine();
            ImGui.TextWrapped("Aenean nec sagittis enim. Vestibulum quis ex tellus. Aliquam finibus sem massa, in rutrum nisi feugiat eget. Quisque porta neque non metus vestibulum, sed rhoncus metus tempus. Integer imperdiet, ipsum at consequat sollicitudin, ipsum velit dictum enim, nec lobortis lacus libero sit amet est. Curabitur sem neque, vulputate a feugiat sed, porta sed nisl. Ut vulputate, tellus vitae ullamcorper ullamcorper, risus orci lobortis lectus, quis tincidunt sapien elit at nibh. Donec elementum mauris ut gravida tincidunt. Praesent placerat, quam vitae dapibus imperdiet, ipsum orci consequat erat, laoreet feugiat ante orci ac arcu. Donec rhoncus commodo elit, eget consectetur mauris auctor ac. Maecenas sodales nec dui eu cursus. Etiam hendrerit ante dolor, non dapibus elit sollicitudin vel.");

            ImGui.Spacing();
            ImGui.Text("[4:26] Siroh'a Relana: ");
            ImGui.SameLine();
            ImGui.TextWrapped("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean nec sagittis enim. Vestibulum quis ex tellus. Aliquam finibus sem massa, in rutrum nisi feugiat eget. Quisque porta neque non metus vestibulum, sed rhoncus metus tempus. Integer imperdiet, ipsum at consequat sollicitudin, ipsum velit dictum enim, nec lobortis lacus libero sit amet est. Curabitur sem neque, vulputate a feugiat sed, porta sed nisl. Ut vulputate, tellus vitae ullamcorper ullamcorper, risus orci lobortis lectus, quis tincidunt sapien elit at nibh. Donec elementum mauris ut gravida tincidunt. Praesent placerat, quam vitae dapibus imperdiet, ipsum orci consequat erat, laoreet feugiat ante orci ac arcu. Donec rhoncus commodo elit, eget consectetur mauris auctor ac. Maecenas sodales nec dui eu cursus. Etiam hendrerit ante dolor, non dapibus elit sollicitudin vel.");

            ImGui.Spacing();
            ImGui.Text("[4:26] Siroh'a Relana: ");
            ImGui.SameLine();
            ImGui.TextWrapped("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean nec sagittis enim. Vestibulum quis ex tellus. Aliquam finibus sem massa, in rutrum nisi feugiat eget. Quisque porta neque non metus vestibulum, sed rhoncus metus tempus. Integer imperdiet, ipsum at consequat sollicitudin, ipsum velit dictum enim, nec lobortis lacus libero sit amet est. Curabitur sem neque, vulputate a feugiat sed, porta sed nisl. Ut vulputate, tellus vitae ullamcorper ullamcorper, risus orci lobortis lectus, quis tincidunt sapien elit at nibh. Donec elementum mauris ut gravida tincidunt. Praesent placerat, quam vitae dapibus imperdiet, ipsum orci consequat erat, laoreet feugiat ante orci ac arcu. Donec rhoncus commodo elit, eget consectetur mauris auctor ac. Maecenas sodales nec dui eu cursus. Etiam hendrerit ante dolor, non dapibus elit sollicitudin vel.");

            ImGui.TextColored(ORANGE_COLOR, "[4:26] L'nikette Elran: ");
            ImGui.SameLine();
            ImGui.TextWrapped("Aenean nec sagittis enim. Vestibulum quis ex tellus. Aliquam finibus sem massa, in rutrum nisi feugiat eget. Quisque porta neque non metus vestibulum, sed rhoncus metus tempus. Integer imperdiet, ipsum at consequat sollicitudin, ipsum velit dictum enim, nec lobortis lacus libero sit amet est. Curabitur sem neque, vulputate a feugiat sed, porta sed nisl. Ut vulputate, tellus vitae ullamcorper ullamcorper, risus orci lobortis lectus, quis tincidunt sapien elit at nibh. Donec elementum mauris ut gravida tincidunt. Praesent placerat, quam vitae dapibus imperdiet, ipsum orci consequat erat, laoreet feugiat ante orci ac arcu. Donec rhoncus commodo elit, eget consectetur mauris auctor ac. Maecenas sodales nec dui eu cursus. Etiam hendrerit ante dolor, non dapibus elit sollicitudin vel.");

            if (AutoScrollToBottom == true)
            {
                ImGui.SetScrollY(ImGui.GetScrollMaxY());
            }

            ImGui.EndChild();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
            if (ImGui.Begin("A Wonderful Configuration Window", ref this.settingsVisible,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Checkbox("Random Config Bool", ref this.fakeConfigBool))
                {
                    // nothing to do in a fake ui!
                }
            }
            ImGui.End();
        }
        #endregion
    }
}
