-- gopet_mob_skill: gán kỹ năng cho từng loại quái (theo petId template).
CREATE TABLE IF NOT EXISTS `gopet_mob_skill` (
  `petId`   INT NOT NULL,
  `skillID` INT NOT NULL,
  `skillLv` INT NOT NULL DEFAULT 1,
  `useRate` TINYINT UNSIGNED NOT NULL DEFAULT 30,
  PRIMARY KEY (`petId`, `skillID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seed: mỗi petId trong gopet_map_moblvl nhận 2 skill cùng nClass (loại trừ IsNeedCard).
-- Boss (petId có trong boss.petTemplateId): 3 skill, useRate = 45.
-- Chỉ seed nếu bảng rỗng.
INSERT INTO `gopet_mob_skill` (`petId`, `skillID`, `skillLv`, `useRate`)
SELECT DISTINCT
    m.petId,
    s.skillID,
    1 AS skillLv,
    CASE WHEN b.petTemplateId IS NOT NULL THEN 45 ELSE 30 END AS useRate
FROM `gopet_map_moblvl` m
JOIN `gopet_pet` pt ON pt.petId = m.petId
JOIN `skill` sk ON sk.nClass = pt.nclass AND sk.IsNeedCard = 0
JOIN (
    SELECT petId, nClass, skillID,
           ROW_NUMBER() OVER (PARTITION BY petId ORDER BY skillID) AS rn
    FROM (
        SELECT DISTINCT m2.petId, pt2.nclass AS nClass, sk2.skillID
        FROM `gopet_map_moblvl` m2
        JOIN `gopet_pet` pt2 ON pt2.petId = m2.petId
        JOIN `skill` sk2 ON sk2.nClass = pt2.nclass AND sk2.IsNeedCard = 0
    ) sub
) s ON s.petId = m.petId AND s.skillID = sk.skillID
LEFT JOIN `boss` b ON b.petTemplateId = m.petId
WHERE s.rn <= CASE WHEN b.petTemplateId IS NOT NULL THEN 3 ELSE 2 END
  AND NOT EXISTS (SELECT 1 FROM `gopet_mob_skill` LIMIT 1);
