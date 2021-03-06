﻿using System.Collections.Generic;
using System.Linq;
using LevelGenerator.Scripts.Exceptions;
using LevelGenerator.Scripts.Helpers;
using LevelGenerator.Scripts.Structure;
using UnityEngine;

namespace LevelGenerator.Scripts
{
    public class LevelGenerator : MonoBehaviour
    {
        /// <summary>
        /// LevelGenerator seed
        /// </summary>
        public int Seed;

        /// <summary>
        /// Container for all sections in hierarchy
        /// </summary>
        public Transform SectionContainer;

        /// <summary>
        /// Maximum level size measured in sections
        /// </summary>
        public int MaxLevelSize;

        /// <summary>
        /// Maximum allowed distance from the original section
        /// </summary>
        public int MaxAllowedOrder;

        /// <summary>
        /// Spawnable section prefabs
        /// </summary>
        public Section[] Sections;

        /// <summary>
        /// Spawnable dead ends
        /// </summary>
        public DeadEnd[] DeadEnds;

        /// <summary>
        /// Tags that will be taken into consideration when building the first section
        /// </summary>
        public string[] InitialSectionTags;

        /// <summary>
        /// Special section rules, limits and forces the amount of a specific tag
        /// </summary>
        public TagRule[] SpecialRules;

        protected List<Section> registeredSections = new List<Section>();

        public int LevelSize { get; private set; }
        public Transform Container => SectionContainer != null ? SectionContainer : transform;

        protected IEnumerable<Collider> RegisteredColliders => registeredSections.SelectMany(s => s.Bounds.Colliders).Union(DeadEndColliders);
        protected List<Collider> DeadEndColliders = new List<Collider>();
        protected bool HalfLevelBuilt => registeredSections.Count > LevelSize;

        public void GenerateLevel()
        {
            if (Seed != 0)
                RandomService.SetSeed(Seed);
            else
                Seed = RandomService.Seed;

            CheckRuleIntegrity();
            LevelSize = MaxLevelSize;
            CreateInitialSection();
            DeactivateBounds();
        }

        protected void Start()
        {
        }

        protected void CheckRuleIntegrity()
        {
            foreach (var ruleTag in SpecialRules.Select(r => r.Tag))
            {
                if (SpecialRules.Count(r => r.Tag.Equals(ruleTag)) > 1)
                    throw new InvalidRuleDeclarationException();
            }
        }

        protected void CreateInitialSection()
        {
            var section = Instantiate(PickSectionWithTag(InitialSectionTags));
            section.transform.SetParent(transform);
            section.Initialize(this, 0);
        }

        public void AddSectionTemplate() => Instantiate(Resources.Load("SectionTemplate"), Vector3.zero, Quaternion.identity);
        public void AddDeadEndTemplate() => Instantiate(Resources.Load("DeadEndTemplate"), Vector3.zero, Quaternion.identity);

        // public bool IsSectionValid(Bounds newSection, Bounds sectionToIgnore) =>
        //     !RegisteredColliders.Except(sectionToIgnore.Colliders).Any(c => c.bounds.Intersects(newSection.Colliders.First().bounds));

        public bool IsSectionValid(Bounds newSection, Bounds sectionToIgnore)
        {
            var colls = RegisteredColliders.Except(sectionToIgnore.Colliders).ToArray();
            var newSectionColls = newSection.Colliders.ToArray();

            for (int i = 0; i < colls.Count(); i++)
            {
                var c = colls[i];
                for (int j = 0; j < newSectionColls.Length; j++)
                {
                    if (c.bounds.Intersects(newSectionColls[j].bounds))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void RegisterNewSection(Section newSection)
        {
            registeredSections.Add(newSection);

            if (SpecialRules.Any(r => newSection.Tags.Contains(r.Tag)))
                SpecialRules.First(r => newSection.Tags.Contains(r.Tag)).PlaceRuleSection();

            LevelSize--;
        }

        public void RegistrerNewDeadEnd(IEnumerable<Collider> colliders) => DeadEndColliders.AddRange(colliders);

        public Section PickSectionWithTag(string[] tags)
        {
            if (RulesContainTargetTags(tags) && HalfLevelBuilt)
            {
                foreach (var rule in SpecialRules.Where(r => r.NotSatisfied))
                {
                    if (tags.Contains(rule.Tag))
                    {
                        return Sections.Where(x => x.Tags.Contains(rule.Tag)).PickOne();
                    }
                }
            }

            var pickedTag = PickFromExcludedTags(tags);
            try
            {
                return Sections.Where(x => x.Tags.Contains(pickedTag)).PickOne();
            }
            catch
            {
                Debug.LogError($"GENERATOR FAILED TO MAKE ROOM '{pickedTag}'");
                throw;
            }
        }

        protected string PickFromExcludedTags(string[] tags)
        {
            var tagsToExclude = SpecialRules.Where(r => r.Completed).Select(rs => rs.Tag);
            return tags.Except(tagsToExclude).PickOne();
        }

        protected bool RulesContainTargetTags(string[] tags) => tags.Intersect(SpecialRules.Where(r => r.NotSatisfied).Select(r => r.Tag)).Any();

        protected void DeactivateBounds()
        {
            foreach (var c in RegisteredColliders)
                c.enabled = false;
        }
    }
}