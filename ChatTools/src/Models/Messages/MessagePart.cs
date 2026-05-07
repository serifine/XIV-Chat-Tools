using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using ChatTools.Helpers;

namespace ChatTools;

public interface IMessagePart
{
    float Width { get; }
    void Draw();
    string ToString();
}

public class MessagePart : IMessagePart
{
    public string Text { get; set; }

    [JsonIgnore] public bool? Watched { get; set; }
    [JsonIgnore] public float Width => ImGui.CalcTextSize(Text).X;

    public MessagePart(string text)
    {
        if (text.StartsWith(' '))
            text = text.Trim();

        Text = text;
    }

    public void Draw()
    {
        if (Watched == true)
        {
            var color = ColorConfigurations.GetColor(ColorCategory.Watch);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
        }

        ImGui.Text(Text);

        if (Watched == true)
        {
            ImGui.PopStyleColor();
        }
    }

    public override string ToString()
    {
        return Text;
    }
}

public class AutoTranslateMessagePart : IMessagePart
{
    public string Text { get; set; }

    // text width + space on either side + icons on either side
    [JsonIgnore]
    public float Width => ImGui.CalcTextSize(Text).X + _spaceWidth * 2 + ImGui.GetFontSize() * 2;

    [JsonIgnore]
    private float _spaceWidth = ImGui.CalcTextSize(" ").X;

    public AutoTranslateMessagePart(string text)
    {
        if (text.StartsWith(''))
            text = text.Substring(2, text.Length - 4);

        Text = text;
    }

    public void Draw()
    {
        DrawHelpers.DrawIcon(BitmapFontIcon.AutoTranslateBegin);
        ImGui.SameLine(0, _spaceWidth);
        ImGui.Text(Text);
        ImGui.SameLine(0, _spaceWidth);
        DrawHelpers.DrawIcon(BitmapFontIcon.AutoTranslateEnd);
    }

    public override string ToString()
    {
        return Text;
    }
}

public class IconMessagePart : IMessagePart
{
    public BitmapFontIcon Icon { get; set; }

    // text width + space on either side + icons on either side
    [JsonIgnore]
    public float Width => ImGui.GetFontSize();

    [JsonIgnore]
    private float _spaceWidth = ImGui.CalcTextSize(" ").X;


    public IconMessagePart(BitmapFontIcon icon)
    {
        Icon = icon;
    }

    public void Draw()
    {
        DrawHelpers.DrawIcon(Icon);
    }

    public override string ToString()
    {
        return Icon.ToString();
    }
}
