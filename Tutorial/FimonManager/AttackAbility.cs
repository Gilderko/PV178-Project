﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FImons
{   
    public class AttackAbility : Ability
    {
        public AttackAbility(ulong id,AbilityType abilityType, ElementalTypes abilityForm, string name, string description, int cost, int upperDamage, int lowerDamage
            ,int hitChance, int critChance) : base(id ,abilityType, abilityForm,name,description,cost)
        {
            DamageValueUpper = upperDamage;
            DamageValueLower = lowerDamage;
            HitChance = hitChance;
            CritChance = critChance;
        }

        [BsonElement("damage_value_upper")]
        public int DamageValueUpper { get; set; }

        [BsonElement("damage_value_lower")]
        public int DamageValueLower { get; set; }

        [BsonElement("hit_chance")]
        public int HitChance { get; set; }

        [BsonElement("crit_chance")]
        public int CritChance { get; set; }

        public string GetDescriptionForMessage()
        {
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization \nDamage range: {DamageValueLower} - {DamageValueUpper}" +
                $"\nHit chance: {HitChance}\nCritical hit chance is: {CritChance}";
            return data;
        }
    }
}
