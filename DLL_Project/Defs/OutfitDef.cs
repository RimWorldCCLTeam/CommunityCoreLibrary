using Verse;
using CommunityCoreLibrary.Detour;

namespace CommunityCoreLibrary
{

    public class OutfitDef : Def
    {
        #region XML Data

        public ThingFilter                          filter              = new ThingFilter();

        #endregion

        public static OutfitDef                     Named( string defName )
        {
            return DefDatabase<OutfitDef>.GetNamed(defName);
        }

        public override void                        ResolveReferences()
        {
            base.ResolveReferences();

            _OutfitDatabase.OutfitDefs.Add( this );
        }
    }
}
