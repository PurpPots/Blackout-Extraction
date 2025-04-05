[System.Serializable]
public class UpgradeOptionData
{
    public string upgradeName;
    public int cost;
    public int count = 0;

    public UpgradeOptionData(string name, int price)
    {
        upgradeName = name;
        cost = price;
    }

    public int TotalCost => count * cost;
}
