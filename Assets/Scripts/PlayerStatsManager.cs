using System.IO;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    public CycloneStats CycloneStats { get; private set; }
    public ChessStats ChessStats { get; private set; }

    private const string CycloneFile = "cyclone_player_stats.json";
    private const string ChessFile = "chess_player_stats.json";



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CycloneStats = Load<CycloneStats>(CycloneFile);

        ChessStats = Load<ChessStats>(ChessFile);
    }

    public void AddCardPlayed()
    {
        CycloneStats.totalCardsPlayed++;
        Save(
            CycloneStats,
            CycloneFile
        );
    }

    public void AddBackupPlayed()
    {
        CycloneStats.totalBackupsPlayed++;
        Save(
            CycloneStats,
            CycloneFile
        );
    }

    public void AddCycloneIncremented(int amount)
    {
        CycloneStats.cycloneAmountIncremented += amount;
        Save(
            CycloneStats,
            CycloneFile
        );
    }

    public void AddCycloneDecremented(int amount)
    {
        CycloneStats.cycloneAmountDecremented += amount;
        Save(
            CycloneStats,
            CycloneFile
        );
    }

    public void AddCycloneWin()
    {
        CycloneStats.wins++;
        Save(
            CycloneStats,
            CycloneFile
        );
    }

    public void AddChessPieceCaptured()
    {
        ChessStats.piecesCaptured++;
        Save(
            ChessStats,
            ChessFile
        );
    }

    public void AddChessPawnPromotion()
    {
        ChessStats.pawnPromotions++;
        Save(
           ChessStats,
           ChessFile
       );
    }

    public void AddChessCheckmate()
    {
        ChessStats.checkmates++;
        Save(
            ChessStats,
            ChessFile
        );
    }

    public void AddChessStalemate()
    {
        ChessStats.stalemates++;
        Save(
           ChessStats,
           ChessFile
       );
    }

    public void AddChessWin()
    {
        ChessStats.wins++;
        Save(
           ChessStats,
           ChessFile
       );
    }

    private void Save<T>(T stats, string fileName)
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            fileName
        );

        string json = JsonUtility.ToJson(stats, true);

        File.WriteAllText(path, json);
    }

    private T Load<T>(string fileName) where T : new()
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            fileName
        );

        if (!File.Exists(path))
        {
            T stats = new T();

            Save(stats, fileName);

            return stats;
        }

        string json = File.ReadAllText(path);

        return JsonUtility.FromJson<T>(json);
    }
}
