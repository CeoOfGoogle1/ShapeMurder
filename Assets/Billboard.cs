using System.Text;
using TMPro;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    TMP_Text label;
    Camera cam;
    Mover mover;
    Region region;
    Battle battle;

    void Start()
    {
        cam = Camera.main;
        mover = GetComponentInParent<Mover>();
        battle = GetComponentInParent<Battle>();
        region = GetComponentInParent<Region>();
        label = GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        transform.localPosition = new Vector3(0, 1.5f, 0);

        if (mover)
        {
            label.text = BuildMoverText(mover);
        }
        else if (battle)
        {
            label.text = BuildBattleText(battle);
        }
        else if (region)
        {
            label.text = BuildRegionText(region);
        }
    }

    void LateUpdate()
    {
        transform.LookAt(
        transform.position + 
        cam.transform.rotation * 
        Vector3.forward, 
        cam.transform.rotation * 
        Vector3.up);
    }

    public static string BuildMoverText(Mover mover)
    {
        StringBuilder sb = new();
        AppendArmy(sb, mover.army);
        return sb.ToString();
    }

    public static string BuildBattleText(Battle battle)
    {
        StringBuilder sb = new();

        for (int sideIndex = 0; sideIndex < battle.sides.Count; sideIndex++)
        {
            if (sideIndex > 0) sb.Append("vs");
            // bold
            //sb.Append(" <b>vs</b> ");
            Side side = battle.sides[sideIndex];

            for (int armyIndex = 0; armyIndex < side.armies.Count; armyIndex++)
            {
                if (armyIndex > 0) sb.Append("+");
                Army army = side.armies[armyIndex];
                AppendArmy(sb, army);
            }
        }
        return sb.ToString();
    }

    public static string BuildRegionText(Region region)
    {
        StringBuilder sb = new();

        AppendArmy(sb, region.garrison);

        foreach (var visitor in region.visitors)
        {
            sb.Append("+");
            Color c = visitor.Value.player.Color;
            sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(c)}>");
            sb.Append(visitor.Value.size);
            sb.Append("</color>");
        }
        return sb.ToString();
    }

    private static void AppendArmy(StringBuilder sb, Army army)
    {
        sb.Append("<color=#");
        sb.Append(ColorUtility.ToHtmlStringRGB(army.player.Color));
        sb.Append(">");
        sb.Append(army.size);
        sb.Append("</color>");
    }
}