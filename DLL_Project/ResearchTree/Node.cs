// ResearchTree/Node.cs
//
// Copyright Karel Kroeze, 2015.
//
// Created 2015-12-28 17:55

using CommunityCoreLibrary;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.ResearchTree
{
    public enum LockedState
    {
        notLockedOut,
        willBeLockedOut,
        LockedOut
    }

    public class Node
    {
        #region Fields

        public List<Node>             Children         = new List<Node>();
        public int                    Depth;
        public string                 Genus;
        public List<Node>             Locks            = new List<Node>();
        public List<Node>             Parents          = new List<Node>();
        public IntVec2                Pos;
        public ResearchProjectDef     Research;
        public Tree                   Tree;

        private const float           LabSize          = 30f;
        private const float           Offset           = 2f;

        private static MainTabWindow_ModHelp _helpWindow = DefDatabase<MainTabDef>.GetNamed( "CCL_ModHelp", false ).Window as MainTabWindow_ModHelp;

        private bool                  _isLockedOut     = false,
                                      _willBeLockedOut = false;

        private bool                  _largeLabel      = false;
        private Vector2               _left            = Vector2.zero;

        private Rect                  _queueRect,
                                      _rect,
                                      _labelRect,
                                      _costLabelRect,
                                      _costIconRect,
                                      _iconsRect;

        private bool                  _rectSet;
        private Vector2               _right           = Vector2.zero;

        #endregion Fields

        #region Constructors

        public Node( ResearchProjectDef research )
        {
            Research = research;

            // get the Genus, this is the research family name, and will be used to group research together.
            // First see if we have a ":" in the name
            List<string> parts = research.LabelCap.Split( ":".ToCharArray() ).ToList();
            if ( parts.Count > 1 )
            {
                Genus = parts.First();
            }
            else
            {
                parts = research.LabelCap.Split( " ".ToCharArray() ).ToList();

                // otherwise, strip the last word (intended to catch 1,2,3/ I,II,III,IV suffixes)
                if ( parts.Count > 1 )
                {
                    parts.Remove( parts.Last() );
                }
                Genus = string.Join( " ", parts.ToArray() );
            }
            Parents = new List<Node>();
            Children = new List<Node>();
        }

        #endregion Constructors

        #region Properties

        public Rect CostIconRect
        {
            get
            {
                if ( !_rectSet )
                {
                    CreateRects();
                }
                return _costIconRect;
            }
        }

        public Rect CostLabelRect
        {
            get
            {
                if ( !_rectSet )
                {
                    CreateRects();
                }
                return _costLabelRect;
            }
        }

        public Rect IconsRect
        {
            get
            {
                if ( !_rectSet )
                {
                    CreateRects();
                }
                return _iconsRect;
            }
        }

        public Rect LabelRect
        {
            get
            {
                if ( !_rectSet )
                {
                    CreateRects();
                }
                return _labelRect;
            }
        }

        /// <summary>
        /// Middle of left node edge
        /// </summary>
        public Vector2 Left
        {
            get
            {
                if ( _left == Vector2.zero )
                {
                    _left = new Vector2( Pos.x * ( Settings.NodeSize.x + Settings.NodeMargins.x ) + Offset,
                                         Pos.z * ( Settings.NodeSize.y + Settings.NodeMargins.y ) + Offset + Settings.NodeSize.y / 2 );
                }
                return _left;
            }
        }

        public LockedState LockedState
        {
            get
            {
                if ( _isLockedOut )
                    return LockedState.LockedOut;
                if ( _willBeLockedOut )
                    return LockedState.willBeLockedOut;
                return LockedState.notLockedOut;
            }
        }

        /// <summary>
        /// Tag UI Rect
        /// </summary>
        public Rect QueueRect
        {
            get
            {
                if ( !_rectSet )
                {
                    CreateRects();
                }
                return _queueRect;
            }
        }

        /// <summary>
        /// Static UI rect for this node
        /// </summary>
        public Rect Rect
        {
            get
            {
                if ( !_rectSet )
                {
                    CreateRects();
                }
                return _rect;
            }
        }

        /// <summary>
        /// Middle of right node edge
        /// </summary>
        public Vector2 Right
        {
            get
            {
                if ( _right == Vector2.zero )
                {
                    _right = new Vector2( Pos.x * ( Settings.NodeSize.x + Settings.NodeMargins.x ) + Offset + Settings.NodeSize.x,
                                          Pos.z * ( Settings.NodeSize.y + Settings.NodeMargins.y ) + Offset + Settings.NodeSize.y / 2 );
                }
                return _right;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determine the closest tree by moving along parents and then children until a tree has been found. Returns first tree encountered, or NULL.
        /// </summary>
        /// <returns></returns>
        public Tree ClosestTree()
        {
            // go up through all Parents until we find a parent that is in a Tree
            Queue<Node> parents = new Queue<Node>();
            parents.Enqueue( this );

            while ( parents.Count > 0 )
            {
                Node current = parents.Dequeue();
                if ( current.Tree != null )
                {
                    return current.Tree;
                }

                // otherwise queue up the Parents to be checked
                foreach ( Node parent in current.Parents )
                {
                    parents.Enqueue( parent );
                }
            }

            // if that didn't work, try seeing if a child is in a Tree (unlikely, but whateva).
            Queue<Node> children = new Queue<Node>();
            children.Enqueue( this );

            while ( children.Count > 0 )
            {
                Node current = children.Dequeue();
                if ( current.Tree != null )
                {
                    return current.Tree;
                }

                // otherwise queue up the Children to be checked.
                foreach ( Node child in current.Children )
                {
                    children.Enqueue( child );
                }
            }

            // finally, if nothing stuck, return null
            return null;
        }

        /// <summary>
        /// Set all prerequisites as parents of this node, and for each parent set this node as a child.
        /// </summary>
        public void CreateLinks()
        {
            // 'vanilla' prerequisites
            foreach ( ResearchProjectDef prerequisite in Research.prerequisites )
            {
                if ( prerequisite != Research )
                {
                    var parent = ResearchTree.Forest.FirstOrDefault( node => node.Research == prerequisite );
                    if ( parent != null )
                    {
                        Parents.Add( parent );
                    }
                }
            }

            // CCL advanced research, inclusive unlocks.
            foreach ( AdvancedResearchDef ard in DefDatabase<AdvancedResearchDef>.AllDefsListForReading
                .Where( ard => ard.IsResearchToggle &&
                               !ard.HideDefs &&
                               ard.effectedResearchDefs.Contains( Research ) ) )
            {
                foreach ( ResearchProjectDef prerequisite in ard.researchDefs )
                {
                    if ( prerequisite != Research )
                    {
                        var parent = ResearchTree.Forest.FirstOrDefault( node => node.Research == prerequisite );
                        if ( parent != null )
                        {
                            Parents.Add( parent );
                        }
                    }
                }
            }

            // CCL advanced research, locks.
            foreach ( AdvancedResearchDef ard in DefDatabase<AdvancedResearchDef>.AllDefsListForReading
                .Where( ard => ard.IsResearchToggle &&
                   ard.HideDefs &&
                   ard.researchDefs.Contains( Research ) ) )
            {
                foreach ( ResearchProjectDef locked in ard.effectedResearchDefs )
                {
                    if ( locked != Research )
                    {
                        var lockedNode = ResearchTree.Forest.FirstOrDefault( node => node.Research == locked );
                        if ( lockedNode != null )
                        {
                            Locks.Add( lockedNode );
                        }
                    }
                }
            }

            foreach ( Node parent in Parents )
            {
                parent.Children.Add( this );
            }
        }

#if DEVELOPER
        /// <summary>
        /// Prints debug information.
        /// </summary>
        public string Debug()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine( Research.LabelCap + " (" + Depth + ", " + Genus + "):" );
            text.AppendLine( "- Parents" );
            foreach ( Node parent in Parents )
            {
                text.AppendLine( "-- " + parent.Research.LabelCap );
            }
            text.AppendLine( "- Children" );
            foreach ( Node child in Children )
            {
                text.AppendLine( "-- " + child.Research.LabelCap );
            }
            text.AppendLine( "" );
            return text.ToString();
        }
#endif

        /// <summary>
        /// Draw the node, including interactions.
        /// </summary>
        public void Draw()
        {
            // set color
            GUI.color = !Research.ResearchPrereqsFulfilled ? Tree.GreyedColor : Tree.MediumColor;
            if ( LockedState == LockedState.LockedOut )
                GUI.color = new Color( .4f, .4f, .4f );
            bool prereqLocks = false;

            // cop out if off-screen
            Rect screen = new Rect( MainTabWindow_ResearchTree._scrollPosition.x, MainTabWindow_ResearchTree._scrollPosition.y, Screen.width, Screen.height - 35 );
            if ( Rect.xMin > screen.xMax ||
                Rect.xMax < screen.xMin ||
                Rect.yMin > screen.yMax ||
                Rect.yMax < screen.yMin )
            {
                return;
            }

            // mouseover highlights
            if ( Mouse.IsOver( Rect ) && LockedState != LockedState.LockedOut )
            {
                // active button
                GUI.DrawTexture( Rect, ResearchTree.ButtonActive );

                // highlight this and all prerequisites if research not completed
                if ( !Research.IsFinished )
                {
                    List<Node> prereqs = GetMissingRequiredRecursive();
                    Highlight( GenUI.MouseoverColor, true, false );
                    if ( !Locks.NullOrEmpty() )
                    {
                        foreach ( Node locked in Locks )
                        {
                            locked.Highlight( Color.red, false, true );
                        }
                    }
                    foreach ( Node prerequisite in prereqs )
                    {
                        prerequisite.Highlight( GenUI.MouseoverColor, true, false );
                        if ( !prerequisite.Locks.NullOrEmpty() )
                        {
                            prereqLocks = true;
                            foreach ( Node locked in prerequisite.Locks )
                            {
                                locked.Highlight( Color.red, false, false );
                            }
                        }
                    }
                }
                else // highlight followups
                {
                    foreach ( Node child in Children )
                    {
                        MainTabWindow_ResearchTree.highlightedConnections.Add( new Pair<Node, Node>( this, child ) );
                        child.Highlight( GenUI.MouseoverColor, false, false );

                        if ( !child.Locks.NullOrEmpty() )
                        {
                            foreach ( Node locked in child.Locks )
                            {
                                locked.Highlight( Color.red, false, false );
                            }
                        }
                    }
                }
            }
            // if not moused over, just draw the default button state
            else
            {
                GUI.DrawTexture( Rect, ResearchTree.Button );
            }

            // grey out center to create a progress bar effect, completely greying out research not started.
            if ( !Research.IsFinished && LockedState != LockedState.LockedOut )
            {
                Rect progressBarRect = Rect.ContractedBy( 2f );
                GUI.color = Tree.GreyedColor;
                progressBarRect.xMin += Research.PercentComplete * progressBarRect.width;
                GUI.DrawTexture( progressBarRect, BaseContent.WhiteTex );
            }

            // draw the research label
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = false;
            Text.Font = _largeLabel ? GameFont.Tiny : GameFont.Small;
            Widgets.Label( LabelRect, Research.LabelCap );

            // draw research cost and icon
            Text.Anchor = TextAnchor.UpperRight;
            Text.Font = GameFont.Small;
            if ( LockedState == LockedState.LockedOut )
            {
                Widgets.Label( CostLabelRect, "Fluffy.ResearchTree.LockedOut".Translate() );
            }
            else
            {
                Widgets.Label( CostLabelRect, Research.totalCost.ToStringByStyle( ToStringStyle.Integer ) );
                GUI.DrawTexture( CostIconRect, ResearchTree.ResearchIcon );
            }
            Text.WordWrap = true;

            // attach description and further info to a tooltip
            TooltipHandler.TipRegion( Rect, GetResearchTooltipString() ); // new TipSignal( GetResearchTooltipString(), Settings.TipID ) );

            // draw unlock icons
            List<Pair<Def, string>> unlocks = Research.GetUnlockDefsAndDescs();
            for ( int i = 0; i < unlocks.Count; i++ )
            {
                Rect iconRect = new Rect( IconsRect.xMax - ( i + 1 ) * ( Settings.IconSize.x + 4f ),
                                          IconsRect.yMin + ( IconsRect.height - Settings.IconSize.y ) / 2f,
                                          Settings.IconSize.x,
                                          Settings.IconSize.y );

                if ( iconRect.xMin - Settings.IconSize.x < IconsRect.xMin &&
                    i + 1 < unlocks.Count )
                {
                    // stop the loop if we're about to overflow and have 2 or more unlocks yet to print.
                    iconRect.x = IconsRect.x + 4f;
                    ResearchTree.MoreIcon.DrawFittedIn( iconRect );
                    string tip = string.Join( "\n", unlocks.GetRange( i, unlocks.Count - i ).Select( p => p.Second ).ToArray() );
                    TooltipHandler.TipRegion( iconRect, tip ); // new TipSignal( tip, Settings.TipID, TooltipPriority.Pawn ) );
                    break;
                }

                // draw icon
                unlocks[i].First.DrawColouredIcon( iconRect );

                // tooltip
                TooltipHandler.TipRegion( iconRect, unlocks[i].Second ); // new TipSignal( unlocks[i].Second, Settings.TipID, TooltipPriority.Pawn ) );
            }

            // draw a big warning label if about to be locked.
            if ( LockedState == LockedState.willBeLockedOut )
            {
                Color color = GUI.color;
                GUI.color = Color.red;
                GUI.DrawTexture( Rect, ResearchTree.WarningIcon, ScaleMode.ScaleToFit );
                GUI.color = color;
            }

            // if clicked and not yet finished, queue up this research and all prereqs.
            if ( LockedState != LockedState.LockedOut && Widgets.InvisibleButton( Rect ) )
            {
                // LMB is queue operations, RMB is info
                if ( Event.current.button == 0 && !Research.IsFinished )
                {
                    if ( !Queue.IsQueued( this ) )
                    {
                        if ( prereqLocks )
                        {
                            foreach ( Node node in GetMissingRequiredRecursive() )
                            {
                                if ( !node.Locks.NullOrEmpty() && !( Event.current.shift && Queue.IsQueued( node ) ) )
                                {
                                    Messages.Message( "Fluffy.ResearchTree.CannotQueueXLocksY".Translate( node.Research.LabelCap,
                                                        string.Join( ", ", node.Locks.Select( n => n.Research.LabelCap ).ToArray() ) ) + " " +
                                                        "Fluffy.ResearchTree.CannotQueueOneByOne".Translate(),
                                                      MessageSound.RejectInput );
                                    return;
                                }
                            }
                        }
                        // if shift is held, add to queue, otherwise replace queue
                        Queue.EnqueueRange( GetMissingRequiredRecursive().Concat( new List<Node>( new[] { this } ) ), Event.current.shift );
                    }
                    else
                    {
                        Queue.Dequeue( this );
                    }
                }
                else if ( Event.current.button == 1 )
                {
                    // right click links to CCL help def.
                    _helpWindow.JumpTo( Research.GetHelpDef() );
                }
            }
        }

        /// <summary>
        /// Get recursive list of all incomplete prerequisites
        /// </summary>
        /// <returns>List<Node> prerequisites</Node></returns>
        public List<Node> GetMissingRequiredRecursive()
        {
            List<Node> parents = new List<Node>( Parents.Where( node => !node.Research.IsFinished ) );
            List<Node> allParents = new List<Node>( parents );
            foreach ( Node current in parents )
            {
                if ( current.LockedState != LockedState.LockedOut )
                {
                    // check advanced researches
                    List<AdvancedResearchDef> advancedResearches = Controller.Data.AdvancedResearchDefs.Where( ard => (
                        ( ard.IsResearchToggle )&&
                        ( !ard.IsLockedOut() )&&
                        ( !ard.HideDefs )&&
                        ( ard.effectedResearchDefs.Contains( current.Research ) )
                    ) ).ToList();

                    if ( !advancedResearches.NullOrEmpty() )
                    {
                        Dictionary<Node, List<Node>> options = new Dictionary<Node, List<Node>>();
                        foreach ( ResearchProjectDef option in advancedResearches.SelectMany( ard => ard.researchDefs ).Where( rd => rd.Node() != null ) )
                        {
                            options.Add( option.Node(), option.Node().GetMissingRequiredRecursive() );
                        }
                        allParents.AddRange( options.MinBy( option => option.Value.Count ).Value );
                    }
                    else
                    {
                        allParents.AddRange( current.GetMissingRequiredRecursive() );
                    }
                }
            }
            return allParents.Distinct().ToList();
        }

        /// <summary>
        /// Draw highlights around node, and optionally highlight links to parents/children of this node.
        /// </summary>
        /// <param name="color">color to use</param>
        /// <param name="linkParents">should links to parents be drawn?</param>
        /// <param name="linkChildren">should links to children be drawn?</param>
        public void Highlight( Color color, bool linkParents, bool linkChildren )
        {
            GUI.color = color;
            Widgets.DrawBox( Rect.ContractedBy( -2f ), 2 );
            GUI.color = Color.white;
            if ( linkParents )
            {
                foreach ( Node parent in Parents )
                {
                    MainTabWindow_ResearchTree.highlightedConnections.Add( new Pair<Node, Node>( parent, this ) );
                }
            }
            if ( linkChildren )
            {
                foreach ( Node child in Children )
                {
                    MainTabWindow_ResearchTree.highlightedConnections.Add( new Pair<Node, Node>( this, child ) );
                }
            }
        }

        public void Notify_LockedOut( bool state )
        {
            _isLockedOut = state;
            Research.ExclusiveDescendants().Select( res => res.Node() ).ToList().ForEach( node => node.Notify_LockedOut( state ) );
        }

        public void Notify_WillBeLockedOut( bool state )
        {
            _willBeLockedOut = state;
            Research.ExclusiveDescendants().Select( res => res.Node() ).ToList().ForEach( node => node.Notify_WillBeLockedOut( state ) );
        }

        /// <summary>
        /// Recursively determine the depth of this node.
        /// </summary>
        public void SetDepth()
        {
            List<Node> level = new List<Node>();
            level.Add( this );
            while ( level.Count > 0 &&
                    level.Any( node => node.Parents.Count > 0 ) )
            {
                // has any parent, increment level.
                Depth++;

                // set level to next batch of distinct Parents, where Parents may not be itself.
                level = level.SelectMany( node => node.Parents ).Distinct().Where( node => node != this ).ToList();

                // stop infinite recursion with loops of size greater than 2
                if ( Depth > 100 )
                {
                    Log.Error( Research.LabelCap +
                               " has more than 100 levels of prerequisites. Is the Research Tree defined as a loop?" );
                }
            }
        }

        public override string ToString()
        {
            return this.Research.LabelCap + this.Pos;
        }

        private void CreateRects()
        {
            // main rect
            _rect = new Rect( Pos.x * ( Settings.NodeSize.x + Settings.NodeMargins.x ) + Offset,
                              Pos.z * ( Settings.NodeSize.y + Settings.NodeMargins.y ) + Offset,
                              Settings.NodeSize.x,
                              Settings.NodeSize.y );

            // queue rect
            _queueRect = new Rect( _rect.xMax - LabSize / 2f,
                                 _rect.yMin + ( _rect.height - LabSize ) / 2f,
                                 LabSize,
                                 LabSize );

            // label rect
            _labelRect = new Rect( _rect.xMin + 6f,
                                   _rect.yMin + 3f,
                                   _rect.width * 2f / 3f - 6f,
                                   _rect.height * .5f - 3f );

            // research cost rect
            _costLabelRect = new Rect( _rect.xMin + _rect.width * 2f / 3f,
                                  _rect.yMin + 3f,
                                  _rect.width * 1f / 3f - 16f - 3f,
                                  _rect.height * .5f - 3f );

            // research icon rect
            _costIconRect = new Rect( _costLabelRect.xMax,
                                      _rect.yMin + ( _costLabelRect.height - 16f ) / 2,
                                      16f,
                                      16f );

            // icon container rect
            _iconsRect = new Rect( _rect.xMin,
                                   _rect.yMin + _rect.height * .5f,
                                   _rect.width,
                                   _rect.height * .5f );

            // see if the label is too big
            _largeLabel = Text.CalcHeight( Research.LabelCap, _labelRect.width ) > _labelRect.height;

            // done
            _rectSet = true;
        }

        /// <summary>
        /// Creates text version of research description and additional unlocks/prereqs/etc sections.
        /// </summary>
        /// <returns>string description</returns>
        private string GetResearchTooltipString()
        {
            // start with the descripton
            StringBuilder text = new StringBuilder();
            text.AppendLine( Research.description );
            text.AppendLine();

            if ( LockedState != LockedState.LockedOut )
            {
                if ( LockedState == LockedState.willBeLockedOut )
                {
                    text.AppendLine( "Fluffy.ResearchTree.WillBeLockedOut".Translate() );
                }
                if ( Queue.IsQueued( this ) )
                {
                    text.AppendLine( "Fluffy.ResearchTree.LClickRemoveFromQueue".Translate() );
                }
                else
                {
                    text.AppendLine( "Fluffy.ResearchTree.LClickReplaceQueue".Translate() );
                    text.AppendLine( "Fluffy.ResearchTree.SLClickAddToQueue".Translate() );
                }
                text.AppendLine( "Fluffy.ResearchTree.RClickForDetails".Translate() );
            }
            else
            {
                text.AppendLine( "Fluffy.ResearchTree.LockedOut".Translate().ToUpper() );
            }
            return text.ToString();
        }

        #endregion Methods
    }
}