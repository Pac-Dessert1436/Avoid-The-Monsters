using Godot;
using VBLibrary.AvoidTheMonsters;

public class HUD : HeadUpDisplayVB
{
    public override async void ShowGameOver()
    {
        ShowMessage("End of Game");
        var messageTimer = GetNode<Timer>("MessageTimer");
        await ToSignal(messageTimer, "timeout");
        var message = GetNode<Label>("Message");
        message.Text = "Avoid the\nMonsters!";
        message.Show();
        await ToSignal(GetTree().CreateTimer(1), "timeout");
        GetNode<Button>("StartButton").Show();
    }
}