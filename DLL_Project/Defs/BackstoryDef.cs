using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CommunityCoreLibrary
{
    public class BackstoryDef : Def
    {

        #region XML Data

        public string                               baseDescription;
        public BodyType                             bodyTypeGlobal      = BodyType.Undefined;
        public BodyType                             bodyTypeMale        = BodyType.Male;
        public BodyType                             bodyTypeFemale      = BodyType.Female;
        public string                               Title;
        public string                               TitleShort;
        public BackstorySlot                        slot                = BackstorySlot.Adulthood;
        public bool                                 shuffleable         = true;
        public bool                                 addToDatabase       = true;
        public List<WorkTags>                       workAllows          = new List<WorkTags>();
        public List<WorkTags>                       workDisables        = new List<WorkTags>();
        public List<BackstoryDefSkillListItem>      skillGains          = new List<BackstoryDefSkillListItem>();
        public List<string>                         spawnCategories     = new List<string>();
        public List<TraitEntry>                     forcedTraits        = new List<TraitEntry>();
        public List<TraitEntry>                     disallowedTraits    = new List<TraitEntry>();
        public string                               saveKeyIdentifier;

        #endregion

        public static BackstoryDef Named( string defName )
        {
            return DefDatabase<BackstoryDef>.GetNamed( defName );
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            #region Error Checking

            if( !this.addToDatabase )
            {
                return;
            }
            if( BackstoryDatabase.allBackstories.ContainsKey( this.UniqueSaveKey() ) )
            {
                return;
            }

            if( this.Title.NullOrEmpty() )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "'{0}' has an empty Title.", this.defName ),
                    "BackstoryDef"
                );
                return;
            }

            if( spawnCategories.NullOrEmpty() )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "'{0}' doesn't have any spawn categories defined.", this.defName ),
                    "BackstoryDef"
                );
                return;
            }

            #endregion

            var backStory = new Backstory();

            backStory.SetTitle( Title );

            if( !TitleShort.NullOrEmpty() )
            {
                backStory.SetTitleShort( TitleShort );
            }
            else
            {
                backStory.SetTitleShort( backStory.Title );
            }

            if( !baseDescription.NullOrEmpty() )
            {
                backStory.baseDesc = baseDescription;
            }
            else
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "'{0}' has an empty description.", this.defName ),
                    "BackstoryDef"
                );
                backStory.baseDesc = "Empty.";
            }

            backStory.bodyTypeGlobal        = bodyTypeGlobal;
            backStory.bodyTypeMale          = bodyTypeMale;
            backStory.bodyTypeFemale        = bodyTypeFemale;

            backStory.slot                  = slot;

            backStory.shuffleable           = shuffleable;
            backStory.spawnCategories       = spawnCategories;

            if( workAllows.Count > 0 )
            {
                foreach( WorkTags current in Enum.GetValues( typeof( WorkTags ) ) )
                {
                    if( !workAllows.Contains( current ) )
                    {
                        backStory.workDisables |= current;
                    }
                }
            }
            else if( workDisables.Count > 0 )
            {
                foreach( var tag in workDisables )
                {
                    backStory.workDisables |= tag;
                }
            }
            else
            {
                backStory.workDisables = WorkTags.None;
            }

            backStory.skillGains = skillGains.ToDictionary( i => i.defName, i => i.amount );

            if( forcedTraits.Count > 0 )
            {
                backStory.forcedTraits = new List<TraitEntry>();
                foreach( var trait in forcedTraits )
                {
                    var newTrait = new TraitEntry( trait.def, trait.degree );
                    backStory.forcedTraits.Add( newTrait );
                }
            }

            if( disallowedTraits.Count > 0 )
            {
                backStory.disallowedTraits = new List<TraitEntry>();
                foreach( var trait in disallowedTraits )
                {
                    var newTrait = new TraitEntry( trait.def, trait.degree );
                    backStory.disallowedTraits.Add( newTrait );
                }
            }

            backStory.ResolveReferences();
            backStory.PostLoad();
            backStory.identifier = this.UniqueSaveKey();

            bool configErrors = false;
            string configErrorList = string.Empty;
            foreach( var s in backStory.ConfigErrors( false ) )
            {
                configErrorList += string.Format( "\n\t{0}", s );
                configErrors = true;
            }
            if( configErrors )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "{0} has error(s):{1}", this.defName, configErrorList ),
                    "BackstoryDef"
                );
                return;
            }

            BackstoryDatabase.AddBackstory( backStory );
            //CCL_Log.Message("Added " + this.UniqueSaveKey() + " backstory", "BackstoryDef");

        }
    }
}
