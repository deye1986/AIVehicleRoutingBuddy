using UnityEngine;
using TMPro;

public class AIDebugUI : MonoBehaviour
{
    [Header("Assign AI Cars")]
    public AICarController[] aiCars;
    public TextMeshProUGUI debugTextPanel;
    public int columns = 3;

    void Update()
    {
        if (aiCars == null || debugTextPanel == null || columns < 1) return;

        string keyboardStatus = GetKeyboardStatus();

        int carsPerColumn = Mathf.CeilToInt(aiCars.Length / (float)columns);
        string[] columnTexts = new string[columns];

        for (int c = 0; c < columns; c++)
            columnTexts[c] = "";

        for (int i = 0; i < aiCars.Length; i++)
        {
            var ai = aiCars[i];
            if (ai == null) continue;

            int col = i / carsPerColumn;

            string overtakeColor = ai.IsOvertaking ? "green" : "white";
            string collisionColor = ai.IsRecovering ? "red" : "white";

            columnTexts[col] += $"[{ai.name}]\n";
            columnTexts[col] += $"Speed: {ai.CurrentSpeed:F1}\t\n";
            columnTexts[col] += $"Throttle: {ai.CurrentThrottleInput:F2}\n";
            columnTexts[col] += $"Overtake: <color={overtakeColor}>{(ai.IsOvertaking ? "YES" : "NO")}</color>\n";
            columnTexts[col] += $"Collision: <color={collisionColor}>{(ai.IsRecovering ? "YES" : "NO")}</color>\n";
        }

        string combinedCarData = "";
        int maxLines = 0;
        foreach (var colText in columnTexts)
        {
            int lines = colText.Split('\n').Length;
            if (lines > maxLines) maxLines = lines;
        }

        string[][] colLines = new string[columns][];
        for (int c = 0; c < columns; c++)
            colLines[c] = columnTexts[c].Split('\n');

        for (int line = 0; line < maxLines; line++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (line < colLines[c].Length)
                    combinedCarData += colLines[c][line] + "\t";
                else
                    combinedCarData += "\t\t";
            }
            combinedCarData += "\n";
        }

        debugTextPanel.text = keyboardStatus + "\n" + combinedCarData;
    }

    string GetKeyboardStatus()
    {
        bool wPressed = Input.GetKey(KeyCode.W);
        bool aPressed = Input.GetKey(KeyCode.A);
        bool sPressed = Input.GetKey(KeyCode.S);
        bool dPressed = Input.GetKey(KeyCode.D);
        bool spacePressed = Input.GetKey(KeyCode.Space);
        
        string wColor = wPressed ? "#FF0000" : "#808080";
        string sColor = sPressed ? "#FFA500" : "#808080";
        string aColor = aPressed ? "#FFA500" : "#808080";
        string dColor = dPressed ? "#FFA500" : "#808080"; 
        string spaceColor = spacePressed ? "#FF0000" : "#808080";
        
        string status = "<b><size=120%>€VOLUTION DEBUGGER (c) by Dave Ikin 2025</size></b>\n\n";
        
        status += $"<color={wColor}>[W]</color> Acceleration    ";
        status += $"<color={sColor}>[S]</color> Reverse    ";
        status += $"<color={spaceColor}>[SPACE]</color> Handbrake\n";
        status += $"<color={aColor}>[A]</color> Left    ";
        status += $"<color={dColor}>[D]</color> Right\n";
        
        status += "--------------------------------";
        
        return status;
    }
}