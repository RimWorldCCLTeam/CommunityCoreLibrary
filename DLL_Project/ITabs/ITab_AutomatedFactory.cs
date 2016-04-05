using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{
    // TODO: see other todos
    /*public class ITab_AutomatedFactory : ITab
    {

        private static readonly Vector2         WinSize;

        // TODO: see other todos
        //private List<ThingDef>                  products;
        //private bool[]                          allowed;
        //private TipSignal[]                     tips;

        // TODO:  Improve UI layout and how information is displayed
        // Currently Displays/Has options for:
        // + Factory name (parent.LabelCap)
        // + Current recipes ([dis]allow checkbox, ingredients (tooltip))

        #region Private Properties

        private Building_AutomatedFactory       SelectedFactory
        {
            get
            {
                Thing thing = Find.Selector.SingleSelectedThing;
                MinifiedThing minifiedThing = thing as MinifiedThing;
                if( minifiedThing != null )
                {
                    thing = minifiedThing.InnerThing;
                }
                if( thing == null )
                {
                    return (Building_AutomatedFactory) null;
                }
                return thing as Building_AutomatedFactory;
            }
        }

        #endregion

        #region Constructors

        static                                  ITab_AutomatedFactory()
        {
            WinSize = new Vector2( 400, 300 );
        }

        public                                  ITab_AutomatedFactory()
        {
            this.size = ITab_AutomatedFactory.WinSize;
            this.labelKey = "ITab_AutomatedFactory";
        }

        #endregion

        #region Class Overrides

        public override bool                    IsVisible
        {
            get
            {
                if( this.SelectedFactory != null )
                {
                    return true;
                }
                return false;
            }
        }

        public override void                    OnOpen()
        {
            products = SelectedFactory.AllProducts();
            allowed = new bool[ products.Count ];
            tips = new TipSignal[ products.Count ];
            for( int index = 0; index < products.Count; ++index )
            {
                var product = products[ index ];
                allowed[ index ] = SelectedFactory.GetAllowed( product );
                string tipText = "";
                var recipe = SelectedFactory.FindRecipeForProduct( product );
                if( recipe != null )
                {
                    foreach( var ingredient in recipe.ingredients )
                    {
                        if( !ingredient.filter.Summary.NullOrEmpty() )
                        {
                            tipText += recipe.IngredientValueGetter.BillRequirementsDescription( ingredient ) + "\n";
                        }
                    }
                }
                tips[ index ].text = tipText;
            }
        }

        protected override void                 FillTab()
        {
            Rect rect1 = GenUI.ContractedBy( new Rect( 0.0f, 0.0f, ITab_AutomatedFactory.WinSize.x, ITab_AutomatedFactory.WinSize.y ), 10f );
            Text.Font = GameFont.Medium;
            Widgets.Label( rect1, this.SelectedFactory.def.LabelCap );
            Rect rect2 = rect1;
            rect2.yMin += 35f;
            Vector2 vector = rect2.position;
            rect2.xMin += 20f;
            Text.Font = GameFont.Small;
            for( int index = 0; index < products.Count; ++index )
            {
                Widgets.Label( rect2, products[ index ].LabelCap );
                Widgets.Checkbox( vector, ref allowed[ index ], 16f );
                TooltipHandler.TipRegion( rect2, tips[ index ] );
                rect2.yMin += 20f;
                vector.y += 20f;
            }
        }

        public override void                    TabUpdate()
        {
            for( int index = 0; index < products.Count; ++index )
            {
                SelectedFactory.SetAllowed( products[ index ], allowed[ index ] );
            }
        }

        #endregion

    }*/

}