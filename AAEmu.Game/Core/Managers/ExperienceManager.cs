﻿using System.Collections.Generic;
using AAEmu.Commons.Utils;
using AAEmu.Game.Models.Game;
using AAEmu.Game.Utils.DB;
using NLog;

namespace AAEmu.Game.Core.Managers;

public class ExperienceManager : Singleton<ExperienceManager>
{
    private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

    private Dictionary<byte, ExperienceLevelTemplate> _levels;

    public int GetExpForLevel(byte level, bool mate = false)
    {
        return level > _levels.Count ? 0 :
            mate ? _levels[level].TotalMateExp : _levels[level].TotalExp;
    }

    public byte GetLevelFromExp(int exp, bool mate = false)
    {
        // Loop the levels to find the level for a given exp value
        for (byte lv = 1; lv < _levels.Count; lv++)
        {
            if (exp >= (mate ? _levels[lv].TotalMateExp : _levels[lv].TotalExp))
                continue;
            return (lv--);
        }
        return 0;
    }

    public int GetExpNeededToGivenLevel(int currentExp, byte targetLevel, bool mate = false)
    {
        var targetexp = GetExpForLevel(targetLevel, mate);
        var diff = targetexp - currentExp;
        return (diff <= 0) ? 0 : diff;
    }

    public int GetSkillPointsForLevel(byte level)
    {
        if (_levels.TryGetValue(level, out var levelInfo))
            return levelInfo.SkillPoints;
        return 0;
    }

    public void Load()
    {
        _levels = new Dictionary<byte, ExperienceLevelTemplate>();
        using (var connection = SQLite.CreateConnection())
        {
            Logger.Info("Loading experience data...");
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM levels";
                command.Prepare();
                using (var sqliteDataReader = command.ExecuteReader())
                using (var reader = new SQLiteWrapperReader(sqliteDataReader))
                {
                    while (reader.Read())
                    {
                        var level = new ExperienceLevelTemplate();
                        level.Level = reader.GetByte("id");
                        level.TotalExp = reader.GetInt32("total_exp");
                        level.TotalMateExp = reader.GetInt32("total_mate_exp");
                        level.SkillPoints = reader.GetInt32("skill_points");
                        _levels.Add(level.Level, level);
                    }
                }
            }

            Logger.Info("Experience data loaded");
        }
    }
}
