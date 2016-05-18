using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse.AI;
using Verse;
using System.Reflection;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _BiomeDef
    {
        
        internal static FieldInfo _wildAnimals;
        internal static FieldInfo _wildPlants;
        internal static FieldInfo _diseases;

        internal static FieldInfo _cachedAnimalCommonalities;
        internal static FieldInfo _cachedPlantCommonalities;
        internal static FieldInfo _cachedDiseaseCommonalities;

        #region Reflected Methods

        internal static List<BiomeAnimalRecord> wildAnimals(this BiomeDef obj)
        {
            if (_wildAnimals == null)
            {
                _wildAnimals = typeof(BiomeDef).GetField("wildAnimals", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return (List<BiomeAnimalRecord>)_wildAnimals.GetValue(obj);
        }

        internal static List<BiomePlantRecord> wildPlants(this BiomeDef obj)
        {
            if (_wildPlants == null)
            {
                _wildPlants = typeof(BiomeDef).GetField("wildPlants", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return (List<BiomePlantRecord>)_wildPlants.GetValue(obj);
        }

        internal static List<BiomeDiseaseRecord> diseases(this BiomeDef obj)
        {
            if (_diseases == null)
            {
                _diseases = typeof(BiomeDef).GetField("diseases", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return (List<BiomeDiseaseRecord>)_diseases.GetValue(obj);
        }

        internal static Dictionary<PawnKindDef, float> cachedAnimalCommonalities(this BiomeDef obj)
        {
            if (_cachedAnimalCommonalities == null)
            {
                _cachedAnimalCommonalities = typeof(BiomeDef).GetField("cachedAnimalCommonalities", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return (Dictionary<PawnKindDef, float>)_cachedAnimalCommonalities.GetValue(obj);
        }

        internal static Dictionary<ThingDef, float> cachedPlantCommonalities(this BiomeDef obj)
        {
            if (_cachedPlantCommonalities == null)
            {
                _cachedPlantCommonalities = typeof(BiomeDef).GetField("cachedPlantCommonalities", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return (Dictionary<ThingDef, float>)_cachedPlantCommonalities.GetValue(obj);
        }

        internal static Dictionary<IncidentDef, float> cachedDiseaseCommonalities(this BiomeDef obj)
        {
            if (_cachedDiseaseCommonalities == null)
            {
                _cachedDiseaseCommonalities = typeof(BiomeDef).GetField("cachedDiseaseCommonalities", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            return (Dictionary<IncidentDef, float>)_cachedDiseaseCommonalities.GetValue(obj);
        }

        #endregion

        #region Detoured Methods

        internal static float _CommonalityOfAnimal(this BiomeDef _this, PawnKindDef animalDef)
        {
            var cachedAnimalCommonalities = _this.cachedAnimalCommonalities();
            if (cachedAnimalCommonalities == null)
            {
                cachedAnimalCommonalities = new Dictionary<PawnKindDef, float>();
                for (int index = 0; index < _this.wildAnimals().Count; ++index)
                    cachedAnimalCommonalities.Add(_this.wildAnimals()[index].animal, _this.wildAnimals()[index].commonality);
                foreach (PawnKindDef current in DefDatabase<PawnKindDef>.AllDefs)
                {
                    if (current.RaceProps.wildBiomes != null)
                    {
                        for (int index = 0; index < current.RaceProps.wildBiomes.Count; ++index)
                        {
                            if (current.RaceProps.wildBiomes[index].biome.defName == _this.defName)
                            {
                                cachedAnimalCommonalities.Add(current.RaceProps.wildBiomes[index].animal, current.RaceProps.wildBiomes[index].commonality);
                            }
                        }
                    }
                }
                _this.cachedAnimalCommonalitiesSet(cachedAnimalCommonalities);
            }
            float num;
            if (cachedAnimalCommonalities.TryGetValue(animalDef, out num))
                return num;
            return 0.0f;
        }

        internal static float _CommonalityOfPlant(this BiomeDef _this, ThingDef plantDef)
        {
            var cachedPlantCommonalities = _this.cachedPlantCommonalities();
            if (cachedPlantCommonalities == null)
            {
                cachedPlantCommonalities = new Dictionary<ThingDef, float>();
                for (int i = 0; i < _this.wildPlants().Count; i++)
                {
                    cachedPlantCommonalities.Add(_this.wildPlants()[i].plant, _this.wildPlants()[i].commonality);
                }
                foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
                {
                    if (current.plant != null && current.plant.wildBiomes != null)
                    {
                        for (int index = 0; index < current.plant.wildBiomes.Count; ++index)
                        {
                            if (current.plant.wildBiomes[index].biome.defName == _this.defName)
                            {
                                cachedPlantCommonalities.Add(current.plant.wildBiomes[index].plant, current.plant.wildBiomes[index].commonality);
                            }
                        }
                    }
                }
                _this.cachedPlantCommonalitiesSet(cachedPlantCommonalities);
            }
            float result;
            if (cachedPlantCommonalities.TryGetValue(plantDef, out result))
            {
                return result;
            }
            return 0f;
        }

        internal static float _MTBDaysOfDisease(this BiomeDef _this, IncidentDef diseaseInc)
        {
            var cachedDiseaseCommonalities = _this.cachedDiseaseCommonalities();
            if (cachedDiseaseCommonalities == null)
            {
                cachedDiseaseCommonalities = new Dictionary<IncidentDef, float>();
                for (int i = 0; i < _this.diseases().Count; i++)
                {
                    cachedDiseaseCommonalities.Add(_this.diseases()[i].diseaseInc, _this.diseases()[i].mtbDays);
                }
                foreach (IncidentDef current in DefDatabase<IncidentDef>.AllDefs)
                {
                    if (current.diseaseBiomeRecords != null)
                    {
                        for (int index = 0; index < current.diseaseBiomeRecords.Count; ++index)
                        {
                            if (current.diseaseBiomeRecords[index].biome.defName == _this.defName)
                            {
                                cachedDiseaseCommonalities.Add(current.diseaseBiomeRecords[index].diseaseInc, current.diseaseBiomeRecords[index].mtbDays);
                            }
                        }
                    }
                }
                _this.cachedDiseaseCommonalitiesSet(cachedDiseaseCommonalities);
            }
            float result;
            if (cachedDiseaseCommonalities.TryGetValue(diseaseInc, out result))
            {
                return result;
            }
            return 9999999f;
        }

        #endregion

        internal static void cachedAnimalCommonalitiesSet(this BiomeDef obj, Dictionary<PawnKindDef, float> value)
        {
            if (_cachedAnimalCommonalities == null)
            {
                _cachedAnimalCommonalities = typeof(BiomeDef).GetField("cachedAnimalCommonalities", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            _cachedAnimalCommonalities.SetValue(obj, value);
        }

        internal static void cachedPlantCommonalitiesSet(this BiomeDef obj, Dictionary<ThingDef, float> value)
        {
            if (_cachedPlantCommonalities == null)
            {
                _cachedPlantCommonalities = typeof(BiomeDef).GetField("cachedPlantCommonalities", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            _cachedPlantCommonalities.SetValue(obj, value);
        }

        internal static void cachedDiseaseCommonalitiesSet(this BiomeDef obj, Dictionary<IncidentDef, float> value)
        {
            if (_cachedDiseaseCommonalities == null)
            {
                _cachedDiseaseCommonalities = typeof(BiomeDef).GetField("cachedDiseaseCommonalities", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            _cachedDiseaseCommonalities.SetValue(obj, value);
        }

    }

}
