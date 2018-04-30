using BattleDelts.Battle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveClass : MonoBehaviour {

	[Header("General Info")]
	public string moveName;
	public string moveDescription;
	public MajorClass majorType;
	public Sprite status;
	public moveType movType; 
	public byte moveIndex;

	[Header("Battle Info")]
	public byte PP;
	public byte PPLeft;
	public int damage;
	public int hitChance;
	public statusType statusType;
	public List<buffTuple> buffs;
	public int statusChance;
	public int critChance;

	// Could add stat and damage moves?
	// Could add multi-turn moves?

	public void duplicateValues(MoveClass recipient) {
		recipient.moveName = moveName;
		recipient.moveDescription = moveDescription;
		recipient.majorType = majorType;
		recipient.status = status;
		recipient.movType = movType;
		recipient.moveIndex = moveIndex;
		recipient.PP = PP;
		recipient.PPLeft = PPLeft;
		recipient.damage = damage;
		recipient.hitChance = hitChance;
		recipient.statusType = statusType;
		recipient.buffs = buffs;
		recipient.statusChance = statusChance;
		recipient.critChance = critChance;
	}

    public Effectiveness GetEffectivenessAgainst(DeltemonClass delt)
    {
        MajorClass m1 = delt.deltdex.major1;
        MajorClass m2 = delt.deltdex.major2;
        return GetEffectivenessAgainst(m1, m2);
    }
    
    public float EffectivenessAgainst(DeltemonClass delt)
    {
        MajorClass m1 = delt.deltdex.major1;
        MajorClass m2 = delt.deltdex.major2;
        return EffectivenessValue(GetEffectivenessAgainst(m1, m2));
    }

    public Effectiveness GetEffectivenessAgainst(MajorClass m1, MajorClass m2)
    {
        if (majorType.zeroDamage.Contains(m1) || majorType.zeroDamage.Contains(m2))
        {
            return Effectiveness.Ineffective;
        }

        float effectiveness = 1f;

        if (majorType.veryEffective.Contains(m1))
        {
            effectiveness *= 2f;
        }
        if (majorType.veryEffective.Contains(m2))
        {
            effectiveness *= 2f;
        }
        if (majorType.uneffective.Contains(m1))
        {
            effectiveness *= 0.5f;
        }
        if (majorType.uneffective.Contains(m2))
        {
            effectiveness *= 0.5f;
        }

        if (effectiveness == 4.0f) return Effectiveness.VeryStrong;
        else if (effectiveness == 2.0f) return Effectiveness.Strong;
        else if (effectiveness == 1.0f) return Effectiveness.Average;
        else if (effectiveness == 0.5f) return Effectiveness.Weak;
        else if (effectiveness == 0.25f) return Effectiveness.VeryWeak;
        else throw new System.Exception("Effectiveness incorrectly calculated. Value = " + effectiveness);
    }

    public float EffectivenessValue(Effectiveness effectiveness)
    {
        switch(effectiveness)
        {
            case Effectiveness.Ineffective:
                return 0;
            case Effectiveness.VeryWeak:
                return 0.25f;
            case Effectiveness.Weak:
                return 0.5f;
            case Effectiveness.Average:
                return 1;
            case Effectiveness.Strong:
                return 2;
            case Effectiveness.VeryStrong:
                return 4;
            default:
                return 1;
        }
    }

    public float GetMoveDamage(DeltemonClass attackingDelt, DeltemonClass defendingDelt)
    {
        float levelDamage = (((2 * (float)attackingDelt.level) + 10)) / 250;
        float atkDefModifier = 1;
        float otherMods = 1;

        // Determine damage based on attacker and defender stats
        if (movType == moveType.PowerAtk)
        {
            atkDefModifier = attackingDelt.Power / defendingDelt.Courage;
        }
        else // is moveType.TruthAtk
        {
            atkDefModifier = attackingDelt.Truth / defendingDelt.Faith;
        }

        // Extra damage if move is same major as Delt
        if (majorType == attackingDelt.deltdex.major1 || majorType == attackingDelt.deltdex.major2)
        {
            otherMods = 1.5f;
        }

        float rawDamage = ((levelDamage * atkDefModifier * damage) + 2) * otherMods;
        return rawDamage * EffectivenessAgainst(defendingDelt);
    }

    public float GetMoveDamage(DeltemonClass attackingDelt, DeltemonClass defendingDelt, BattleState state, bool isPlayerAttacking)
    {
        float levelDamage = (((2 * (float)attackingDelt.level) + 10)) / 250;
        float atkDefModifier = 1;
        float otherMods = 1;

        // Determine damage based on attacker and defender stats
        if (movType == moveType.PowerAtk)
        {
            atkDefModifier = (attackingDelt.Power + state.GetStatAddition(isPlayerAttacking, DeltStat.Power)) / 
                (defendingDelt.Courage + state.GetStatAddition(!isPlayerAttacking, DeltStat.Courage));
        }
        else // is moveType.TruthAtk
        {
            atkDefModifier = (attackingDelt.Truth + state.GetStatAddition(isPlayerAttacking, DeltStat.Truth)) / 
                (defendingDelt.Faith + state.GetStatAddition(!isPlayerAttacking, DeltStat.Faith));
        }

        // Extra damage if move is same major as Delt
        if (majorType == attackingDelt.deltdex.major1 || majorType == attackingDelt.deltdex.major2)
        {
            otherMods = 1.5f;
        }

        float rawDamage = ((levelDamage * atkDefModifier * damage) + 2) * otherMods;
        return rawDamage * EffectivenessAgainst(defendingDelt);
    }
}

[System.Serializable]
public class buffTuple {
	public bool isBuff;
	public buffType buffT;
	public byte buffAmount;
}

public enum moveType {
	TruthAtk,
	PowerAtk,
	Buff,
	Debuff,
	Status,
	Block
}

public enum buffType {
	None,
	Heal,
	Truth,
	Courage,
	Faith,
	Power,
	ChillToPull
}

public enum statusType {
	None,
	Plagued, // Poison -> Soap
	Indebted, // Cursed -> Daddy Check
	Suspended, // Frozen -> Humility
	Roasted, // Burned -> Ice
	Asleep, // Sleep -> Airhorn
	High, // Paralyzed -> Dutch
	Drunk, // Confused -> Deltialyte
	DA, // Fainted
	All // Just for items to cure all statuses
}

public enum Effectiveness
{
    Ineffective,
    VeryWeak,
    Weak,
    Average,
    Strong,
    VeryStrong
}