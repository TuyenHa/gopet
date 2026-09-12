using Dapper;
using Gopet.Util;
using System;
using System.Linq;

/// <summary>
/// Ranking for the pet selected to defend in Pet League.  The old client had
/// an opcode for this screen but the server never supplied a handler or top,
/// despite persisting <c>PlayerData.PetDefLeague</c>.
/// </summary>
public sealed class TopPetLeague : Top
{
    public static readonly TopPetLeague Instance = new TopPetLeague();

    private TopPetLeague() : base("top_pet_league")
    {
        name = "Top Pet League";
        desc = "Thú cưng phòng thủ mạnh nhất";
    }

    public override TopData getMyInfo(Player player)
    {
        var found = datas.FirstOrDefault(x => x.id == player.playerData.user_id);
        if (found != null) return found;

        var pet = player.playerData.PetDefLeague;
        if (pet == null) return null;
        var template = pet.getPetTemplate();
        return new TopData
        {
            id = player.playerData.user_id,
            name = player.playerData.name,
            imgPath = template.icon,
            title = TopPet.Instance.getNameWithStar(pet.star, template),
            desc = $"Chưa xếp hạng: Cấp {pet.lvl}, {Utilities.FormatNumber(pet.exp)} kinh nghiệm"
        };
    }

    public override void Update()
    {
        try
        {
            lastDatas.Clear();
            lastDatas.AddRange(datas);
            datas.Clear();
            using var conn = MYSQLManager.create();
            var rows = conn.Query(@"SELECT user_id, name, avatarPath,
                JSON_VALUE(PetDefLeague, '$.petIdTemplate') AS petTemplateId,
                JSON_VALUE(PetDefLeague, '$.star') AS star,
                JSON_VALUE(PetDefLeague, '$.lvl') AS lvl,
                JSON_VALUE(PetDefLeague, '$.exp') AS exp
                FROM player
                WHERE isAdmin = 0 AND PetDefLeague IS NOT NULL AND PetDefLeague <> 'null'
                ORDER BY CAST(JSON_VALUE(PetDefLeague, '$.star') AS UNSIGNED) DESC,
                         CAST(JSON_VALUE(PetDefLeague, '$.lvl') AS UNSIGNED) DESC,
                         CAST(JSON_VALUE(PetDefLeague, '$.exp') AS UNSIGNED) DESC
                LIMIT 50");

            var rank = 1;
            foreach (dynamic row in rows)
            {
                var template = GopetManager.PETTEMPLATE_HASH_MAP.get((int)row.petTemplateId);
                if (template == null) continue;
                var star = Convert.ToInt32(row.star);
                var level = Convert.ToInt32(row.lvl);
                var exp = Convert.ToInt64(row.exp);
                datas.Add(new TopData
                {
                    id = row.user_id,
                    name = row.name,
                    imgPath = template.icon,
                    title = TopPet.Instance.getNameWithStar(star, template) + " của " + row.name,
                    desc = $"Hạng {rank}: Cấp {level}, {Utilities.FormatNumber(exp)} kinh nghiệm"
                });
                rank++;
            }
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
}
