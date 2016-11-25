using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public class PawnBioDef : Def
    {

        #region XML Data

        public BackstoryDef             childhoodDef;
        public BackstoryDef             adulthoodDef;
        public string                   firstName;
        public string                   nickName = "";
        public string                   lastName;
        public GenderPossibility        gender = GenderPossibility.Either;

        #endregion

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            #region Error Checking

            if( string.IsNullOrEmpty( firstName ) )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "'{0}' has null or empty firstname", this.defName ),
                    "PawnBioDef"
                );
                return;
            }
            if( !string.IsNullOrEmpty( lastName ) )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "'{0}' has null or empty lastname", this.defName ),
                    "PawnBioDef"
                );
                return;
            }
            if( childhoodDef == null )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "'{0}' has null or empty childhoodDef", this.defName ),
                    "PawnBioDef"
                );
                return;
            }
            if( adulthoodDef == null )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "'{0}' has null or empty adulthoodDef", this.defName ),
                    "PawnBioDef"
                );
                return;
            }

            var childBackstory = BackstoryDatabase.GetWithKey( this.childhoodDef.UniqueSaveKey() );
            if( childBackstory != null )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Could not resolve childhoodDef '{0}' in '{1}'", childhoodDef.defName, this.defName ),
                    "PawnBioDef"
                );
                return;
            }

            var adultBackstory = BackstoryDatabase.GetWithKey( this.adulthoodDef.UniqueSaveKey() );
            if( adultBackstory != null )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Could not resolve adulthoodDef '{0}' in '{1}'", adulthoodDef.defName, this.defName ),
                    "PawnBioDef"
                );
                return;
            }
            #endregion

            var pawnBio = new PawnBio();
            pawnBio.gender                  = this.gender;
            pawnBio.name                    = new NameTriple( firstName, nickName, lastName );

            pawnBio.childhood               = childBackstory;
            pawnBio.childhood.shuffleable   = false;
            pawnBio.childhood.slot          = BackstorySlot.Childhood;

            pawnBio.adulthood               = adultBackstory;
            pawnBio.adulthood.shuffleable   = false;
            pawnBio.adulthood.slot          = BackstorySlot.Adulthood;

            pawnBio.name.ResolveMissingPieces();

            bool configErrors = false;
            string configErrorList = string.Empty;
            foreach( var s in pawnBio.ConfigErrors() )
            {
                configErrorList += string.Format( "\n\t{0}", s );
                configErrors = true;
            }
            if( configErrors )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "{0} has error(s):{1}", this.defName, configErrorList ),
                    "PawnBioDef"
                );
                return;
            }

            if( !SolidBioDatabase.allBios.Contains( pawnBio ) )
            {
                SolidBioDatabase.allBios.Add( pawnBio );
            }
        }
    }
}
