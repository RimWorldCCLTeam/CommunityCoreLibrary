using System;

using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _PreLoadUtility
    {

        internal static void _CheckVersionAndLoad( string path, ScribeMetaHeaderUtility.ScribeHeaderMode mode, Action loadAct )
        {
            bool mismatchWarnings;
            try
            {
                try
                {
                    Scribe.InitLoadingMetaHeaderOnly( path );
                }
                catch (Exception ex)
                {
                    Log.Warning(string.Concat(new object[4]
                    {
                        (object) "Exception loading ",
                        (object) path,
                        (object) ": ",
                        (object) ex
                    }));
                }
                ScribeMetaHeaderUtility.LoadGameDataHeader( mode, false );
                mismatchWarnings = ScribeMetaHeaderUtility.TryCreateDialogsForVersionMismatchWarnings( loadAct );
                CrossRefResolver.ResolveAllCrossReferences();
                PostLoadInitter.DoAllPostLoadInits();
            }
            catch
            {
                CrossRefResolver.Clear();
                PostLoadInitter.Clear();
                throw;
            }
            Controller.Data.ResetInjectionSubController();
            if( mismatchWarnings )
            {
                return;
            }
            loadAct();
        }

    }

}
