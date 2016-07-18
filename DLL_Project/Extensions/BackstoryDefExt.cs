using Verse;

namespace CommunityCoreLibrary
{
    public static class BackstoryDefExt
    {
        public static string UniqueSaveKey(this BackstoryDef def)
        {
            if (def.saveKeyIdentifier.NullOrEmpty())
                return "CustomBackstory_" + def.defName;
            else
                return def.saveKeyIdentifier + "_" + def.defName;
        }
    }
}
